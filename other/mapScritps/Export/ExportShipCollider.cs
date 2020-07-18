#if UNITY_EDITOR
using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 导出船碰撞信息
    /// </summary>
    [ExecuteInEditMode]
    public class ExportShipCollider : MonoBehaviour
    {
        private IEnumerator m_UpdateEnumerator;

        [MenuItem("Custom/生成船的碰撞信息")]
        public static void Export()
        {
            ExportShipCollider[] shipColliders = UnityEngine.Object.FindObjectsOfType<ExportShipCollider>();
            if (shipColliders != null && shipColliders.Length > 0)
            {
                for (int iShip = shipColliders.Length - 1; iShip >= 0; iShip--)
                {
                    GameObject obj = shipColliders[iShip].gameObject;
                    if (obj != null)
                    {
                        GameObject.DestroyImmediate(obj);
                    }
                }
            }

            GameObject exportObj = new GameObject(typeof(ExportShipCollider).Name);
            ExportShipCollider collider = exportObj.AddComponent<ExportShipCollider>();
            collider.Init();

        }

        public void Init()
        {
            if (EditorGamingMapData.m_VoNameDic == null)
            {
                EditorGamingMapData.m_VoNameDic = new Dictionary<string, List<string>>();
            }
            EditorGamingMapData.m_VoNameDic.Clear();
            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(ModShipDecorateVO).Name))
            {
                List<string> shipInfo = new List<string>();
                shipInfo.Add("ship_decorate.csv");
                shipInfo.Add("ship_decorate");
                EditorGamingMapData.m_VoNameDic.Add(typeof(ModShipDecorateVO).Name, shipInfo);
            }

            if (!EditorGamingMapData.m_VoNameDic.ContainsKey(typeof(ModelVO).Name))
            {
                List<string> modelInfo = new List<string>();
                modelInfo.Add("model.csv");
                modelInfo.Add("model");
                EditorGamingMapData.m_VoNameDic.Add(typeof(ModelVO).Name, modelInfo);
            }

            EditorApplication.update += OnUpdate;
            ConfigVO<ModelVO>.Instance.Clear();
            ConfigVO<ModShipDecorateVO>.Instance.Clear();
            m_UpdateEnumerator = ExportUpdate();

        }

        private IEnumerator ExportUpdate()
        {
            ConfigVO<ModelVO>.Instance.GetList();
            yield return null;
            ConfigVO<ModShipDecorateVO>.Instance.GetList();
            ConfigVO<ModShipDecorateVO>.Instance.ResetData();
            yield return null;
            List<ModelVO> shipModelList = new List<ModelVO>();
            List<ModelVO> modelList = ConfigVO<ModelVO>.Instance.GetList();
            if (modelList != null && modelList.Count > 0)
            {
                for (int iModel = 0; iModel < modelList.Count; iModel++)
                {
                    ModelVO modelVo = modelList[iModel];
                    if (modelVo.type == (int)EditorGamingMapData.ModelType.WarShip)
                    {
                        shipModelList.Add(modelVo);
                    }
                }
            }
            yield return null;
            if (shipModelList != null && shipModelList.Count > 0)
            {
                for (int iShip = 0; iShip < shipModelList.Count; iShip++)
                {
                    ModelVO modelVo = shipModelList[iShip];
                    string assetName = modelVo.assetName;
                    if (!string.IsNullOrEmpty(assetName))
                    {
                        string[] resAssets = AssetDatabase.FindAssets(string.Format("{0} t:Prefab", assetName));
                        if (resAssets != null && resAssets.Length > 0)
                        {
                            for (int iRes = 0; iRes < resAssets.Length; iRes++)
                            {
                                assetName = AssetDatabase.GUIDToAssetPath(resAssets[iRes]);
                                string[] assetSplit = assetName.Split('/');
                                if (assetSplit != null && assetSplit.Length > 0)
                                {
                                    if (assetSplit[assetSplit.Length - 1].Equals(string.Format("{0}.prefab", modelVo.assetName)))
                                    {
                                        break;
                                    }
                                }

                            }
                            GameObject assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetName);
                            if (assetObj != null)
                            {
                                GameObject shipObj = GameObject.Instantiate(assetObj);
                                JudegeShipCollider(shipObj);
                                shipObj.transform.SetParent(transform);
                                shipObj.transform.localPosition = Vector3.zero;
                                CapsuleCollider shipCollider = EditorGamingMapData.CalculateCapsuleCollider(shipObj);
                                EditorGamingMapData.CorrectCollider(shipCollider);
                                yield return null;
                                yield return null;
                                Quaternion shipRot = shipCollider.transform.rotation;
                                shipCollider.transform.rotation = Quaternion.identity;
                                yield return null;
                                EditorDecorate decorate = EditorGamingMapData.SaveColliderData(shipCollider, shipRot);
                                yield return null;
                                shipCollider.transform.rotation = shipRot;
                                ModShipDecorateVO shipDecorateVo = ConfigVO<ModShipDecorateVO>.Instance.GetData(modelVo.ID);
                                if (shipDecorateVo != null)
                                {
                                    shipDecorateVo.colliderMax = decorate.dirMax;
                                    shipDecorateVo.colliderMin = decorate.dirMin;
                                    // shipDecorateVo.colliderMax = new float[3] { CalcuateRound(decorate.dirmax.x, 3), CalcuateRound(decorate.dirmax.y, 3), CalcuateRound(decorate.dirmax.z, 3) };
                                    // shipDecorateVo.colliderMin = new float[3] { CalcuateRound(decorate.dirmin.x, 3), CalcuateRound(decorate.dirmin.y, 3), CalcuateRound(decorate.dirmin.z, 3) };
                                }
                                else
                                {
                                    ModShipDecorateVO decorateVo = new ModShipDecorateVO();
                                    decorateVo.ID = modelVo.ID;
                                    decorateVo.unitId = modelVo.ID;
                                    decorateVo.type = (int)EditorGamingMapData.COLLIDER_TYPE.PX_CAPSULE;
                                    //decorateVo.colliderMax = new float[3] { CalcuateRound(decorate.dirmax.x, 3), CalcuateRound(decorate.dirmax.y, 3), CalcuateRound(decorate.dirmax.z, 3) };
                                    //decorateVo.colliderMin = new float[3] { CalcuateRound(decorate.dirmin.x, 3), CalcuateRound(decorate.dirmin.y, 3), CalcuateRound(decorate.dirmin.z, 3) };
                                    decorateVo.colliderMax = decorate.dirMax;
                                    decorateVo.colliderMin = decorate.dirMin;
                                    ConfigVO<ModShipDecorateVO>.Instance.AddData(decorateVo);
                                }
                                yield return null;
                                if (shipObj != null)
                                {
                                    GameObject.DestroyImmediate(shipObj);
                                }
                                yield return null;
                            }
                        }
                    }
                }
            }
            yield return null;
            ConfigVO<ModShipDecorateVO>.Instance.SaveCSV();
            bool isConfirm = EditorUtility.DisplayDialog("提示", "战舰碰撞信息导出成功", "确定");
            if (isConfirm)
            {
                GameObject.DestroyImmediate(gameObject);
            }
            
        }
        /// <summary>
        /// 矫正船碰撞信息
        /// </summary>
        /// <param name="shipObj"></param>
        private void JudegeShipCollider(GameObject shipObj)
        {
            CapsuleCollider capsule = shipObj.GetComponentInChildren<CapsuleCollider>();
            if (capsule == null)
            {
                return;
            }
            if (capsule.center != Vector3.zero || capsule.direction != 2)
            {
                Debug.LogError(shipObj.name + " 碰撞盒信息非法");
            }
        }

        private float CalcuateRound(float value, int num)
        {
            return (float)Math.Round(value, num);
        }

        private void OnEndExport()
        {
            GameObject.DestroyImmediate(gameObject);
            GamingMapEditorSetting gamingSetting = GamingMapEditorUtility.GetGamingMapEditorSetting();
            MapEditorUtility.OpenFolder(MapEditorUtility.GetFullPath(gamingSetting.m_ConfigPath));
        }

        private void Clear()
        {
            EditorApplication.update -= OnUpdate;
            m_UpdateEnumerator = null;
        }

        void OnUpdate()
        {
            if (m_UpdateEnumerator != null)
            {
                if (!m_UpdateEnumerator.MoveNext())
                {
                    m_UpdateEnumerator = null;
                    Debug.Log("战舰导出："+ Application.isBatchMode);
                    if(Application.isBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }
            }
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}

#endif
