/*===============================
 * Author: [Allen]
 * Purpose: WwiseManager
 * Time: 2019/5/18 12:10:41
================================*/
using AK.Wwise;
using Eternity.FlatBuffer;
using System;
using System.Collections.Generic;
using UnityEngine;
using SystemObject = System.Object;

public class WwiseManager : Singleton<WwiseManager>
{
    /// <summary>
    /// 组合表 语音ID
    /// </summary>
    public const int voiceComboID = 999;

    /// <summary>
    /// 音效库挂点
    /// </summary>
    private GameObject SoundBankObjRoot;

    /// <summary>
    /// 关卡音效库物体
    /// </summary>
    private GameObject SoundBanklevelObj;

    /// <summary>
    /// 音源挂点
    /// </summary>
    private GameObject SoundSourceObjRoot;

    /// <summary>
    ///2D音源挂点
    /// </summary>
    private GameObject SoundSource2DObj;

    /// <summary>
    /// 3D音源模板挂点
    /// </summary>
    private GameObject SoundSource3DObj_TemplateRoot;

    /// <summary>
    /// 摄像机组件
    /// </summary>
    private TAkGameObj3rd AkGameObj3Rd;


    /// <summary>
    ///关卡背景音乐是否加载完毕
    /// </summary>
    private bool SoundBanklevelBackgroundPlayed = false;

    /// <summary>
    /// 3D音源对象池 string 音源名字，GameObject 池子
    /// </summary>
    private Dictionary<string, GameObject> SoundSource3DObjPools = new Dictionary<string, GameObject>();

    /// <summary>
    /// 3D 声源需要记录，公共物体的容器
    /// </summary>
    private Dictionary<string, GameObject> SoundSource3DPublicObj = new Dictionary<string, GameObject>();


    /// <summary>
    /// sound name 对应的 GameObject 容器
    /// </summary>
    private Dictionary<string, GameObject> SoundNameAboutGameObject = new Dictionary<string, GameObject>();

    /// <summary>
    /// 关于Bank 的物体容器
    /// </summary>
    private Dictionary<GameObject, List<TAkBank>> GameObjectAboutBanks = new Dictionary<GameObject, List<TAkBank>>();

    /// <summary>
    /// bank 计数容器，计数有多少音效通过这个bank 正在播放，string bankName，int 用到这个bank正在播放的计数
    /// </summary>
    private Dictionary<string, int> PlayIdCoutAbountSoundBank = new Dictionary<string, int>();

    /// <summary>
    /// 游戏启动最先加载的bank 一直保存到游戏结束
    /// </summary>
    private System.Collections.Generic.List<int> WWISE_FirstBankList = new System.Collections.Generic.List<int>() {
        (int)WwiseSoundBank.SoundBank_Music,
        (int)WwiseSoundBank.SoundBank_int_EntireEvents,
        (int)WwiseSoundBank.SoundBank_int_VoxEvents,
        (int)WwiseSoundBank.SoundBank_SFX_UI,
        (int)WwiseSoundBank.SoundBank_Str2D_BG_InD,
        (int)WwiseSoundBank.SoundBank_Str2D_Atmos_DsP,
        (int)WwiseSoundBank.SoundBank_Vox,
    };

    /// <summary>
    /// 游戏启动最先加载的bank 一直保存到游戏结束
    /// </summary>
    private List<string> WWISE_FirstBankNameList = new List<string>();


    /// <summary>
    /// 加载计数
    /// </summary>
    private int FirstBankLoadOverCout = 0;

    /// <summary>
    /// 时间触发器管理
    /// </summary>
    private WwiseTimeTriggerManager m_triggerMgr;

    /// <summary>
    /// GameMusic 表数据
    /// </summary>
    private CfgEternityProxy cfgEternityProxy;

    /// <summary>
    /// 3D音效回调数据结构
    /// </summary>
    private class SoundSource3DCallBackData
    {
        public GameObject gameObject;
        public string publicObjsEventKey;
        public string bankName;

        public SoundSource3DCallBackData(GameObject gb, string bankname, string publicObjsKey)
        {
            gameObject = gb;
            publicObjsEventKey = publicObjsKey;
            bankName = bankname;
        }
    }

    #region ====================================函数区==================================================================

    /// <summary>
    /// 获取时间触发器管理
    /// </summary>
    /// <returns></returns>
    public WwiseTimeTriggerManager GetTimeTriggerMgr()
    {
        return m_triggerMgr;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;

        m_triggerMgr = new WwiseTimeTriggerManager();
        m_triggerMgr.Init();

        //wwise Root 挂点
        GameObject WwiseSoundRoot = new GameObject("WwiseSoundRoot");

        //root 挂点不销毁
        GameObject.DontDestroyOnLoad(WwiseSoundRoot);

        //设置主（全局）音效库
        SoundBankObjRoot = new GameObject("SoundBankRoot");
        SoundBankObjRoot.transform.SetParent(WwiseSoundRoot.transform, false);

        //设置音源总挂点
        SoundSourceObjRoot = new GameObject("SoundSourceRoot");
        SoundSourceObjRoot.transform.SetParent(WwiseSoundRoot.transform, false);

        //设置2D音源挂点
        SoundSource2DObj = new GameObject("SoundSource2DObj");
        SoundSource2DObj.transform.SetParent(SoundSourceObjRoot.transform, false);

        //设置3D音源模板挂点
        SoundSource3DObj_TemplateRoot = new GameObject("SoundSource3DObj_TemplateRoot");
        SoundSource3DObj_TemplateRoot.transform.SetParent(SoundSourceObjRoot.transform, false);

        // 加载最先的bank
        LoadFirstBank();


    }

    /// <summary>
    /// 加载最先的bank
    /// </summary>
    private void LoadFirstBank()
    {
        Debug.Log("------WwiseManager--------->  加载最先的bank");
        if (cfgEternityProxy == null)
        {
            Debug.LogError("WwiseManager.cs--->LoadFirstBank()-->cfgEternityProxy == null !请检查！");
            return;
        }

        for (int i = 0; i < WWISE_FirstBankList.Count; i++)
        {
            SoundBank? soundbank = cfgEternityProxy.GetSoundBankByKey((uint)WWISE_FirstBankList[i]);

            if (string.IsNullOrEmpty(soundbank.Value.BankName))
            {
                Debug.LogWarning(string.Format("bankName == null ! 检查表格  BankId = {0}", WWISE_FirstBankList[i]));
                continue;
            }
            LoadSoundBank(soundbank.Value.BankName, FirstBankListLoadOverDetector);
        }
    }

