#if UNITY_EDITOR
using EditorExtend;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 触发器
    /// </summary>
	[ExecuteInEditMode]
	public class Trigger : MapEntity<TriggerRoot>
	{
		#region 公开属性
		/// <summary>
		/// 点的唯一id
		/// </summary>
		public ulong m_Index;

		/// <summary>
		/// TriggerId
		/// </summary>
		public uint m_TriggerId;
        /// <summary>
        /// npc id
        /// </summary>
        public uint m_NpcId;

        /// <summary>
        /// 是否自动创建
        /// </summary>
        public bool m_AutoCreation;

        /// <summary>
        /// 半径
        /// </summary>
        public float m_Range;

        /// <summary>
		/// 当前选中的npc列表中的哪个
		/// </summary>
        [System.NonSerialized]
        public int m_SelectNpcIndex = -1;
        [System.NonSerialized]
        public string[] m_NpcIdArray;

        /// <summary>
		/// 当前选中的NpcTrigger列表中的哪个
		/// </summary>
        [System.NonSerialized]
        public int m_SelectTriggerIndex = -1;
        [System.NonSerialized]
        public string[] m_TriggerIdArray;
        #endregion

        #region 公开方法
        public IEnumerator OnUpdate(TriggerRoot root, bool showModel)
		{
			m_Root = root;
			m_ShowModel = showModel;
			RefreshModel();
			yield return null;
			RefreshPosition();
			yield return null;
            gameObject.name = m_Index.ToString();
            yield return null;
            transform.localScale = Vector3.one;
            int childCount = transform.childCount;
            if(childCount>0)
            {
                for(int iChild = 0;iChild<childCount;iChild++)
                {
                    Transform childTrans = transform.GetChild(iChild);
                    childTrans.localScale = Vector3.one * m_Range;
                }
            }
            
        }

		public void Init(ulong index, TriggerRoot root)
		{
			m_Root = root;
            m_Index = index;
            gameObject.name = m_Index.ToString();
            transform.localPosition = Vector3.zero;
        }

		public void Init(EditorTrigger trigger, TriggerRoot root)
		{
			m_Root = root;
            m_Index = trigger.triggerIndex;
            m_TriggerId = trigger.triggerId;
            m_NpcId = trigger.triggerNpcId;
            m_AutoCreation = trigger.autoCreation == 1 ? true : false;
            m_Range = trigger.reviveRange;
            transform.position = GetPositon(trigger.position);
            //         string strUid = m_Uid.ToString();
            //         if (strUid.Length > 4)
            //{
            //	string triggerIdStr = strUid.Substring(strUid.Length - 4, 4);
            //	if (!string.IsNullOrEmpty(triggerIdStr))
            //	{
            //		m_TriggerId = int.Parse(triggerIdStr);
            //	}
            //}

            gameObject.name = m_Index.ToString();
        }

        #endregion

        #region 私有方法
        private void OnDisable()
        {
            Clear();
        }

        private void Clear()
        {
            m_NpcIdArray = null;
            m_TriggerIdArray = null;
        }

        private void OnDestroy()
        {
            Clear();
        }

        #endregion

        #region 基类方法

        public override void DestroySelf()
        {
            Clear();
            base.DestroySelf();
        }

        protected override string GetModelPath()
		{
            if(!string.IsNullOrEmpty(m_ModelPath))
            {
                return m_ModelPath;
            }

            //NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
            //if (vo != null)
            //{
            //    int modelId = vo.Model;
            //    string assetName = "";
            //    ModelVO modelVo = ConfigVO<ModelVO>.Instance.GetData(modelId);
            //    if (modelVo != null)
            //    {
            //        assetName = modelVo.assetName;
            //    }
            //    if (!string.IsNullOrEmpty(assetName))
            //    {
            //        if (!assetName.Contains("Assets"))
            //        {
            //            string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", assetName));
            //            if (resAssets != null && resAssets.Length > 0)
            //            {
            //                for (int iRes = 0; iRes < resAssets.Length; iRes++)
            //                {
            //                    string path = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
            //                    string[] resSplit = path.Split('/');
            //                    if (resSplit != null && resSplit.Length > 0)
            //                    {
            //                        string lastName = resSplit[resSplit.Length - 1];
            //                        if (lastName.Equals(string.Format("{0}.prefab", assetName)))
            //                        {
            //                            assetName = path;
            //                            break;
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //    }
            //    m_ModelPath = assetName;
            //    return assetName;
            //}
            m_ModelPath = GamingMapEditorUtility.GetTriggerTempletePath();

            return m_ModelPath;
		}

		public override EditorPosition GetEditorPosition()
		{
			return new EditorPosition(transform.position);
		}

		public override EditorRotation GetEditorRotation()
		{
			return new EditorRotation(transform.rotation);
		}

        public int GetAutoCreation()
        {
            return m_AutoCreation ? 1 : 0;
        }
        #endregion
    }
}
#endif