namespace DownLoadLib
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public enum DownloadState
    {
        /// <summary>
        /// 下载已开始
        /// </summary>
        Started,

        /// <summary>
        /// 下载已暂停
        /// </summary>
        Paused,

        /// <summary>
        /// 正在下载
        /// </summary>
        Downloading,

        /// <summary>
        /// 下载已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 下载已取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 下载发生错误
        /// </summary>
        ErrorOccured
    }
}
