using PureMVC.Interfaces;
using PureMVC.Patterns.Command;
using UnityEngine;
using static ConfirmPanel;

/// <summary>
/// 退出游戏
/// </summary>
public class ExitGameCommand : SimpleCommand
{
	public override void Execute(INotification notification)
	{
		OnConfirmExit();
	}
	/// <summary>
	/// 确认退出
	/// </summary>
	private void OnConfirmExit()
	{
		OpenParameter openParameter = new OpenParameter();
		openParameter.Title = TableUtil.GetLanguageString("common_text_id_1018");
		openParameter.Content = TableUtil.GetLanguageString("common_text_id_1016");
		openParameter.backgroundColor = BackgroundColor.Normal;
		HotKeyButton HotKeyCancel = new HotKeyButton();
		HotKeyCancel.actionName = HotKeyID.FuncB;
		HotKeyCancel.showText = TableUtil.GetLanguageString("common_hotkey_id_1002");
		HotKeyCancel.onEvent = ConfirmPanelClose;
		HotKeyButton HotKeyQuit = new HotKeyButton();
		HotKeyQuit.actionName = HotKeyID.FuncX;
		HotKeyQuit.showText = TableUtil.GetLanguageString("common_hotkey_id_1001");
		HotKeyQuit.onEvent = ConfirmExitGame;
		openParameter.HotkeyArray = new HotKeyButton[] { HotKeyQuit,HotKeyCancel};//esc  退出游戏，取消
		UIManager.Instance.OpenPanel(UIPanel.ConfirmPanel, openParameter);
	}

	/// <summary>
	/// 关闭ConfirmPanel 面板
	/// </summary>
	/// <param name="callback"></param>
	private void ConfirmPanelClose(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			UIManager.Instance.ClosePanel((ConfirmPanel)Facade.RetrieveMediator(UIPanel.ConfirmPanel));
		}
	}

	/// <summary>
	/// 确认退出
	/// </summary>
	/// <param name="callback"></param>
	private void ConfirmExitGame(HotkeyCallback callback)
	{
		if (callback.performed)
		{
			NetworkManager.Instance.GetLoginController().ExitCurrentServer(3);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
		}
	}
}
