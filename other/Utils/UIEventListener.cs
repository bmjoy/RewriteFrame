/*===============================
 * Author: [Allen]
 * Purpose: UI 按钮事件监听
 * Time: 2018/08/21  15:22
================================*/
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
namespace UIEventListener
{

	public class UIEventListener : EventTrigger
	{
		public delegate void VoidDelegate(GameObject go, params object[] objs);


		#region  委托定义
		private VoidDelegate _onClick;
		private VoidDelegate _onDown;
		private VoidDelegate _onEnter;
		private VoidDelegate _onExit;
		private VoidDelegate _onUp;
		private VoidDelegate _onDrag;
		private VoidDelegate _onRightDown;
		private VoidDelegate _onRightUp;
		private VoidDelegate _onMove;
		private VoidDelegate _onRightClick;
		private VoidDelegate _onSelect;
		private VoidDelegate _onCancelSelect;
		private VoidDelegate _onEnterTips;
		private VoidDelegate _onDragTips;



		public enum EDelegateTpye
		{
			Tpye_onClick,
			Tpye_onDown,
			Tpye_onEnter,
			Tpye_onExit,
			Tpye_onUp,
			Tpye_onDrag,
			Tpye_onMove,
			Tpye_onRightClick,
			Type_onRightUp,
			Type_onRightDown,
			Type_onSelect,
			Type_onCancelSelect,
			Type_onEnterTips,
			Type_onDragTips,
		}

		#endregion

		#region  逻辑变量
		//双击
		private float endtime = 0;
		private float doubleClickTime = 0.5f;                        //响应时间

		// 长按
		public float durationThreshold = 1.0f;             //响应时间
		private bool bDown = false;
		private int nLangDelegate = -1;
		private float fLangtime = 0;

		public object[] parames;                                 //参数

		public int soundId = -1;                                    //音效ID

		private Dictionary<EDelegateTpye, object[]> parameDic = new Dictionary<EDelegateTpye, object[]>(); //参数容器
		private Dictionary<EDelegateTpye, int> soundDic = new Dictionary<EDelegateTpye, int>();                    //音效容器

		#endregion


		/// <summary>
		/// 单机 ~按下和释放在同一对象
		/// </summary>
		public VoidDelegate onRightClick
		{
			get
			{
				return _onRightClick;
			}
			set
			{
				_onRightClick = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onRightClick] = M_parames;
				soundDic[EDelegateTpye.Tpye_onRightClick] = soundId;
			}
		}

