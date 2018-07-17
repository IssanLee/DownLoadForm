namespace DownLoadLib
{
    /// <summary>
    /// 下载进度变化参数
    /// </summary>
    public class DownloadProgressChangedEventArgs
    {
        /// <summary>
        /// 下载进度
        /// </summary>
        public int Progress { get; }

        /// <summary>
        /// 下载速度
        /// </summary>
        public int Speed { get; }

        /// <summary>
        /// 下载进度变化参数
        /// </summary>
        /// <param name="progress">进度</param>
        /// <param name="speed">速度</param>
        public DownloadProgressChangedEventArgs(int progress, int speed)
        {
            Progress = progress;
            Speed = speed;
        }
    }
}