    /// <summary>
    /// 加载bank
    /// </summary>
    /// <param name="bankName"></param>
    public void LoadSoundBank(string bankName, AkBank.VoidCompleted voidCompleted = null)
    {
        GameObject soundBankObj = null;
        if (!SoundNameAboutGameObject.ContainsKey(bankName))
        {
            soundBankObj = new GameObject(string.Format("SoundBank_{0}", bankName));
            soundBankObj.transform.SetParent(SoundBankObjRoot.transform, false);
            SoundNameAboutGameObject.Add(bankName, soundBankObj);
        }

        soundBankObj = SoundNameAboutGameObject[bankName];
        soundBankObj.SetActive(false);
        TAkBank akBank = soundBankObj.AddComponent<TAkBank>();
        akBank.Completed = voidCompleted;
        akBank.SetBankName(bankName);
        soundBankObj.SetActive(true);

        List<TAkBank> akBanks = null;
        if (!GameObjectAboutBanks.ContainsKey(soundBankObj))
        {
            akBanks = new List<TAkBank>();
            GameObjectAboutBanks.Add(soundBankObj, akBanks);
        }
        GameObjectAboutBanks[soundBankObj].Add(akBank);

        WWISE_FirstBankNameList.Add(bankName);
    }

    /// <summary>
    /// 卸载某个bank ,
    /// 注意bank 是有计数累计的，只有 计数累计 等于0 时才真正卸载
    /// </summary>
    /// <param name="bankName"></param>
    public void UnLoadSoundBank(string bankName)
    {
        GameObject soundBankObj = null;
        if (!SoundNameAboutGameObject.ContainsKey(bankName))
        {
            return;
        }
        soundBankObj = SoundNameAboutGameObject[bankName];
        if (!GameObjectAboutBanks.ContainsKey(soundBankObj))
        {
            return;
        }
        List<TAkBank> akBanks = GameObjectAboutBanks[soundBankObj];
        if (akBanks.Count > 0)
        {
            TAkBank akBank = akBanks[0];
            GameObject.Destroy(akBank);
            akBanks.RemoveAt(0);
        }
        if (akBanks.Count == 0)
        {
            GameObject.Destroy(soundBankObj);
            SoundNameAboutGameObject.Remove(bankName);
        }
    }

    /// <summary>
    /// 主摄像机增加监听器
    /// </summary>
    /// <param name="cameraObj"></param>
    public void AddAudiolisendner(GameObject cameraObj)
    {
        if (cameraObj == null)
        {
            return;
		}

		AkGameObj3Rd = cameraObj.GetComponent<TAkGameObj3rd>();
		if (AkGameObj3Rd == null)
		{
			AkGameObj3Rd = cameraObj.AddComponent<TAkGameObj3rd>();
		}
		AkGameObj3Rd.isEnvironmentAware = false;    //环境交互，必须携带刚体

		if (cameraObj.GetComponent<AkAudioListener>() == null)
		{
			cameraObj.AddComponent<AkAudioListener>();
		}

		Debug.Log("主摄像机添加 wwise AkAudioListener 完毕!");
    }

    /// <summary>
    /// 设置声音Listener 的目标
    /// 默认在摄像机上 
    /// </summary>
    /// <param name="target"></param>
    public void SetAudioListenerTarget(Transform target)
    {
        if (AkGameObj3Rd != null)
        {
            AkGameObj3Rd.SetTarget(target);
            AkGameObj3Rd.BeUseToTarget(true);
        }
    }

    /// <summary>
    /// 加载检查
    /// </summary>
    private void FirstBankListLoadOverDetector()
    {
        FirstBankLoadOverCout++;

        if (FirstBankLoadOverCout == WWISE_FirstBankList.Count)
        {
            Debug.Log(" 最先加载bank 列表全部OK ! ");
            //播放主背景音乐
            WwiseUtil.PlaySound((int)WwiseMusic.Music_MainBackgroundMusic, false, null);
        }
    }

    /// <summary>
    /// 场景切换结束
    /// </summary>
    /// <param name="mapId"></param>
    public void SceneSwitchEnd(uint mapId)
    {
        Debug.LogWarning(" SceneSwitchEnd --> mapID = " + mapId);

        if (mapId < 0)
        {
            return;
        }





    }

    /// <summary>
    /// 预加载 Event
    /// </summary>
    /// <param name="SounEventName"></param>
    /// <param name="load"></param>
    /// <returns></returns>
    public bool LoadorUnLoadPrepareEvent(string SounEventName, bool load = true)
    {
        AKRESULT result = AkSoundEngine.PrepareEvent(load ? AkPreparationType.Preparation_Load : AkPreparationType.Preparation_Unload, new string[] { SounEventName }, 1);
        if (result != AKRESULT.AK_Success)
        {
            //UnityEngine.Debug.LogError(string.Format("WwiseUnity: PlaySound2D---> PrepareEvent失败!  SounEventName = {0}, result  = {1} ", SounEventName, result));
            return false;
        }
        return true;
    }

    /// <summary>
    /// 2D声音
    /// </summary>
    /// <param name="SounEventName">事件名字</param>
    /// <param name="endCallback">播放完毕的回调,返回 playingID</param>
    /// <returns></returns>
    public uint PlaySound2D(string SounEventName, Action<SystemObject> endCallback = null,  SystemObject userEndData = null)
    {
        TAkGameObjEventMonitor eventMonitor = SoundSource2DObj.GetOrAddComponent<TAkGameObjEventMonitor>();
        uint playId = AkSoundEngine.PostEvent(SounEventName, SoundSource2DObj, (uint)AkCallbackType.AK_EndOfEvent, EventCallback2D,null);

      //  Debug.LogWarning("----------> PlaySound2D ---> playId = " + playId + "  SounEventName = " + SounEventName );

        eventMonitor.AddPlaying(SounEventName, playId, endCallback,  userEndData);
        return playId;
    }

