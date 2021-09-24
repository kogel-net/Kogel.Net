using System;
using System.Collections.Generic;
using System.Text;
using Kogel.Net.WebSocket.Extension;
using Kogel.Net.WebSocket.Server;

namespace Kogel.Net.Test.Command
{
    /// <summary>
    /// websocket 长连接执行 server
    /// </summary>
    public class WebSocketServerCommand : ICommand, IDisposable
    {
        private WebSocketServer _webServer;

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_webServer != null)
            {
                _webServer.Stop();
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            try
            {
                _webServer = new WebSocketServer("ws://127.0.0.1:8081");
                _webServer.AddController<TestController>("/test");
                _webServer.Start();
                Console.WriteLine("启动成功!");
                Console.ReadKey(true);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    /// <summary>
    /// 服务处理类
    /// </summary>
    public class TestController : WebSocketControllerBase
    {
        protected override string OnUniqueId()
        {
            return base.OnUniqueId();
        }

        /// <summary>
        /// 打开
        /// </summary>
        protected override void OnOpen()
        {
            Console.WriteLine($"获取到一个请求,用户标识为:{this.Id}");
            base.OnOpen();
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            //消息内容
            var message = e.Data;
            Console.WriteLine($"获取到一条消息,用户标识为:{this.Id},消息内容为:{message}");     
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="e"></param>
        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine($"获取到一个异常,用户标识为:{this.Id}");
            base.OnError(e);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine($"获取到一个离线请求,用户标识为:{this.Id}");
            base.OnClose(e);
        }
    }
}
