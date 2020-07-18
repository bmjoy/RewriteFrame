///*===============================
// * Author: [dinghuilin]
// * Purpose: SetTextCard.cs
// * Time: 2019/03/22  14:16
//================================*/
//using TMPro;
//using UnityEngine;
///// <summary>
/////SetTextCard 
///// </summary>
//public class SetTextElement : SetElementBase
//{
//	/// <summary>
//	/// Card名字
//	/// </summary>
//	private TMP_Text m_CardName;
//	/// <summary>
//	/// 初始化
//	/// </summary>
//	/// <param name="setVO">设置VO</param>
//	/// <param name="proxy">设置proxy</param>
//	public void Init(SetVO setVO, CfgSettingProxy proxy)
//	{
//		m_CardName = transform.Find("Name").GetComponent<TMP_Text>();
//		m_CardName.text = proxy.GetLocalization(SystemLanguage.English, setVO.Id);
//		transform.localScale = Vector3.one;
//	}
//}
