using DG.Tweening;
using Eternity.FlatBuffer;
using PureMVC.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePanel : UIPanelBase
{
	private static string ASSET_ADDRESS = "Assets/Artwork/UI/Prefabs/NpcTalkPanel.prefab";
	private static readonly string LANGUAGE_PREFIX = "dialogue_talk_";
	private static readonly float SECTION_SPACE = 2.0F;
	private static readonly float CHARS_SPACE = 0.01F;

	private CfgEternityProxy m_CfgEternityProxy;
	private CanvasGroup m_CanvasGroup;
	private TMP_Text m_Label;
	private Queue<TextData> m_DataList;
	private bool m_IsUsing;
	private IEnumerator m_StartFunc;
	private float m_StartTime;

	private DialogueInfo m_Data;

	public DialoguePanel() : base(UIPanel.DialoguePanel, ASSET_ADDRESS, PanelType.Dialugue)
	{
	}

	public override void Initialize()
	{
		base.Initialize();

        IsPermanent = true;

		m_CfgEternityProxy = Facade.RetrieveProxy(ProxyName.CfgEternityProxy) as CfgEternityProxy;
		m_DataList = new Queue<TextData>();
		m_CanvasGroup = FindComponent<CanvasGroup>("Content");
		m_Label = FindComponent<TMP_Text>("Content/Image_Bg/Label");
		GetGameObject().SetActive(false);
	}

	public override void HandleNotification(INotification notification)
	{
		switch (notification.Name)
		{
			case NotificationName.MSG_DIALOGUE_SHOW:
				CheckAndShow(notification.Body as DialogueInfo);
				break;
			case NotificationName.MSG_DIALOGUE_HIDE:
				Hide();
				break;
		}
	}

	public override NotificationName[] ListNotificationInterests()
	{
		return new NotificationName[]
		{
			NotificationName.MSG_DIALOGUE_SHOW,
			NotificationName.MSG_DIALOGUE_HIDE,
		};
	}

	public override void OnShow(object msg)
	{
		base.OnShow(msg);
		GetGameObject().SetActive(false);
	}

	public void CheckAndShow(DialogueInfo data)
	{
		if (data.DialogueTid == 0)
		{
			ClosePanel();
			return;
		}
		Show();

		if (m_IsUsing)
		{
			if (ServerTimeUtil.Instance.GetNowTime() - m_StartTime >= 3)
			{
				return;
			}
			SendDialogueCompleted();
			UIManager.Instance.StopCoroutine(m_StartFunc);
			m_IsUsing = false;
			m_Data = data;
			//Hide(() =>
				//{
					Close(false);
				//}
			//);
			return;
		}
		m_Data = data;

		CreateData(data.DialogueTid, data.SoundParent, data.SoundPoint);
		StartDialogue();
	}

	private void CreateData(uint tid, Transform soundParent, Vector3? soundPoint)
	{
		m_DataList.Clear();
		while (tid != 0)
		{
			Dialogue? config = m_CfgEternityProxy.GetDialogueByKey(tid);
			if (config.HasValue)
			{
				TextData info = new TextData();
				info.Text = TableUtil.GetLanguageString(LANGUAGE_PREFIX + tid);
				info.SoundID = config.Value.SoundID;
				info.SoundParent = soundParent;
				info.SoundPoint = soundPoint;
				m_DataList.Enqueue(info);
				tid = (uint)config.Value.ContinueId;
			}
			else
			{
				tid = 0;
			}
		}
	}

	private void StartDialogue()
	{
		m_StartFunc = ShowText();
		m_StartTime = ServerTimeUtil.Instance.GetNowTime();
		UIManager.Instance.StartCoroutine(m_StartFunc);
		SendDialogueCompleted();
	}

	private IEnumerator ShowText()
	{
		while (m_DataList.Count > 0)
		{
			m_IsUsing = true;
			TextData data = m_DataList.Dequeue();
			m_Label.maxVisibleCharacters = 0;
			m_Label.text = data.Text;
			PlaySound(data.SoundID, data.SoundParent, data.SoundPoint);
			yield return new WaitForEndOfFrame();
			while (m_Label.maxVisibleCharacters < m_Label.textInfo.characterCount)
			{
				yield return new WaitForSecondsRealtime(CHARS_SPACE);
				m_Label.maxVisibleCharacters++;
			}
			yield return new WaitForSecondsRealtime(SECTION_SPACE);
		}
		m_IsUsing = false;
		Close();
	}

	private void PlaySound(int soundId, Transform soundParent, Vector3? soundPoint)
	{
		if (soundId != 0)
		{
			//TODO 王辰兴 加播放声音
		}
	}

	private void StopSound()
	{
		//TODO 王辰兴 主动触发的停止声音
	}

	private void Show(Action action = null)
	{
		GetGameObject().SetActive(true);
		m_CanvasGroup.DOFade(1F, 0.1F).OnComplete(
			() =>
			{
				action?.Invoke();
			}
		);
	}

	private void Hide(Action action = null)
	{
		m_CanvasGroup.DOFade(0F, 0.1F).OnComplete(
			() =>
			{
				action?.Invoke();
			}
		);
	}

	public void Close(bool isClose = true)
	{
		m_StartTime = 0;
		UIManager.Instance.StopCoroutine(m_StartFunc);
		if (isClose)
		{
			//SendDialogueCompleted();
			Hide(ClosePanel);
		}
		else
		{
			CheckAndShow(m_Data);
		}
	}

	private void ClosePanel()
	{
		m_Data = null;
		m_IsUsing = false;
		m_StartTime = 0;
		GetGameObject().SetActive(false);
	}

	private void SendDialogueCompleted()
	{
		if (m_Data != null && m_Data.NeedSendToServer)
		{
			NetworkManager.Instance.GetMissionController().SendNpcTalk(m_Data.NpcTid,m_Data.DialogueTid);
		}
	}

	class TextData
	{
		public string Text;
		public int SoundID;
		public Transform SoundParent;
		public Vector3? SoundPoint;
	}
}