    /// <summary>
    /// 2D音效播放完毕回调
    /// </summary>
    /// <param name="cookie"> 参数</param>
    /// <param name="type">类型</param>
    /// <param name="callbackInfo"></param>
    private void EventCallback2D(object cookie, AkCallbackType type, AkCallbackInfo callbackInfo)
    {
        if (type == AkCallbackType.AK_EndOfEvent)
        {
            AkEventCallbackInfo info = callbackInfo as AkEventCallbackInfo;
            if (info != null)
            {
                 // Debug.LogWarning("--------> 2d 回收！ playingID = " + info.playingID );
//                 Action<uint> endCallback = (Action<uint>)cookie;
//                 endCallback?.Invoke(info.playingID);


                TAkGameObjEventMonitor eventMonitor = SoundSource2DObj.GetComponent<TAkGameObjEventMonitor>();
                if (eventMonitor != null)
                    eventMonitor.DeleOneEvent(info.playingID);

            }
        }
    }

    /// <summary>
    /// 3D 声音
    /// </summary>
    /// <param name="SounEventName">事件名字</param>
    /// <param name="BankName">所属的bank name</param>
    /// <param name="publicObjsEventNameArray">公共物体事件集合</param>
    /// <param name="SoundParent">3D 声音挂点</param>
    /// 注意：没有挂点，音源则在 坐标 零点 位置
    /// <returns></returns>
    public uint PlaySound3D(string SounEventName, string BankName, int publicObjsEventKey, Transform SoundParent = null, Action<SystemObject> endCallback = null, SystemObject userEndData = null)
    {
        if(SoundParent == null )
        {
            Debug.LogWarning(string.Format("3D 音效，挂点却为null，不予播放，请检查!!! SounEventName = {0}", SounEventName));
            return 0;
        }

        GameObject soundObj = null;
        if (!SoundSource3DObjPools.ContainsKey(SounEventName))
        {
            GameObject templateObj = new GameObject(string.Format("3DSound_{0}", SounEventName));
            templateObj.transform.SetParent(SoundSource3DObj_TemplateRoot.transform, false);
            templateObj.CreatePool(1,string.Empty);
            SoundSource3DObjPools.Add(SounEventName, templateObj);
        }

        bool havePublicObj = false;
        string pKey = string.Format("{0}_{1}", publicObjsEventKey, SoundParent.GetInstanceID());
        if (publicObjsEventKey > 0 && SoundSource3DPublicObj.TryGetValue(pKey, out soundObj))
        {
            havePublicObj = true;

            if (soundObj == null)
                SoundSource3DPublicObj.Remove(pKey);
        }

        if (!havePublicObj || soundObj == null)
        {
            GameObject templateobj = SoundSource3DObjPools[SounEventName];
            soundObj = templateobj.Spawn();
        }
        soundObj.transform.SetParent(SoundParent == null ? SoundSourceObjRoot.transform : SoundParent, false);

         //Debug.LogError("------------->PlaySound3D   gameObjID = " + AkSoundEngine.GetAkGameObjectID(soundObj) + " SounEventName = " + SounEventName);

        if (!SoundSource3DPublicObj.ContainsKey(pKey) && publicObjsEventKey > 0)
            SoundSource3DPublicObj.Add(pKey, soundObj);

        TAkGameObjEventMonitor eventMonitor = soundObj.GetOrAddComponent<TAkGameObjEventMonitor>();
        uint playId = AkSoundEngine.PostEvent(SounEventName, soundObj, (uint)AkCallbackType.AK_EndOfEvent, EventCallback3D, new SoundSource3DCallBackData(soundObj, BankName, pKey));
        eventMonitor.AddPlaying(SounEventName, playId, endCallback, userEndData);
        return playId;
    }

    /// <summary>
    /// 3D 声音
    /// <param name="SounEventName">事件名字</param>
    /// <param name="BankName">所属的bank name</param>
    /// <param name="publicObjsEventNameArray">公共物体事件集合</param>
    /// <param name="position">声音安放位置</param>
    /// <returns></returns>
    public uint PlaySound3D(string SounEventName, string BankName, int publicObjsEventKey, Vector3 position, Action<SystemObject> endCallback = null, SystemObject userEndData = null)
    {
        GameObject soundObj = null;
        if (!SoundSource3DObjPools.ContainsKey(SounEventName))
        {
            GameObject templateObj = new GameObject(string.Format("3DSound_{0}", SounEventName));
            templateObj.transform.SetParent(SoundSource3DObj_TemplateRoot.transform, false);
            templateObj.CreatePool(1,string.Empty);
            SoundSource3DObjPools.Add(SounEventName, templateObj);
        }

        bool havePublicObj = false;
        string pKey = string.Format("{0}_{1}", publicObjsEventKey, position.ToString());
        if (publicObjsEventKey > 0 && SoundSource3DPublicObj.TryGetValue(pKey, out soundObj))
        {
            havePublicObj = true;

            if (soundObj == null)
                SoundSource3DPublicObj.Remove(pKey);
        }

        if (!havePublicObj || soundObj == null)
        {
            GameObject templateobj = SoundSource3DObjPools[SounEventName];
            soundObj = templateobj.Spawn(SoundSourceObjRoot.transform);
        }
        soundObj.transform.position = position;

        //Debug.LogError("------------->PlaySound3D   gameObjID = " + AkSoundEngine.GetAkGameObjectID(soundObj) + " SounEventName = " + SounEventName);

        if (!SoundSource3DPublicObj.ContainsKey(pKey) && publicObjsEventKey > 0)
            SoundSource3DPublicObj.Add(pKey, soundObj);

        TAkGameObjEventMonitor eventMonitor = soundObj.GetOrAddComponent<TAkGameObjEventMonitor>();
        uint playId = AkSoundEngine.PostEvent(SounEventName, soundObj, (uint)AkCallbackType.AK_EndOfEvent, EventCallback3D, new SoundSource3DCallBackData(soundObj, BankName, pKey));
        eventMonitor.AddPlaying(SounEventName, playId, endCallback, userEndData);

        return playId;
    }

