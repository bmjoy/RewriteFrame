using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine;

public class SceneShipProxy : Proxy
{
	private GameObject m_ShipGameObject;
	private bool m_Show;

	public SceneShipProxy() : base(ProxyName.SceneShipProxy) { }

	public void ShowShip()
	{
        ShipProxy shipProxy = GameFacade.Instance.RetrieveProxy(ProxyName.ShipProxy) as ShipProxy;
		IShip ship = shipProxy.GetAppointWarShip();
		if (ship != null)
		{
			CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
			Model model = cfgEternityProxy.GetItemModelByKey(ship.GetTID());
			GameObject container = GameObject.Find(model.ExhibitionPoint);
			if (container)
			{
				m_Show = true;
				TransformUtil.FindUIObject<Transform>(container.transform, "Ship_Light").gameObject.SetActive(true);
                AssetUtil.InstanceAssetAsync(model.AssetName,
                    (pathOrAddress, returnObject, userData) =>
                    {
                        if (returnObject != null)
                        {
                            GameObject gameobj = (GameObject)returnObject;
                            gameobj.transform.SetParent(container.transform, false);
                            m_ShipGameObject = gameobj;
                            if (!m_Show)
                            {
                                HideShip();
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("资源加载成功，但返回null,  pathOrAddress = {0}", pathOrAddress));
                        }
                    });
            }
        }
	}

	//private void OnLoadModel(AsyncOperationHandle<GameObject> asyncOperation)
	//{
	//	m_ShipGameObject = asyncOperation.Result;
	//	if (!m_Show)
	//	{
	//		HideShip();
	//	}
	//}

	public void HideShip()
	{
		m_Show = false;
		if (m_ShipGameObject)
		{
			TransformUtil.FindUIObject<Transform>(m_ShipGameObject.transform.parent, "Ship_Light").gameObject.SetActive(false);
            GameObject.Destroy(m_ShipGameObject);
            //AssetManager.ReleaseInstance(m_ShipGameObject);
        }
		m_ShipGameObject = null;
	}
}