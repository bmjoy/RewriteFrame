using Eternity.FlatBuffer;
using PureMVC.Patterns.Proxy;
using UnityEngine.Assertions;

public partial class CfgEternityProxy : Proxy
{

    /// <summary>
    /// 获得音效组合
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public SoundComboData?[] GetSoundComboDataByKey(int tid)
    {
        SoundCombo? soundcombo = m_Config.SoundCombosByKey((uint)tid);
        Assert.IsTrue(soundcombo.HasValue, "CfgEternityProxy => GetSoundComboDataByKey not exist tid " + tid);
        SoundComboData?[] Arraydate = new SoundComboData?[soundcombo.Value.DatasLength];
        int n = soundcombo.Value.DatasLength;
        for (int i = 0; i < n; i++)
        {
            Arraydate[i] = soundcombo.Value.Datas(i);
        }
        return Arraydate;
    }

	/// <summary>
	/// 获得音效组合
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public SoundComboData GetSoundComboDataByKeyAndType(int tid, int type)
	{
		SoundCombo? soundcombo = m_Config.SoundCombosByKey((uint)tid);
		Assert.IsTrue(soundcombo.HasValue, "CfgEternityProxy => GetSoundComboDataByKey not exist tid " + tid);
		SoundComboData? data = null;
		int n = soundcombo.Value.DatasLength;
		for (int i = 0; i < n; i++)
		{
			if (soundcombo.Value.Datas(i).Value.Type == type)
			{
				data = soundcombo.Value.Datas(i);
				break;
			}
		}
		Assert.IsTrue(data.HasValue, "CfgEternityProxy => GetSoundComboDataByKeyAndType not exist tid " + tid + " type:" + type);
		return data.Value;
	}

	/// <summary>
	/// 获得音效
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public Sound? GetSoundByKey(uint tid)
    {
        Sound? sound = m_Config.SoundsByKey(tid);
        Assert.IsTrue(sound.HasValue, "CfgEternityProxy => GetSoundByKey not exist tid " + tid);
        return sound;
    }

    /// <summary>
    /// 获得音效事件
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public SoundEvent? GetSoundEventDataByKey(uint tid)
    {
        SoundEvent? soundevent = m_Config.SoundEventsByKey(tid);
        Assert.IsTrue(soundevent.HasValue, "CfgEternityProxy => GetSoundEventDataByKey not exist tid " + tid);
        return soundevent;
    }

	/// <summary>
	/// 获得音效事件
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public SoundEvent GetSoundEvent(uint tid)
	{
		SoundEvent? soundevent = m_Config.SoundEventsByKey(tid);
		Assert.IsTrue(soundevent.HasValue, "CfgEternityProxy => GetSoundEventDataByKey not exist tid " + tid);
		return soundevent.Value;
	}

	/// <summary>
	/// 获取库
	/// </summary>
	/// <param name="tid"></param>
	/// <returns></returns>
	public SoundBank? GetSoundBankByKey(uint tid)
    {
        SoundBank? soundbank = m_Config.SoundBanksByKey(tid);
        Assert.IsTrue(soundbank.HasValue, "CfgEternityProxy => GetSoundBankByKey not exist tid " + tid);
        return soundbank;
    }

    /// <summary>
    /// 获取swtich
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public SoundSwitch? GetSoundSwitchByKey(uint tid)
    {
        SoundSwitch? soundswitch = m_Config.SoundSwitchsByKey(tid);
        Assert.IsTrue(soundswitch.HasValue, "CfgEternityProxy => GetSoundSwitchByKey not exist tid " + tid);
        return soundswitch;
    }

    /// <summary>
    /// 获取热键音效
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public SoundHotkey? GetSoundHotkeyByKey(string tid)
    {
        SoundHotkey? soundHotKey = m_Config.SoundHotkeysByKey(tid);
        Assert.IsTrue(soundHotKey.HasValue, "CfgEternityProxy => GetSoundHotkeyByKey not exist tid " + tid);
        return soundHotKey;
    }

    /// <summary>
    /// 全局开关
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public SoundRtpc? GetSoundRtpcByKey(uint tid)
    {
        SoundRtpc? soundrtpc = m_Config.SoundRtpcsByKey(tid);
        Assert.IsTrue(soundrtpc.HasValue, "CfgEternityProxy => GetSoundRtpcByKey not exist tid " + tid);
        return soundrtpc;
    }

}