		/// <summary>
		/// 单机 ~按下和释放在同一对象
		/// </summary>
		public VoidDelegate onClick
		{
			get
			{
				return _onClick;
			}
			set
			{
				_onClick = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onClick] = M_parames;
				soundDic[EDelegateTpye.Tpye_onClick] = soundId;
			}
		}
		/// <summary>
		/// 指针按下
		/// </summary>
		public VoidDelegate onDown
		{
			get
			{
				return _onDown;
			}
			set
			{
				_onDown = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onDown] = M_parames;
				soundDic[EDelegateTpye.Tpye_onDown] = soundId;

			}
		}
		/// <summary>
		/// 右键按下
		/// </summary>
		public VoidDelegate onRightDown
		{
			get
			{
				return _onRightDown;
			}
			set
			{
				_onRightDown = value;
				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onRightDown] = M_parames;
				soundDic[EDelegateTpye.Type_onRightDown] = soundId;
			}
		}
		/// <summary>
		/// 右键抬起
		/// </summary>
		public VoidDelegate onRightUp
		{
			get
			{
				return _onRightUp;
			}
			set
			{
				_onRightUp = value;
				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onRightUp] = M_parames;
				soundDic[EDelegateTpye.Type_onRightUp] = soundId;
			}
		}

		/// <summary>
		/// 指针进入
		/// </summary>
		public VoidDelegate onEnter
		{
			get
			{
				return _onEnter;
			}
			set
			{
				_onEnter = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onEnter] = M_parames;
				soundDic[EDelegateTpye.Tpye_onEnter] = soundId;
			}
		}

		/// <summary>
		/// 指针进入
		/// </summary>
		public VoidDelegate onEnterTips
		{
			get
			{
				return _onEnterTips;
			}
			set
			{
				_onEnterTips = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onEnterTips] = M_parames;
				soundDic[EDelegateTpye.Type_onEnterTips] = soundId;
			}
		}


		/// <summary>
		/// 指针退出
		/// </summary>
		public VoidDelegate onExit
		{
			get
			{
				return _onExit;
			}
			set
			{
				_onExit = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onExit] = M_parames;
				soundDic[EDelegateTpye.Tpye_onExit] = soundId;
			}
		}
		/// <summary>
		/// 指针释放
		/// </summary>
		public VoidDelegate onUp
		{
			get
			{
				return _onUp;
			}
			set
			{
				_onUp = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onUp] = M_parames;
				soundDic[EDelegateTpye.Tpye_onUp] = soundId;
			}
		}

		/// <summary>
		/// 指针按下拖动
		/// </summary>
		public VoidDelegate onDrag
		{
			get
			{
				return _onDrag;
			}
			set
			{
				_onDrag = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onDrag] = M_parames;
				soundDic[EDelegateTpye.Tpye_onDrag] = soundId;
			}
		}

		/// <summary>
		/// 指针拖动
		/// </summary>
		public VoidDelegate onMove
		{
			get
			{
				return _onMove;
			}
			set
			{
				_onMove = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Tpye_onMove] = M_parames;
				soundDic[EDelegateTpye.Tpye_onMove] = soundId;
			}
		}

		/// <summary>
		/// 调用对象成为选定的对象
		/// </summary>
		public VoidDelegate onSelect
		{
			get
			{
				return _onSelect;
			}
			set
			{

				_onSelect = value;


				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onSelect] = M_parames;
				soundDic[EDelegateTpye.Type_onSelect] = soundId;
			}
		}

		/// <summary>
		/// 调用对象成为选定的对象
		/// </summary>
		public VoidDelegate onCancelSelect
		{
			get
			{
				return _onCancelSelect;
			}
			set
			{

				_onCancelSelect = value;


				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onCancelSelect] = M_parames;
				soundDic[EDelegateTpye.Type_onCancelSelect] = soundId;
			}
		}

		/// <summary>
		/// 指针进入
		/// </summary>
		public VoidDelegate onDragTips
		{
			get
			{
				return _onDragTips;
			}
			set
			{
				_onEnterTips = value;

				object[] M_parames = new object[parames.Length];
				Array.Copy(parames, 0, M_parames, 0, parames.Length);
				parameDic[EDelegateTpye.Type_onDragTips] = M_parames;
				soundDic[EDelegateTpye.Type_onDragTips] = soundId;
			}
		}




		public VoidDelegate onUpdateSelect;
		/// <summary>
		/// 双击 单双击同时又得花，第一点击单击启动
		/// </summary>
		public VoidDelegate onDoubleClick;
		/// <summary>
		/// 长按
		/// </summary>
		public VoidDelegate onLongPress;


		#region //==============================函数区========================================

		private void OnDestroy()
		{
			onClick = null;
			onDown = null;
			onEnter = null;
			onExit = null;
			onUp = null;
			onSelect = null;
			onDrag = null;
			onMove = null;
			onUpdateSelect = null;
			onDoubleClick = null;
			onLongPress = null;
			onRightDown = null;
			onRightUp = null;
			//audioProxy = null;todo

			parameDic.Clear();
			soundDic.Clear();
		}

		private object[] GetParam(EDelegateTpye tpye)
		{
			if (parameDic.ContainsKey(tpye))
				return parameDic[tpye];

			return null;
		}

		// 播放声音
		private void OnPlayMusic(EDelegateTpye tpye)
		{
			int musicid = -1;
			if (soundDic.ContainsKey(tpye))
				musicid = soundDic[tpye];
			
            if (musicid > 0)
            {
                WwiseUtil.PlaySound(musicid, false, null);
            }
        }


		static public UIEventListener AttachListener(GameObject go, params object[] objs)
		{
			UIEventListener listener = go.GetComponent<UIEventListener>();
			if (listener == null)
			{
				listener = go.AddComponent<UIEventListener>();
			}
			listener.parames = objs;

			return listener;
		}

		static public UIEventListener AttachListener(Transform transform, params object[] objs)
		{
			UIEventListener listener = transform.GetComponent<UIEventListener>();
			if (listener == null)
			{
				listener = transform.gameObject.AddComponent<UIEventListener>();
			}
			listener.parames = objs;

			return listener;
		}

        /// <summary>
        /// 设置监听
        /// </summary>
        /// <param name="go">GameObject</param>
        /// <param name="soundId">音效ID</param>
        /// <param name="objs">参数</param>
        /// <returns></returns>
        static public UIEventListener AttachListenerWithSound(GameObject go, int soundId = (int)WwiseMusic.Music_Button_Click_1, params object[] objs)
        {
            UIEventListener listener = go.GetOrAddComponent<UIEventListener>();
            listener.parames = objs;
            listener.soundId = soundId;
            return listener;
        }

        /// <summary>
        /// 设置监听
        /// </summary>
        /// <param name="transform">Transform</param>
        /// <param name="soundId">音效ID</param>
        /// <param name="objs">参数</param>
        /// <returns></returns>
        static public UIEventListener AttachListenerWithSound(Transform transform, int soundId = (int)WwiseMusic.Music_Button_Click_1, params object[] objs)
        {
            UIEventListener listener = transform.GetOrAddComponent<UIEventListener>();
            listener.parames = objs;
            listener.soundId = soundId;
            return listener;
        }

        public override void OnPointerDown(PointerEventData eventData)
		{
			if (onDown != null && eventData.button == PointerEventData.InputButton.Left)
			{
				onDown(gameObject, GetParam(EDelegateTpye.Tpye_onDown));
				OnPlayMusic(EDelegateTpye.Tpye_onDown);
				return;
			}
			else if (onRightDown != null && eventData.button == PointerEventData.InputButton.Right)
			{
				onRightDown(gameObject, GetParam(EDelegateTpye.Type_onRightDown));
				OnPlayMusic(EDelegateTpye.Type_onRightDown);
				return;
			}
			// 长按
			fLangtime = Time.realtimeSinceStartup;
			bDown = true;
			nLangDelegate = 0;
		}
		public override void OnPointerEnter(PointerEventData eventData)
		{
			onEnter?.Invoke(gameObject, GetParam(EDelegateTpye.Tpye_onEnter));
			OnPlayMusic(EDelegateTpye.Tpye_onEnter);

			onEnterTips?.Invoke(gameObject, GetParam(EDelegateTpye.Type_onEnterTips));
			OnPlayMusic(EDelegateTpye.Type_onEnterTips);
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			onExit?.Invoke(gameObject, GetParam(EDelegateTpye.Tpye_onExit));
			OnPlayMusic(EDelegateTpye.Tpye_onExit);

			bDown = false;
			nLangDelegate = -1;
		}
		public override void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				onUp?.Invoke(gameObject, GetParam(EDelegateTpye.Tpye_onUp));
				OnPlayMusic(EDelegateTpye.Tpye_onUp);
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				onRightUp?.Invoke(gameObject, GetParam(EDelegateTpye.Type_onRightUp));
				OnPlayMusic(EDelegateTpye.Type_onRightUp);
			}
			bDown = false;
			if (nLangDelegate > 0)
				nLangDelegate = 1;
		}
		public override void OnSelect(BaseEventData eventData)
		{
			onSelect?.Invoke(gameObject, GetParam(EDelegateTpye.Type_onSelect));
		}
		public override void OnUpdateSelected(BaseEventData eventData)
		{
			onUpdateSelect?.Invoke(gameObject, parames);
		}

		//物体从选中状态到取消选中状态时会被调用
		public override void OnDeselect(BaseEventData eventData)
		{
			onCancelSelect?.Invoke(gameObject, GetParam(EDelegateTpye.Type_onCancelSelect));
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			//长按执行，就屏蔽点击，跟双击
			if (nLangDelegate > 0)
				return;

			// 双击
			float time = Time.realtimeSinceStartup;
			if ((endtime + doubleClickTime) > time)
			{
				if (onDoubleClick != null)
				{
					onDoubleClick?.Invoke(gameObject, parames);
					return;
				}
			}
			endtime = time;

			//左键单机
			if (onClick != null && eventData.button == PointerEventData.InputButton.Left)
			{
				onClick?.Invoke(gameObject, GetParam(EDelegateTpye.Tpye_onClick));
				OnPlayMusic(EDelegateTpye.Tpye_onClick);
				return;
			}
			//右键单机
			if (onRightClick != null && eventData.button == PointerEventData.InputButton.Right)
			{
				onRightClick(gameObject, GetParam(EDelegateTpye.Tpye_onRightClick));
				OnPlayMusic(EDelegateTpye.Tpye_onRightClick);
				return;
			}
		}
        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            onClick?.Invoke(gameObject, GetParam(EDelegateTpye.Tpye_onClick));
            OnPlayMusic(EDelegateTpye.Tpye_onClick);
        }
        // 按下拖动  
        public override void OnDrag(PointerEventData eventData)
		{
			onDrag?.Invoke(gameObject, parames);
		}
		//进入拖动
		public override void OnMove(AxisEventData eventData)
		{
			onMove?.Invoke(gameObject, parames);
		}

		private void Update()
		{
			if (bDown && nLangDelegate >= 0)
			{
				if (Time.realtimeSinceStartup - fLangtime > durationThreshold)
				{
					nLangDelegate++;
					onLongPress?.Invoke(gameObject, parames);
				}
			}
		}
		//获得某个委托类型的参数
		public object[] GetParamesByDelegateTpye(EDelegateTpye tpye)
		{
			if (parameDic.ContainsKey(tpye))
				return parameDic[tpye];

			return null;
		}

		//修改某个委托类型的参数
		public void ChangeParames(EDelegateTpye tpye, object[] parames)
		{
			if (!parameDic.ContainsKey(tpye) || parames == null)
				return;

			parameDic[tpye] = parames;
		}
		#endregion //===========================end===函数区=================================
	}
}
