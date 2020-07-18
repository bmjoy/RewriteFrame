#if UNITY_EDITOR
using EditorExtend;
using Leyoutech.Utility;
using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Map
{
	/// <summary>
	/// 地编中的npc实体
	/// </summary>
	[ExecuteInEditMode]
	public class Creature : MapEntity<CreatureRoot>
	{
		#region 私有属性

		#endregion

		#region 公开属性
		/// <summary>
		/// Creature的唯一id
		/// </summary>
		public ulong m_Uid;
		/// <summary>
		/// 对应配置表中的id
		/// </summary>
		public int m_NpcId;
		/// <summary>
		/// 是否自动创建
		/// </summary>
		public bool m_AutoCreation;
		/// <summary>
		/// 自增的id
		/// </summary>
		public int m_CreatureId;

		/// <summary>
		/// 当前选中的npc列表中的哪个
		/// </summary>
        [System.NonSerialized]
		public int m_SelectNpcIndex = -1;
        [System.NonSerialized]
		public string[] m_NpcIdArray;

        /// <summary>
        /// 复活半径
        /// </summary>
        public float m_ReviveRange;
        public bool m_DebugShowRange;
        #endregion

        #region 挂载

        #endregion

        #region 私有方法

        private void OnDisable()
        {
            m_NpcIdArray = null;
        }

        private void OnDestroy()
        {
            m_NpcIdArray = null;
        }
        #endregion

        #region 公开方法

        /// <summary>
        /// 当前npc是否为复活npc
        /// </summary>
        /// <returns></returns>
        public bool IsRelieveCreature()
        {
            if(m_NpcId<=0)
            {
                return false;
            }

            NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
            if(vo == null)
            {
                return false;
            }

            return vo.NpcType == (int)KHeroType.reliveObj;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(ulong uid, int creatureId, CreatureRoot root)
		{
			m_Root = root;
			m_Uid = uid;
			m_CreatureId = creatureId;

            NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
            if (vo != null)
            {
                gameObject.name = string.Format("{0}_{1}", vo.Name, m_Uid);
            }
            transform.localPosition = Vector3.zero;
        }

		public void Init(EditorCreature creature)
		{
			m_Uid = creature.creatureId;
			m_NpcId = creature.tplId;
			m_AutoCreation = creature.autoCreation == 1 ? true : false;
            m_ReviveRange = creature.reviveRange;
            string strUid = m_Uid.ToString();
			if (strUid.Length > 3)
			{
				string creatureIdStr = strUid.Substring(strUid.Length - 3, 3);
				if (!string.IsNullOrEmpty(creatureIdStr))
				{
					m_CreatureId = int.Parse(creatureIdStr);
				}
			}
            NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
            if(vo != null)
            {
                gameObject.name = string.Format("{0}_{1}", vo.Name, m_Uid);
            }
            
			transform.position = GetPositon(creature.position);
			transform.rotation = GetRotation(creature.rotation);
		}

		public int GetAutoCreation()
		{
			return m_AutoCreation ? 1 : 0;
		}

        public int GetBindTeleportId()
        {
            BindTeleport bindTeleport = GetComponent<BindTeleport>();
            if(bindTeleport != null)
            {
                return bindTeleport.m_TeleportId;
            }
            return 0;
        }

		public override EditorPosition GetEditorPosition()
		{
            return new EditorPosition(transform.position);
		}

		public override EditorRotation GetEditorRotation()
		{
            return new EditorRotation(transform.rotation);
		}

		public IEnumerator OnUpdate(CreatureRoot root, bool showModel)
		{
			m_Root = root;
			m_ShowModel = showModel;
			RefreshModel();
			yield return null;
			RefreshPosition();
			yield return null;
            NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
            if (vo != null)
            {
                gameObject.name = string.Format("{0}_{1}", vo.Name, m_Uid);
            }
            yield return null;
        }

        protected void OnDrawGizmosSelected()
        {
            if(!m_DebugShowRange)
            {
                return;
            }
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, m_ReviveRange/2);
        }
        #endregion

        #region 父类方法
        public override void DestroySelf()
		{
			m_NpcIdArray = null;
			base.DestroySelf();
		}

		protected override string GetModelPath()
		{
            if(!string.IsNullOrEmpty(m_ModelPath))
            {
                return m_ModelPath;
            }
			NpcVO vo = ConfigVO<NpcVO>.Instance.GetData(m_NpcId);
			if (vo != null)
			{
				int modelId = vo.Model;
                string assetName = "";
				ModelVO modelVo = ConfigVO<ModelVO>.Instance.GetData(modelId);
				if (modelVo != null)
				{
                    assetName = modelVo.assetName;
				}
                if (!string.IsNullOrEmpty(assetName))
                {
                    if (!assetName.Contains("Assets"))
                    {
                        string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", assetName));
                        if (resAssets != null && resAssets.Length > 0)
                        {
                            for (int iRes = 0; iRes < resAssets.Length; iRes++)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                                string[] resSplit = path.Split('/');
                                if (resSplit != null && resSplit.Length > 0)
                                {
                                    string lastName = resSplit[resSplit.Length - 1];
                                    if (lastName.Equals(string.Format("{0}.prefab", assetName)))
                                    {
                                        assetName = path;
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }
                m_ModelPath = assetName;
                return assetName;
            }
			return GamingMapEditorUtility.GetCreatureTempletePath();
		}
		#endregion

	}
}
#endif
