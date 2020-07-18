
using Crucis.Protocol.GameSession;
using System.Collections.Generic;
using UnityEngine;

namespace Crucis.Protocol
{
    public static class DropHandler
    {
        private static ToSyncDropStream cs_stream;
        
        private static void DeathDropRun(GameSession.ToSyncDropResponse message)
        {
            GameSession.ToSyncDropResponse.Types.Success success = message.Success;
            if (success != null)
            {
				DeathDrop deathDrop = success.Success_.DeathDrop;
				if (deathDrop != null)
				{
					///Debug.LogError("DropHandlerdeathDrop.DropList:" + deathDrop.DropList);
					List<DropInfo> myDropList = new List<DropInfo>();
					foreach (var item in deathDrop.DropList)
					{
						var dInfo = new DropInfo();
						dInfo.PlayerUid = item.PlayerUid;
						dInfo.ChestTid = item.ChestTid;
						dInfo.ShipTlv = item.ShipTlv;
						dInfo.Quality = item.Quality;
						myDropList.Add(dInfo);
					}

					Debug.Log($"ToSyncDropResponse:DeathDropRun->heroid = {deathDrop.HeroID} myDropList.Count = {myDropList.Count}");

					if (myDropList != null && myDropList.Count > 0)
					{
						DropItemManager.Instance.SetDropItemInfoByDeath(deathDrop.HeroID, myDropList);
					}
				}
            }
        }

		private static void PersonalDropRun(GameSession.ToSyncDropResponse message)
		{
			GameSession.ToSyncDropResponse.Types.Success success = message.Success;
			if (success != null)
			{
				s2cSyncPersonalDrop personalDrop = success.Success_.PersonalDrop;
				if (personalDrop != null)
				{
					Debug.Log($"ToSyncDropResponse:PersonalDropRun");
					MineDropItemManager.Instance.CreateDropItemByRespond(personalDrop);
				}
			}
		}

		private static void PersonalDropResultRun(GameSession.ToSyncDropResponse message)
		{
			GameSession.ToSyncDropResponse.Types.Success success = message.Success;
			if (success != null)
			{
				s2c_NotifyPersonalDropResult result = success.Success_.NotifyPersonalDrop;
				if (result != null)
				{
					if (result.Result)
					{
						string key = result.MapId.ToString() + result.AreaUid.ToString() + result.NpcUid.ToString();
						Debug.Log($"ToSyncDropResponse:PersonalDropResultRun key = {key}");
						MineDropItemManager.Instance.NotifyDropResult(key);
					}
				}
			}
		}

		public static async void HandleSyncDrop()
        {
            cs_stream?.Close();
            cs_stream = new ToSyncDropStream();
			ToSyncDropResponse response;
            while ((response = await cs_stream.ReadAsync()) != null)
            {
				DropHandler.DeathDropRun(response);
				DropHandler.PersonalDropRun(response);
				DropHandler.PersonalDropResultRun(response);
            }
        }
    }
}
