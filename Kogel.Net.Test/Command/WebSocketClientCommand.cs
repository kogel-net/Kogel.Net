using System;
using System.Collections.Generic;
using System.Text;
using Kogel.Net.WebSocket;

namespace Kogel.Net.Test.Command
{
    public class WebSocketClientCommand : ICommand, IDisposable
    {
        private Kogel.Net.WebSocket.WebSocket _webSocket;
        public void Dispose()
        {
            if (_webSocket != null)
            {
                _webSocket.Close();
            }
        }

        public void Start()
        {
            _webSocket = new WebSocket.WebSocket("ws://127.0.0.1:8081/test");
            //开始连接
            _webSocket.Connect();
            //绑定消息事件
            _webSocket.OnMessage += _webSocket_OnMessage;
            //发送消息
            _webSocket.Send($"测试消息:{Guid.NewGuid()}");
            //自定义发送
            while (true)
            {
                var message = Console.ReadLine();
                _webSocket.Send(message);
                Console.WriteLine("发送成功");
            }
        }

        /// <summary>
        /// 消息接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _webSocket_OnMessage(object sender, WebSocket.Extension.MessageEventArgs e)
        {

        }
    }
}
