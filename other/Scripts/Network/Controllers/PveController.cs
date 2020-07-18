
using Assets.Scripts.Proto;
using Game.Frame.Net;

public class PveController : AbsRpcController
{
	private PveProxy pveProxy = null;

	public PveController()
	{
		pveProxy = ((PveProxy)GameFacade.Instance.RetrieveProxy(ProxyName.PveProxy));
	}

	/// <summary>
	/// 退出Pve场景
	/// </summary>
	public void ExitPve()
	{
		if (!pveProxy.m_ExitSended)
		{
			var msg = new C2S_ExitPve();
			msg.protocolID = (ushort)KC2S_Protocol.c2s_exit_pve;

			NetworkManager.Instance.SendToGameServer(msg);

			pveProxy.m_ExitSended = true;
		}
	}
}

