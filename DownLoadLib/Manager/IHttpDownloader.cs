using System;

namespace DownLoadLib
{
    interface IHttpDownloader
    {
        /// <summary>
        /// delegate 下载取消事件
        /// </summary>
        event EventHandler DownloadCancelled;

        /// <summary>
        /// delegate 下载完成事件
        /// </summary>
        event EventHandler DownloadCompleted;

        /// <summary>
        /// delegate 下载进度改变事件
        /// </summary>
        event ProgressChangedEventHandler DownloadProgressChanged;

        /// <summary>
        /// 内容大小
        /// </summary>
        long ContentSize { get; }

        /// <summary>
        /// 已接收大小
        /// </summary>
        long BytesReceived { get; }

        /// <summary>
        /// 进度
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// 下载速度
        /// </summary>
        int SpeedInBytes { get; }

        /// <summary>
        /// 下载文件URL
        /// </summary>
        string FileURL { get; }

        /// <summary>
        /// 下载文件路径
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// 是否支持Range
        /// </summary>
        bool AcceptRange { get; }

        /// <summary>
        /// 下载状态
        /// </summary>
        DownloadState State { get; }

        /// <summary>
        /// 异步开始下载
        /// </summary>
        void StartAsync();

        /// <summary>
        /// 暂停下载
        /// </summary>
        void Pause();

        /// <summary>
        /// 异步继续下载
        /// </summary>
        void ResumeAsync();

        /// <summary>
        /// 取消下载
        /// </summary>
        void Cancel();
    }
}
