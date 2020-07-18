using Assets.Scripts.Lib.Net;
using Crucis.Protocol;
using Crucis.Protocol.GameSession;
using System;
using pb = global::Google.Protobuf;

namespace Game.Frame.Net
{
	public class SocketClient
	{
        public Action OnConnected;
		public Action OnConnectFailed;
		public Action OnDisconnected;

        private string m_Address;
		private int m_Port;

        private void onConnected_imp()
        {
            SafeInvoke(OnConnected, "OnConnected");
        }

        private void onConnectFailed_imp()
        {
            SafeInvoke(OnConnectFailed, "OnConnectFailed");
        }

        private void onDisconnected_imp()
        {
            SafeInvoke(OnDisconnected, "OnDisconnected");
        }

        public void Connect(string address, int port)
		{
            m_Address = address;
            m_Port = port;

            string uri = address + ":" + port.ToString();
            OldPLService.Service = new OldPLService(uri);
            GameSession.Service = new GameSession(uri);

            OldPLService.Service.OnConnected += onConnected_imp;
            OldPLService.Service.OnConnectFailed += onConnectFailed_imp;
            OldPLService.Service.OnDisconnected += onDisconnected_imp;

            OldPLService.Service.ConnectAsync();
        }

		public void Close()
		{
            if(OldPLService.Service != null)
            {
                OldPLService.Service.DisConnect();
            }
        }

		public bool IsConnected()
		{
            if (OldPLService.Service != null)
            {
                return OldPLService.Service.connectState == ConnectState.Connected;
            }
            return false;
		}

		public void Send(byte[] buffer, int length)
		{
            pb::ByteString body_ = pb::ByteString.CopyFrom(buffer, 0, length);
            OldPLService.Service.OldPl_SendAsync(body_);
		}

		private void SafeInvoke(Action action, string log)
		{
			try
			{
				Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG
					, string.Format("Begin invoke ({0})", log));

				action?.Invoke();

				Leyoutech.Utility.DebugUtility.Log(KConstants.LOG_TAG
					, string.Format("End invoke ({0})", log));
			}
			catch (Exception e)
			{
				Leyoutech.Utility.DebugUtility.LogError(KConstants.LOG_TAG
					, string.Format("Invoke ({0}) Exception:\n{1}", log, e.ToString()));
			}
		}

	}
}