    /// <summary>
    /// 3D音效播放完毕回调
    /// </summary>
    /// <param name="cookie"> 参数</param>
    /// <param name="type">类型</param>
    /// <param name="callbackInfo"></param>
    private void EventCallback3D(object cookie, AkCallbackType type, AkCallbackInfo callbackInfo)
    {
        if (type == AkCallbackType.AK_EndOfEvent)
        {
            AkEventCallbackInfo info = callbackInfo as AkEventCallbackInfo;
            if (info != null)
            {
                // Debug.LogError("-------->event 结束！gameObjID = " + info.gameObjID);

                SoundSource3DCallBackData data = (SoundSource3DCallBackData)cookie;
                if (data != null && data.gameObject != null)
                {
                    if (!string.IsNullOrEmpty(data.bankName))
                    {
                        DelePlayIdCoutToBankDic(data.bankName);
                    }

                    if (data.gameObject != null)
                    {
                        TAkGameObjEventMonitor eventMonitor = data.gameObject.GetComponent<TAkGameObjEventMonitor>();
                        if (eventMonitor != null)
                        {
                            eventMonitor.DeleOneEvent(info.playingID);
                            if (eventMonitor.GetCurrentCout() == 0)
                            {
                                if (SoundSource3DPublicObj.ContainsKey(data.publicObjsEventKey))
                                    SoundSource3DPublicObj.Remove(data.publicObjsEventKey);
                                // Debug.LogWarning("-------->3D gameObject 回收！gameObjID = " + info.gameObjID);
                                data.gameObject.Recycle();
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 设置参数
    /// </summary>
    /// <param name="parametName"></param>
    /// <param name="value"></param>
    public bool SetParameter(string parametName, float value)
    {
        UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(value), "value is NaN");

        AKRESULT result = AkSoundEngine.SetRTPCValue(parametName, value);

        if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
        {
            UnityEngine.Debug.LogWarning(string.Format("WwiseUnity: 设置参数失败!  parametName = {0}，value = {1}", parametName, value));
        }
        return result == AKRESULT.AK_Success;
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    /// <param name="stateGroupName">状态所属组名</param>
    /// <param name="stateName">状态名</param>

    public void SetState(string stateGroupName, string stateName)
    {
        AKRESULT result = AkSoundEngine.SetState(stateGroupName, stateName);

        if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
        {
            UnityEngine.Debug.LogWarning(string.Format("WwiseUnity: 设置状态失败!  stateGroupName = {0}，stateName = {1}", stateGroupName, stateName));
        }
    }

    private void SetSwitch(string SwitchGroupName, string SwitchName, UnityEngine.GameObject gameObject)
    {
        AKRESULT result = AkSoundEngine.SetSwitch(SwitchGroupName, SwitchName, gameObject);

        if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
        {
            UnityEngine.Debug.LogWarning(string.Format("WwiseUnity: 设置Swithch 失败!  SwitchGroupName = {0}，SwitchName = {1}", SwitchGroupName, SwitchName));
        }
    }


    /// <summary>
    /// 2D 声音
    /// </summary>
    /// <param name="SounEventName">事件名字</param>
    /// 注意：没有挂点，音源则在 坐标 零点 位置
    /// <returns></returns>
    public uint PlaySound2DFormSwitch(string[] SwitchGroupNames, string[] SwitchNames, string SounEventName,Action<SystemObject> endAction = null , SystemObject userEndData = null)
    {

        for (int i = 0; i < SwitchGroupNames.Length && SwitchGroupNames.Length == SwitchNames.Length; i++)
        {
            string SwitchGroupName = SwitchGroupNames[i];
            string SwitchName = SwitchNames[i];

            if (!string.IsNullOrEmpty(SwitchGroupName) && !string.IsNullOrEmpty(SwitchName))
                SetSwitch(SwitchGroupName, SwitchName, SoundSource2DObj);
        }

        return PlaySound2D(SounEventName, endAction, userEndData);
    }

    /// <summary>
    /// 3D 声音
    /// </summary>
    /// <param name="SwitchGroupNames">Switch组名</param>
    /// <param name="SwitchNames">Switch名</param>
    /// <param name="SounEventName">事件名字</param>
    /// <param name="bankName">bank名字</param>
    /// <param name="publicObjsEventKey">公共物体事件Key标记</param>
    /// <param name="SoundParent">3D 声音挂点</param>
    ///  注意：没有挂点，音源则在 坐标 零点 位置
    /// <returns></returns>
    public uint PlaySound3DFormSwitch(string[] SwitchGroupNames, string[] SwitchNames, string SounEventName, string bankName, int publicObjsEventKey, Transform SoundParent = null,Action<SystemObject> endAction = null, SystemObject userEndData = null)
    {
        if (SoundParent == null)
        {
            Debug.LogWarning(string.Format("3D 音效，挂点却为null，不予播放，请检查!!! SounEventName = {0}", SounEventName));
            return 0;
        }

        GameObject soundObj = null;
        if (!SoundSource3DObjPools.ContainsKey(SounEventName))
        {
            GameObject templateObj = new GameObject(string.Format("3DSound_{0}", SounEventName));
            templateObj.transform.SetParent(SoundSource3DObj_TemplateRoot.transform, false);
            templateObj.CreatePool(1,string.Empty);
            SoundSource3DObjPools.Add(SounEventName, templateObj);
        }

        bool havePublicObj = false;
        string pKey = string.Format("{0}_{1}", publicObjsEventKey, SoundParent.GetInstanceID());
        if (publicObjsEventKey > 0 && SoundSource3DPublicObj.TryGetValue(pKey, out soundObj))
        {
            havePublicObj = true;

            if (soundObj == null)
                SoundSource3DPublicObj.Remove(pKey);
        }

        if (!havePublicObj || soundObj == null)
        {
            GameObject templateobj = SoundSource3DObjPools[SounEventName];
            soundObj = templateobj.Spawn();
        }
        soundObj.transform.SetParent(SoundParent == null ? SoundSourceObjRoot.transform : SoundParent, false);

        //Debug.LogWarning("------------->PlaySound3DFormSwitch   gameObjID = " + AkSoundEngine.GetAkGameObjectID(soundObj) + " SounEventName = " + SounEventName + SoundParent.gameObject.GetHashCode());

        if (!SoundSource3DPublicObj.ContainsKey(pKey) && publicObjsEventKey > 0)
            SoundSource3DPublicObj.Add(pKey, soundObj);

        for (int i = 0; i < SwitchGroupNames.Length && SwitchGroupNames.Length == SwitchNames.Length; i++)
        {
            string SwitchGroupName = SwitchGroupNames[i];
            string SwitchName = SwitchNames[i];

            if (!string.IsNullOrEmpty(SwitchGroupName) && !string.IsNullOrEmpty(SwitchName))
                SetSwitch(SwitchGroupName, SwitchName, soundObj);
        }

        TAkGameObjEventMonitor eventMonitor = soundObj.GetOrAddComponent<TAkGameObjEventMonitor>();
        uint playId = AkSoundEngine.PostEvent(SounEventName, soundObj, (uint)AkCallbackType.AK_EndOfEvent, EventCallback3D, new SoundSource3DCallBackData(soundObj, bankName, pKey));
        eventMonitor.AddPlaying(SounEventName, playId, endAction, userEndData);

        return playId;
    }

    /// <summary>
    /// 3D 声音
    /// </summary>
    /// <param name="SounEventName">事件名字</param>
    /// <param name="bankName">bank名字</param>
    ///<param name="publicObjsEventKey">公共物体事件Key标记</param>
    /// <param name="position">播放位置</param>
    /// <returns></returns>
    public uint PlaySound3DFormSwitch(string[] SwitchGroupNames, string[] SwitchNames, string SounEventName, string bankName, int publicObjsEventKey, Vector3 position,Action<SystemObject> endAction = null, SystemObject userEndData = null)
    {
        GameObject soundObj = null;
        if (!SoundSource3DObjPools.ContainsKey(SounEventName))
        {
            GameObject templateObj = new GameObject(string.Format("3DSound_{0}", SounEventName));
            templateObj.transform.SetParent(SoundSource3DObj_TemplateRoot.transform, false);
            templateObj.CreatePool(1,string.Empty);
            SoundSource3DObjPools.Add(SounEventName, templateObj);
        }

        bool havePublicObj = false;
        string pKey = string.Format("{0}_{1}", publicObjsEventKey, position.ToString());
        if (publicObjsEventKey > 0 && SoundSource3DPublicObj.TryGetValue(pKey, out soundObj))
        {
            havePublicObj = true;

            if (soundObj == null)
                SoundSource3DPublicObj.Remove(pKey);
        }

        if (!havePublicObj || soundObj == null)
        {
            GameObject templateobj = SoundSource3DObjPools[SounEventName];
            soundObj = templateobj.Spawn(SoundSourceObjRoot.transform);
        }
        soundObj.transform.position = position;

        //  Debug.LogWarning("------------->PlaySound3DFormSwitch   gameObjID = " + AkSoundEngine.GetAkGameObjectID(soundObj) + " SounEventName = " + SounEventName);

        if (!SoundSource3DPublicObj.ContainsKey(pKey) && publicObjsEventKey > 0)
            SoundSource3DPublicObj.Add(pKey, soundObj);

        for (int i = 0; i < SwitchGroupNames.Length && SwitchGroupNames.Length == SwitchNames.Length; i++)
        {
            string SwitchGroupName = SwitchGroupNames[i];
            string SwitchName = SwitchNames[i];

            if (!string.IsNullOrEmpty(SwitchGroupName) && !string.IsNullOrEmpty(SwitchName))
                SetSwitch(SwitchGroupName, SwitchName, soundObj);
        }

        TAkGameObjEventMonitor eventMonitor = soundObj.GetOrAddComponent<TAkGameObjEventMonitor>();
        uint playId = AkSoundEngine.PostEvent(SounEventName, soundObj, (uint)AkCallbackType.AK_EndOfEvent, EventCallback3D, new SoundSource3DCallBackData(soundObj, bankName, pKey));
        eventMonitor.AddPlaying(SounEventName, playId, endAction, userEndData);

        return playId;
    }


    /// <summary>
    /// 增加一个音效到bank 计数容器内
    /// </summary>
    public void AddPlayIdCoutToBankDic(string bankName, uint playId)
    {
        if (playId == 0)
            return;

        if (!PlayIdCoutAbountSoundBank.ContainsKey(bankName))
        {
            PlayIdCoutAbountSoundBank.Add(bankName, 1);
        }
        else
        {
            PlayIdCoutAbountSoundBank[bankName] = PlayIdCoutAbountSoundBank[bankName] + 1;
        }
    }

    /// <summary>
    /// 删除一个用到这个bank 的正在播放中的音效计数
    /// </summary>
    /// <param name="bankName"></param>
    private void DelePlayIdCoutToBankDic(string bankName)
    {
        if (!PlayIdCoutAbountSoundBank.ContainsKey(bankName))
            return;

        int cout = PlayIdCoutAbountSoundBank[bankName] - 1;
        if (cout > 0)
        {
            PlayIdCoutAbountSoundBank[bankName] = cout;
        }
        else
        {
            PlayIdCoutAbountSoundBank.Remove(bankName);

            if(!WWISE_FirstBankNameList.Contains(bankName))
                UnLoadSoundBank(bankName);
        }
    }

    /// <summary>
    /// 创建环境声源盒
    /// </summary>
    /// <param name="enterEventName">进入盒子 音效</param>
    /// <param name="leaveEventName">退出盒子 音效</param>
    /// <returns></returns>
    public GameObject CreatTAkAmbientSphereBox(string enterEventName, string leaveEventName)
    {
        GameObject obj = new GameObject("TAkAmbientBox");
        obj.layer =  LayerMask.NameToLayer("WwiseAmbient");
        obj.SetActive(false);

        SphereCollider collider = obj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;

        //进入
        TAkAmbient akAmbient_1 = obj.AddComponent<TAkAmbient>();
        akAmbient_1.m_AmbientEventType = TAkAmbient.AmbientEventType.enter;
        akAmbient_1.SetEventName(enterEventName);

        //离开
        TAkAmbient akAmbient_2 = obj.AddComponent<TAkAmbient>();
        akAmbient_2.m_AmbientEventType = TAkAmbient.AmbientEventType.leave;
        akAmbient_2.SetEventName(leaveEventName);
        obj.SetActive(true);

        return obj;
    }




    public void OnDestroy()
    {
        foreach (var item in SoundSource3DObjPools)
        {
            item.Value.RecycleAll();
        }
        SoundSource3DObjPools.Clear();

        foreach (var item in SoundNameAboutGameObject)
        {
            GameObject.Destroy(item.Value);
        }
        SoundNameAboutGameObject.Clear();

        foreach (var item in GameObjectAboutBanks)
        {
            foreach (var akBank in item.Value)
            {
                akBank.UnloadBank(null);
            }
            item.Value.Clear();
        }
        GameObjectAboutBanks.Clear();
        SoundSource3DPublicObj.Clear();
        PlayIdCoutAbountSoundBank.Clear();
        m_triggerMgr.Release();
        WWISE_FirstBankNameList.Clear();
    }

    #endregion end 函数区

    ////////////////////////////////////////////////////////////////////////分隔符/////////////////////////////////////////////////////////////////////////////////////////////////////
    #region 播放请使用 command 消息，为了结耦以下函数不允许播放调用！

    /// <summary>
    ///播放音乐音效
    /// 2D或者3D Transform 类型挂点的
    /// </summary>
    /// <param name="musicId">ID</param>
    /// <param name="alreadyPrepare">是否已经执行了预先加载事件 Prepare</param>
    /// <param name="SoundParent"> 如果是3D 的，就要传入挂点，否则零点处理</param>
    /// <returns></returns>
    public static void PlayMusicOrSound(int musicId, bool alreadyPrepare = false, Transform SoundParent = null, Action<SystemObject> endAction = null, SystemObject userEndData = null)
    {
        if (musicId < 0)
        {
            return;
        }

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        Sound? sound = cfgEternityProxy.GetSoundByKey((uint)musicId);
        bool haveSwitch = sound.Value.SwitchIdsLength > 0;
        string[] switchGroups = new string[sound.Value.SwitchIdsLength];
        string[] switchNames = new string[sound.Value.SwitchIdsLength];

        if (haveSwitch)
        {
            for (int i = 0; i < sound.Value.SwitchIdsLength; i++)
            {
                int switchID = sound.Value.SwitchIds(i);
                SoundSwitch? soundswitch = cfgEternityProxy.GetSoundSwitchByKey((uint)switchID);
                switchGroups[i] = soundswitch.Value.SwitchGroupName;
                switchNames[i] = soundswitch.Value.SwitchName;
            }
        }

        SoundEvent? soundevent = cfgEternityProxy.GetSoundEventDataByKey((uint)sound.Value.EventId);
        SoundBank? bank = cfgEternityProxy.GetSoundBankByKey((uint)soundevent.Value.BankID);
        if (AkBankManager.GetCurrentBankCount(bank.Value.BankName) == 0)
        {
            WwiseManager.Instance.LoadSoundBank(bank.Value.BankName, () =>
            {
                bool prepare = true;
                if (!soundevent.Value.HaveMedia)
                {
                    prepare = WwiseManager.Instance.LoadorUnLoadPrepareEvent(soundevent.Value.EventName);
                }

                if (prepare)
                {
                    if (soundevent.Value.Type)
                    {
                        uint playId = haveSwitch ? WwiseManager.Instance.PlaySound3DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, SoundParent, endAction, userEndData)
                            : WwiseManager.Instance.PlaySound3D(soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, SoundParent, endAction, userEndData);
                        WwiseManager.Instance.AddPlayIdCoutToBankDic(bank.Value.BankName, playId);
                    }
                    else
                    {
                        uint playId = haveSwitch ? WwiseManager.Instance.PlaySound2DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, endAction, userEndData)
                            : WwiseManager.Instance.PlaySound2D(soundevent.Value.EventName, endAction, userEndData);
                    }
                }
            });
        }
        else
        {
            bool prepare = true;
            if (!soundevent.Value.HaveMedia && !alreadyPrepare)
            {
                prepare = WwiseManager.Instance.LoadorUnLoadPrepareEvent(soundevent.Value.EventName);
            }

            if (prepare)
            {
                if (soundevent.Value.Type)
                {
                    uint playId = haveSwitch ? WwiseManager.Instance.PlaySound3DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, SoundParent, endAction, userEndData)
                        : WwiseManager.Instance.PlaySound3D(soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, SoundParent, endAction, userEndData);
                    WwiseManager.Instance.AddPlayIdCoutToBankDic(bank.Value.BankName, playId);
                }
                else
                {
                    uint playId = haveSwitch ? WwiseManager.Instance.PlaySound2DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, endAction, userEndData)
                        : WwiseManager.Instance.PlaySound2D(soundevent.Value.EventName, endAction, userEndData);
                }
            }
        }
    }


    /// <summary>
    /// 播放音乐音效
    /// 专门为 3D  Vector3 坐标的使用，如果不是，请用重载函数
    /// </summary>
    /// <param name="musicId">ID</param>
    /// <param name="alreadyPrepare">是否已经执行了预先加载事件 Prepare</param>
    /// <param name="position"> 3D 的，声源坐标</param>
    /// <returns></returns>
    public static void PlayMusicOrSound(int musicId, bool alreadyPrepare, Vector3 position, Action<SystemObject> endAction = null , SystemObject userEndData = null)
    {
        if (musicId < 0)
        {
            return;
        }

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        Sound? sound = cfgEternityProxy.GetSoundByKey((uint)musicId);
        bool haveSwitch = sound.Value.SwitchIdsLength > 0;
        string[] switchGroups = new string[sound.Value.SwitchIdsLength];
        string[] switchNames = new string[sound.Value.SwitchIdsLength];

        if (haveSwitch)
        {
            for (int i = 0; i < sound.Value.SwitchIdsLength; i++)
            {
                int switchID = sound.Value.SwitchIds(i);
                SoundSwitch? soundswitch = cfgEternityProxy.GetSoundSwitchByKey((uint)switchID);
                switchGroups[i] = soundswitch.Value.SwitchGroupName;
                switchNames[i] = soundswitch.Value.SwitchName;
            }
        }

        SoundEvent? soundevent = cfgEternityProxy.GetSoundEventDataByKey((uint)sound.Value.EventId);
        SoundBank? bank = cfgEternityProxy.GetSoundBankByKey((uint)soundevent.Value.BankID);
        if (AkBankManager.GetCurrentBankCount(bank.Value.BankName) == 0)
        {
            WwiseManager.Instance.LoadSoundBank(bank.Value.BankName, () =>
            {
                bool prepare = true;
                if (!soundevent.Value.HaveMedia)
                {
                    prepare = WwiseManager.Instance.LoadorUnLoadPrepareEvent(soundevent.Value.EventName);
                }

                if (prepare)
                {
                    if (soundevent.Value.Type)
                    {
                        uint playId = haveSwitch ? WwiseManager.Instance.PlaySound3DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, position, endAction,userEndData)
                            : WwiseManager.Instance.PlaySound3D(soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, position, endAction, userEndData);
                        WwiseManager.Instance.AddPlayIdCoutToBankDic(bank.Value.BankName, playId);
                    }
                    else
                    {
                        // Debug.LogWarning(string.Format("!!!!!! 播放音效的是2D的，位置 参数position 无效! 请检查配置 musicId = {0}", musicId));
                        uint playId = haveSwitch ? WwiseManager.Instance.PlaySound2DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, endAction, userEndData)
                            : WwiseManager.Instance.PlaySound2D(soundevent.Value.EventName, endAction, userEndData);
                    }
                }
            });
        }
        else
        {
            bool prepare = true;
            if (!soundevent.Value.HaveMedia && !alreadyPrepare)
            {
                prepare = WwiseManager.Instance.LoadorUnLoadPrepareEvent(soundevent.Value.EventName);
            }

            if (prepare)
            {
                if (soundevent.Value.Type)
                {
                    uint playId = haveSwitch ? WwiseManager.Instance.PlaySound3DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, position, endAction, userEndData)
                        : WwiseManager.Instance.PlaySound3D(soundevent.Value.EventName, bank.Value.BankName, soundevent.Value.PublicGameObject, position, endAction, userEndData);
                    WwiseManager.Instance.AddPlayIdCoutToBankDic(bank.Value.BankName, playId);
                }
                else
                {
                    // Debug.LogWarning(string.Format("!!!!!! 播放音效的是2D的，位置 参数position 无效! 请检查配置 musicId = {0}", musicId));
                    uint playId = haveSwitch ? WwiseManager.Instance.PlaySound2DFormSwitch(switchGroups, switchNames, soundevent.Value.EventName, endAction, userEndData)
                        : WwiseManager.Instance.PlaySound2D(soundevent.Value.EventName, endAction, userEndData);
                }
            }
        }
    }

