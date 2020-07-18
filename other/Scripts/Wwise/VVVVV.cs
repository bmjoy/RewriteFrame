/*===============================
 * Author: [Allen]
 * Purpose: 给音效同学测试用的！
 * Time: 2019/05/31  15:25
================================*/
using UnityEngine;
using System.Collections.Generic;

public class VVVVV : MonoBehaviour
{
    private uint SoundID = 0;

    private float hSliderValue_All = 50;//总音量
    private float hSliderValue_effect = 50;//音效音量
    private float hSliderValue_music = 50;//音乐音量

    //[Header("组合ID")]
    //public int ZuheID;

    //[Header("SwitchGroup 队列，注意：同样Group 下，不同Name，也请补全Group")]
    //public List<string> SwitchGroupList = new List<string>();

    //[Header("Switch名字 队列")]
    //public List<string> SwitchNameList = new List<string>();

    //[Header("Switch 的 Event ")]
    //public string SwitchEventName;

    //[Header("表格music ID ")]
    //public int MusicID;



    void aaa()
    {
        Debug.LogError("--------------> Ok  ");
    }


    private void OnGUI()
    {
        float SX = Screen.width;

        ////////////////////////////////////////////////////////////////////////分隔符/////////////////////////////////////////////////////////////////////////////////////////////////////
        GUIStyle titleStyle2 = new GUIStyle();
        titleStyle2.fontSize = 20;
        titleStyle2.normal.textColor = new Color(139 / 255f, 0 / 255f, 139 / 255f, 255f / 255f);

        titleStyle2.fontSize = 20;
        titleStyle2.normal.textColor = new Color(139 / 255f, 0 / 255f, 139 / 255f, 255f / 255f);

        GUI.Label(new Rect(0, 200, 100, 40), string.Format("总音量：{0}", hSliderValue_All), titleStyle2);
        hSliderValue_All = GUI.HorizontalSlider(new Rect(0, 250, 200, 30), hSliderValue_All, 0, 100);
        WwiseManager.Instance.SetParameter("BusVolume_Master", hSliderValue_All);

        GUI.Label(new Rect(0, 260, 100, 40), string.Format("音效音量：{0}", hSliderValue_effect), titleStyle2);
        hSliderValue_effect = GUI.HorizontalSlider(new Rect(0, 300, 200, 30), hSliderValue_effect, 0, 100);
        WwiseManager.Instance.SetParameter("BusVolume_SFX", hSliderValue_effect);

        GUI.Label(new Rect(0, 310, 100, 40), string.Format("音乐音量：{0}", hSliderValue_music), titleStyle2);
        hSliderValue_music = GUI.HorizontalSlider(new Rect(0, 350, 200, 30), hSliderValue_music, 0, 100);
        WwiseManager.Instance.SetParameter("BusVolume_Music", hSliderValue_music);

        ////////////////////////////////////////////////////////////////////////分隔符/////////////////////////////////////////////////////////////////////////////////////////////////////

        if (GUI.Button(new Rect(0, 100, 100, 50), "播放音效3000"))
        {
            WwiseUtil.PlaySound(7001, false, null);
        }

        if (GUI.Button(new Rect(100, 100, 100, 50), "播放音效3002"))
        {
            WwiseUtil.PlaySound(7017, false, null);
        }

        //if (GUI.Button(new Rect(100, 100, 200, 50), "指定Event播放带switch的"))
        //{
        //    string[] Groups = SwitchGroupList.ToArray();
        //    string[] names = SwitchNameList.ToArray();

        //    // WwiseManager.Instance.PlaySound3DFormSwitch(new string[] { "O_Ground", "O_Armor" }, new string[] { "O_Ground_Glass", "O_Armor_Ayer" }, "Play_Foley_PlayerStep");
        //    // WwiseManager.Instance.PlaySound3DFormSwitch(Groups, names, SwitchEventName,0);
        //}
        //if (GUI.Button(new Rect(300, 100, 100, 50), "表格Event播放"))
        //{
        //    WwiseUtil.PlayMusicOrSound(MusicID);
        //}
    }
}

