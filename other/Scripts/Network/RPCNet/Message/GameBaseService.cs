using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crucis.Protocol
{
    public class BaseService
    {
        /// <summary>
		/// 连接成功
		/// </summary>
		public Action OnConnected;
        /// <summary>
        /// 连接失败
        /// </summary>
        public Action OnConnectFailed;
        /// <summary>
        /// 连接断开
        /// </summary>
        public Action OnDisconnected;

        public ConnectState connectState = ConnectState.Disconnected;

        public string uri_ { get; set; }

        public BaseService()
        {
            NetWork.Instance?.AddService(this);
        }

        public void Dispose()
        {
            NetWork.Instance?.RemoveService(this);
        }

        private void SafeInvoke(Action action, string log)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Fail($"SafeInvoke Error: {log}, ErrorMessage:{e.Message}");
            }
        }

        public void ConnectAsync()
        {
            SetConnectState(ConnectState.Connecting);
            NetWork.Instance?.ConnectAsync(uri_);
        }

        public void DisConnect()
        {
            SetConnectState(ConnectState.Disconnected);
        }

        public void SetConnectState(ConnectState type)
        {
            switch(type)
            {
                case ConnectState.Disconnected:
                    connectState = ConnectState.Disconnected;
                    SafeInvoke(OnDisconnected, "OnDisconnected");
                    break;
                case ConnectState.Connecting:
                    connectState = ConnectState.Connecting;
                    break;
                case ConnectState.Connected:
                    connectState = ConnectState.Connected;
                    SafeInvoke(OnConnected, "OnConnected");
                    break;
                case ConnectState.ConnectFailed:
                    connectState = ConnectState.ConnectFailed;
                    SafeInvoke(OnConnectFailed, "OnConnectFailed");
                    break;
            }
        }
    }

    public enum ConnectState : byte
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectFailed
    }
}
