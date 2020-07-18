using Assets.Scripts.Define;
using Assets.Scripts.Proto;
using UnityEngine;

namespace Game.Frame.Net
{
	public class ReliveController : AbsRpcController
	{
		public void RequestRelive(PlayerReliveType playerReliveType)
		{
			GameplayProxy gameplayProxy = GameFacade.Instance.RetrieveProxy(ProxyName.GameplayProxy) as GameplayProxy;

			C2S_PLAYER_RELIVE request = SingleInstanceCache.GetInstanceByType<C2S_PLAYER_RELIVE>();
			request.protocolID = (ushort)KC2S_Protocol.c2s_player_relive;
            request.relive_type = (ushort)playerReliveType;
			NetworkManager.Instance.SendToGameServer(request);
		}
	}
}
