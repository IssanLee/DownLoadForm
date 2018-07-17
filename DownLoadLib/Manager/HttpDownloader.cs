using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownLoadLib
{
    public class HttpDownloader : IHttpDownloader
    {
        private static readonly int BufferSize = 4096;

        private Stopwatch stopwatch;
        private AsyncOperation operation;
        private DownloadState state;
        private int progress;

        /// <summary>
        /// 下载完成事件
        /// </summary>
        public event EventHandler DownloadCancelled;
        /// <summary>
        /// 下载取消事件
        /// </summary>
        public event EventHandler DownloadCompleted;
        /// <summary>
        /// 下载进度事件
        /// </summary>
        public event ProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// 下载文件的URL
        /// </summary>
        public string FileURL { get; }

        /// <summary>
        /// 下载文件的全路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 下载进度
        /// </summary>
        public int Progress
        {
            get { return progress; }
            private set
            {
                progress = value;
                operation.Post(new SendOrPostCallback(delegate
                {
                    if (DownloadProgressChanged != null)
                        DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(progress, SpeedInBytes));
                }
                ), null);
            }
        }

        /// <summary>
        /// 下载状态
        /// </summary>
        public DownloadState State
        {
            get { return state; }
            private set
            {
                state = value;
                if (state == DownloadState.Completed && DownloadCompleted != null)
                    operation.Post(new SendOrPostCallback(delegate
                    {
                        if (DownloadCompleted != null)
                            DownloadCompleted(this, EventArgs.Empty);
                    }), null);
                else if (state == DownloadState.Cancelled && DownloadCancelled != null)
                    operation.Post(new SendOrPostCallback(delegate
                    {
                        if (DownloadCancelled != null)
                            DownloadCancelled(this, EventArgs.Empty);
                    }), null);
            }
        }

        /// <summary>
        /// 下载文件的大小
        /// </summary>
        public long ContentSize { get; private set; }

        /// <summary>
        /// 已接收大小
        /// </summary>
        public long BytesReceived { get; private set; }

        /// <summary>
        /// 下载速度
        /// </summary>
        public int SpeedInBytes { get; private set; }

        /// <summary>
        /// 是否支持Range
        /// </summary>
        public bool AcceptRange { get; private set; }

 
        /// <summary>
        /// 下载器
        /// </summary>
        /// <param name="url">文件url</param>
        /// <param name="path">文件path</param>
        public HttpDownloader(string url, string path)
        {
            FileURL = url;
            FilePath = path;
            operation = AsyncOperationManager.CreateOperation(null);
            progress = 0;//??
            BytesReceived = 0;//??
            SpeedInBytes = 0;
            stopwatch = new Stopwatch();
        }

        /// <summary>
        /// 检测是否支持断点下载
        /// </summary>
        /// <param name="url">下载文件URL</param>
        /// <param name="fileName">文件名包括路径</param>
        /// <returns></returns>
        private bool IsResume(string url, string fileName)
        {
            string tempFileName = fileName + ".downloading";
            bool resumeDownload = false;
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = WebRequest.Create(FileURL) as HttpWebRequest;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
                request.AddRange(0);
                response = (HttpWebResponse)request.GetResponse();
                // 支持Range
                AcceptRange = GetAcceptRanges(response);
                if (AcceptRange)
                {
                    string lastModifiedNew = GetLastModified(response);         // 获取最新的LastModified
                    string lastModifiedOld = Util.QueryLastModified(fileName);  // 获取本地的LastModified

                    // 下载目录下,有该文件的downloading文件,且LastModified相同
                    if (File.Exists(tempFileName) && lastModifiedNew == lastModifiedOld)
                    {
                        resumeDownload = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lastModifiedNew))
                        {
                            Util.CreateLastModified(fileName, lastModifiedNew);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                
            }    
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return resumeDownload;
        }

        /// <summary>
        /// 下载方法
        /// </summary>
        /// <param name="length"></param>
        /// <param name="overFile"></param>
        private void Download(bool isPause)
        {
            bool resumeDownload = IsResume(FileURL, FilePath);
            string tempFileName = FilePath + ".downloading";

            FileMode fileMode = FileMode.Create;
            Stream stream = null;
            FileStream fileStream = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                HttpWebRequest httpWebRequest = WebRequest.Create(FileURL) as HttpWebRequest;
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
                if (resumeDownload)
                {
                    FileInfo fileInfo = new FileInfo(tempFileName);
                    httpWebRequest.AddRange(fileInfo.Length);
                    fileMode = FileMode.Append;
                }
                httpWebRequest.AllowAutoRedirect = true;
                httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                stream = httpWebResponse.GetResponseStream();
                if (!isPause)
                    ContentSize = httpWebResponse.ContentLength;

                fileStream = new FileStream(tempFileName, fileMode, FileAccess.Write);
                int bytesRead = 0;
                int speedBytes = 0;
                byte[] buffer = new byte[BufferSize];
                stopwatch.Start();

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (state == DownloadState.Cancelled | state == DownloadState.Paused) break;
                    state = DownloadState.Downloading;
                    fileStream.Write(buffer, 0, bytesRead);
                    fileStream.Flush();
                    BytesReceived += bytesRead;
                    speedBytes += bytesRead;
                    Progress = progress = (int)(BytesReceived * 100.0 / ContentSize);
                    SpeedInBytes = (int)(speedBytes / 1.0 / stopwatch.Elapsed.TotalSeconds);
                }
            }
            catch(Exception)
            {
                state = DownloadState.Completed;
                return;
            }
            finally
            {
                stopwatch.Reset();
                if (fileStream != null)
                {
                    fileStream.Flush();
                    fileStream.Close();
                }
                if (stream != null)
                    stream.Close();
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }
            Thread.Sleep(100);
            if (state == DownloadState.Downloading)
            {
                state = DownloadState.Completed;
                State = state;
            }
            File.Move(tempFileName, FilePath);

        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public async void StartAsync()
        {
            if (state != DownloadState.Started & state != DownloadState.Completed & state != DownloadState.Cancelled)
                return;
            state = DownloadState.Started;
            await Task.Run(() => { Download(false); });
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public void Pause()
        {
            if (!AcceptRange)
                return; // 不支持断点下载
            if (State == DownloadState.Downloading)
                state = DownloadState.Paused;
        }

        /// <summary>
        /// 继续下载
        /// </summary>
        public async void ResumeAsync()
        {
            if (State != DownloadState.Paused) return;
            state = DownloadState.Started;
            await Task.Run(() =>
            {
                Download(true);
            });
        }

        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancel()
        {
            if (state == DownloadState.Completed | state == DownloadState.Cancelled | state == DownloadState.ErrorOccured) return;
            if (state == DownloadState.Paused)
            {
                this.Pause();
                state = DownloadState.Cancelled;
                Thread.Sleep(100);
                CloseResources();
            }

            state = DownloadState.Cancelled;
        }

        void CloseResources()
        {
            if (FilePath != null && state == DownloadState.Cancelled | state == DownloadState.ErrorOccured)
            {
                try
                {
                    File.Delete(FilePath);
                }
                catch
                {
                    throw new Exception("There is an error unknown. This problem may cause because of the file is in use");
                }
            }
        }

        /// <summary>
        /// 判断是否支持Range
        /// </summary>
        /// <returns></returns>
        private bool GetAcceptRanges(HttpWebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; i++)
            {
                if (response.Headers.AllKeys[i].Contains("Range"))
                    return response.Headers[i].Contains("byte");
            }
            return false;
        }

        /// <summary>
        /// 获取Etag信息
        /// </summary>
        /// <returns></returns>
        private string GetEtag(HttpWebResponse response)
        {
            return response.Headers["ETag"];
        }

        /// <summary>
        /// 获取LastModified信息
        /// </summary>
        /// <returns></returns>
        private string GetLastModified(HttpWebResponse response)
        {
            return response.Headers["Last-Modified"];
        }
    }
}
