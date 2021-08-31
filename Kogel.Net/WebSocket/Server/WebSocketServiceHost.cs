using Kogel.Net.WebSocket.Enums;
using Kogel.Net.WebSocket.Extension.Net.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogel.Net.WebSocket.Server
{
    /// <summary>
    /// 公开用于访问信息的方法和属性
    /// </summary>
    public abstract class WebSocketServiceHost
    {
        private string _path;
        private WebSocketSessionManager _sessions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        protected WebSocketServiceHost(string path)
        {
            _path = path;
            _sessions = new WebSocketSessionManager();
        }

        internal ServerState State
        {
            get
            {
                return _sessions.State;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示服务是否清理不活动的会话定期。
        /// </summary>
        public bool KeepClean
        {
            get
            {
                return _sessions.KeepClean;
            }

            set
            {
                _sessions.KeepClean = value;
            }
        }

        /// <summary>
        /// 获取服务的路径。
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// 获取服务中会话的管理函数。
        /// </summary>
        public WebSocketSessionManager Sessions
        {
            get
            {
                return _sessions;
            }
        }

        /// <summary>
        /// 获取服务的行为类型
        /// </summary>
        public abstract Type BehaviorType { get; }

        /// <summary>
        /// 获取或设置等待响应 WebSocket Ping 或的时间
        /// </summary>
        public TimeSpan WaitTime
        {
            get
            {
                return _sessions.WaitTime;
            }

            set
            {
                _sessions.WaitTime = value;
            }
        }

        /// <summary>
        /// 启动会话
        /// </summary>
        internal void Start()
        {
            _sessions.Start();
        }

        /// <summary>
        /// 启动会话
        /// </summary>
        /// <param name="context"></param>
        internal void StartSession(WebSocketContext context)
        {
            CreateSession().Start(context, _sessions);
        }

        /// <summary>
        /// 停止会话
        /// </summary>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        internal void Stop(ushort code, string reason)
        {
            _sessions.Stop(code, reason);
        }

        /// <summary>
        /// 为服务创建一个新会话。
        /// </summary>
        /// <returns></returns>
        protected abstract WebSocketBehavior CreateSession();
    }

    /// <summary>
    /// 内部用于访问信息的方法和属性
    /// </summary>
    /// <typeparam name="TBehavior"></typeparam>
    internal class WebSocketServiceHost<TBehavior> : WebSocketServiceHost
    where TBehavior : WebSocketBehavior
    {
        private Func<TBehavior> _creator;
        internal WebSocketServiceHost(string path, Func<TBehavior> creator) : this(path, creator, null)
        {
        }

        internal WebSocketServiceHost(string path, Func<TBehavior> creator, Action<TBehavior> initializer) : base(path)
        {
            _creator = CreateCreator(creator, initializer);
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(TBehavior);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="initializer"></param>
        /// <returns></returns>
        private Func<TBehavior> CreateCreator(Func<TBehavior> creator, Action<TBehavior> initializer)
        {
            if (initializer == null)
                return creator;

            return () =>
            {
                var ret = creator();
                initializer(ret);

                return ret;
            };
        }

        protected override WebSocketBehavior CreateSession()
        {
            return _creator();
        }
    }
}