    /// <summary>
    /// 设置音效全局参数
    /// 注意：参数直接跳到目标值，不经过任何过渡，可能导致内存不足
    /// https://www.audiokinetic.com/zh/library/edge/?source=SDK&id=goingfurther__optimizingmempools.html
    /// </summary>
    /// <param name="parameter">参数枚举</param>
    /// <param name="value">参数值</param>
    public static bool SetParameter(WwiseRtpc parameter, float value)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundRtpc? rtpc = cfgEternityProxy.GetSoundRtpcByKey((uint)parameter);
        string rtpcName = rtpc.Value.RtpcName;
        if (string.IsNullOrEmpty(rtpcName))
        {
            Debug.LogWarning(string.Format("音效参数无效 null ! 检查表格 RtpcID = {0} ", (int)parameter));
            return false;
        }
        return WwiseManager.Instance.SetParameter(rtpcName, value);
    }

    /// <summary>
    /// 设置音效全局参数
    /// 注意：参数直接跳到目标值，不经过任何过渡，可能导致内存不足
    /// https://www.audiokinetic.com/zh/library/edge/?source=SDK&id=goingfurther__optimizingmempools.html
    /// </summary>
    /// <param name="parameter">参数枚举</param>
    /// <param name="value">参数值</param>
    public static bool SetParameter(int  RtpcID, float value)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundRtpc? rtpc = cfgEternityProxy.GetSoundRtpcByKey((uint)RtpcID);
        string rtpcName = rtpc.Value.RtpcName;
        if (string.IsNullOrEmpty(rtpcName))
        {
            Debug.LogWarning(string.Format("音效参数无效 null ! 检查表格 RtpcID = {0} ", (int)RtpcID));
            return false;
        }
        return WwiseManager.Instance.SetParameter(rtpcName, value);
    }

    /// <summary>
    /// 加载 / 卸载 音效组合
    /// </summary>
    /// <param name="ComboId">组合 ID</param>
    /// <param name="todoPrepareEvent">是否是需要执行 PrepareEvent</param>
    public static void LoadMusicCombo(int ComboId, bool todoPrepareEvent)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundComboData?[] soundCombos = cfgEternityProxy.GetSoundComboDataByKey(ComboId);

        List<int> bankList = new List<int>();
        List<string> EventList = new List<string>();

        for (int i = 0; i < soundCombos.Length; i++)
        {
            int musicId = soundCombos[i].Value.MusicId;
            if (musicId <= 0)
                continue;

            Sound? sound = cfgEternityProxy.GetSoundByKey((uint)musicId);
            SoundEvent? soundevent = cfgEternityProxy.GetSoundEventDataByKey((uint)sound.Value.EventId);
            if (!bankList.Contains(soundevent.Value.BankID))
                bankList.Add(soundevent.Value.BankID);
            if (!EventList.Contains(soundevent.Value.EventName) && !soundevent.Value.HaveMedia)
                EventList.Add(soundevent.Value.EventName);
        }

        //执行加载
        int loadbankOverCount = 0;       //计数
        for (int i = 0; i < bankList.Count; i++)
        {
            SoundBank? bank = cfgEternityProxy.GetSoundBankByKey((uint)bankList[i]);
            WwiseManager.Instance.LoadSoundBank(bank.Value.BankName, () =>
            {
                loadbankOverCount++;
                if (loadbankOverCount == bankList.Count)
                {
                    Debug.Log(string.Format(" 组合ID = {0} 所有的bank 列表加载全部OK ! ", ComboId));
                    if (!todoPrepareEvent)
                        return;

                    //加载预备event
                    bool eventOk = true;
                    for (int t = 0; t < EventList.Count; t++)
                    {
                        string eventName = EventList[t];
                        bool prepare = WwiseManager.Instance.LoadorUnLoadPrepareEvent(eventName, true);
                        eventOk = eventOk && prepare;
                        if (!prepare)
                        {
                            Debug.LogWarning(string.Format(" 组合ID = {0} 预先加载 event 失败 eveneName = ", ComboId, eventName));
                            continue;
                        }
                    }
                    if (eventOk)
                        Debug.Log(string.Format(" 组合ID = {0} 所有的预备event列表加载全部OK ! ", ComboId));
                }
            }
            );
        }
    }


    /// <summary>
    ///  卸载 音效组合
    /// </summary>
    /// <param name="ComboId">组合 ID</param>
    /// <param name="todoPrepareEvent">是否是需要执行 PrepareEvent</param>
    public static void UnLoadMusicCombo(int ComboId)
    {
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundComboData?[] soundCombos = cfgEternityProxy.GetSoundComboDataByKey(ComboId);

        List<int> bankList = new List<int>();
        List<string> EventList = new List<string>();

        for (int i = 0; i < soundCombos.Length; i++)
        {
            int musicId = soundCombos[i].Value.MusicId;
            if (musicId <= 0)
                continue;

            Sound? sound = cfgEternityProxy.GetSoundByKey((uint)musicId);
            SoundEvent? soundevent = cfgEternityProxy.GetSoundEventDataByKey((uint)sound.Value.EventId);
            if (!bankList.Contains(soundevent.Value.BankID))
                bankList.Add(soundevent.Value.BankID);
            if (!EventList.Contains(soundevent.Value.EventName) && !soundevent.Value.HaveMedia)
                EventList.Add(soundevent.Value.EventName);
        }

        //执行卸载,   注意 卸载只是当音效停止状态才能真正执行
        for (int i = 0; i < EventList.Count; i++)
        {
            WwiseManager.Instance.LoadorUnLoadPrepareEvent(EventList[i], false);
        }
        for (int t = 0; t < bankList.Count; t++)
        {
            SoundBank? bank = cfgEternityProxy.GetSoundBankByKey((uint)bankList[t]);
            WwiseManager.Instance.UnLoadSoundBank(bank.Value.BankName);
        }
    }


    /// <summary>
    /// 播放特殊枚举类型音效
    /// 注：必须属于组合表内表明定义的
    /// </summary>
    /// <param name="ComboId">组合Id</param>
    /// <param name="type">类型枚举</param>
    /// <param name="place">主角or其他 --玩家跟主角音效不同,目前只有船有需求</param>
    ///<param name="alreadyPrepare">是否已经执行了预先加载事件 Prepare</param>
    /// <param name="SoundParent">挂点</param>
    public static void PlaySpecialTypeMusicOrSound(int ComboId, WwiseMusicSpecialType type, WwiseMusicPalce place, bool alreadyPrepare = false, Transform SoundParent = null, Action<object> endAction = null, SystemObject userEndData = null)
    {
        if (ComboId <= 0)
            return;
        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundComboData?[] soundCombos = cfgEternityProxy.GetSoundComboDataByKey(ComboId);

        int musicId = -1;
        for (int i = 0; i < soundCombos.Length; i++)
        {
            if (soundCombos[i].Value.Type == (int)type && soundCombos[i].Value.Place == (int)place)
            {
                musicId = soundCombos[i].Value.MusicId;
                break;
            }
        }

        if (musicId <= 0)
        {
            Debug.LogWarning(string.Format("检查表SoundCombo 表，数据不对 ComboId = {0}, type = {1}", ComboId, (int)type));
            return;
        }

        PlayMusicOrSound(musicId, alreadyPrepare, SoundParent, endAction);
    }


    /// <summary>
    /// 播放特殊枚举类型音效
    /// 注：必须属于组合表内表明定义的
    /// </summary>
    /// <param name="ComboId">组合Id</param>
    /// <param name="type">类型枚举</param>
    /// <param name="place">主角or其他 --玩家跟主角音效不同,目前只有船有需求</param>
    ///<param name="alreadyPrepare">是否已经执行了预先加载事件 Prepare</param>
    /// <param name="point">播放位置</param>
    public static void PlaySpecialTypeMusicOrSound(int ComboId, WwiseMusicSpecialType type, WwiseMusicPalce place, bool alreadyPrepare, Vector3 point, Action<object> endAction = null, SystemObject userEndData = null)
    {
        if (ComboId <= 0)
            return;

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        SoundComboData?[] soundCombos = cfgEternityProxy.GetSoundComboDataByKey(ComboId);

        int musicId = -1;
        for (int i = 0; i < soundCombos.Length; i++)
        {
            if (soundCombos[i].Value.Type == (int)type && soundCombos[i].Value.Place == (int)place)
            {
                musicId = soundCombos[i].Value.MusicId;
                break;
            }
        }

        if (musicId <= 0)
        {
            Debug.LogWarning(string.Format("检查表SoundCombo 表，数据不对 ComboId = {0}, type = {1}", ComboId, (int)type));
            return;
        }
        PlayMusicOrSound(musicId, alreadyPrepare, point, endAction);
    }

    /// <summary>
    /// 获取EventName
    /// </summary>
    /// <param name="musicId"></param>
    /// <returns></returns>
    public static string GetEventName(int musicId)
    {
        if (musicId < 0)
        {
            return string.Empty;
        }

        CfgEternityProxy cfgEternityProxy = GameFacade.Instance.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
        Sound? sound = cfgEternityProxy.GetSoundByKey((uint)musicId);

        SoundEvent? soundevent = cfgEternityProxy.GetSoundEventDataByKey((uint)sound.Value.EventId);
        return soundevent.Value.EventName;
    }


    /// <summary>
    /// 创建环境声源盒
    /// </summary>
    /// <param name="enterEventName">进入盒子 音效</param>
    /// <param name="leaveEventName">退出盒子 音效</param>
    /// <returns></returns>
    public static GameObject CreatTAkAmbientSphereBox(int enterSoundId, int leaveSoundId)
    {
        string enterEventName = GetEventName(enterSoundId);
        string leaveEventName = GetEventName(leaveSoundId);
        if(!string.IsNullOrEmpty(enterEventName) && !string.IsNullOrEmpty(leaveEventName))
        {
            return WwiseManager.Instance.CreatTAkAmbientSphereBox(enterEventName , leaveEventName);
        }
        return null;
    }


    #endregion  end 静态函数
}

