using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionMaster.Core
{
    /// <summary>
    /// 连接接口
    /// </summary>
    public interface IBinaryConnection
    {
        /// <summary>
        /// 连接是否已打开
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// 接收缓冲区大小,单位字节
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// 连接已关闭事件
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// 异步打开连接
        /// </summary>
        /// <returns></returns>
        Task OpenAsync();

        /// <summary>
        /// 异步关闭连接
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="message">要发送的字节</param>
        /// <param name="startIndex">要发送字节的开始下标,从0开始</param>
        /// <param name="count">要发送的字节数</param>
        /// <returns></returns>
        Task SendAsync(byte[] message,int startIndex,int count,CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步接收消息
        /// </summary>
        /// <returns>接收结果</returns>
        Task<ReceiveResult> ReceiveAsync(CancellationToken cancellationToken = default);
    }
}
