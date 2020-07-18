using LeyoutechEditor.Core.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler
{
    public class AssetGroup : ScriptableObject
    {
        [FieldLabel("是否可用")]
        [FormerlySerializedAs("isEnable")]
        public bool m_IsEnable = true;

        [FieldLabel("组名")]
        [FormerlySerializedAs("groupName")]
        public string m_GroupName = "Asset Group";

        [EnumLabel("资源集合类型")]
        [FormerlySerializedAs("assetAssemblyType")]
        public AssetAssemblyType m_AssetAssemblyType = AssetAssemblyType.AssetAddress;

        /// <summary>
        /// 搜索路径条件列表
        /// </summary>
        [FormerlySerializedAs("assetSearchers")]
        public List<AssetSearcher> m_AssetSearchers = new List<AssetSearcher>();

        /// <summary>
        /// 过滤条件列表--对搜索路径二次过滤
        /// </summary>
        [FormerlySerializedAs("filterOperations")]
        public List<AssetGroupFilterOperation> m_FilterOperations = new List<AssetGroupFilterOperation>();


        /// <summary>
        /// 创建组返回结果类型
        /// </summary>
        /// <returns></returns>
        protected virtual AssetGroupResult CreateGroupResult()
        {
            return new AssetGroupResult();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="assemblyResult">资源集合返回结果</param>
        /// <returns>组搜索结果</returns>
        public virtual AssetGroupResult Execute(AssetAssemblyResult assemblyResult)
        {
            if(assemblyResult == null)
            {
                Debug.LogError("AssetGroup::Execute->Assembly Result is Null");
                return null;
            }
            if(!m_IsEnable ||  m_FilterOperations.Count == 0)
            {
                return null;
            }

            AssetSearcherResult searcherResult = new AssetSearcherResult();
            foreach(var searcher in m_AssetSearchers)
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder(searcher.m_Folder))
                {
                    Debug.LogError($"AssetGroup::Execute->Folder is unvalid.groupName = {m_GroupName},folder={searcher.m_Folder}");
                    continue;
                }

                //逐个文件夹路径搜索条件，执行搜索
                searcherResult.m_AssetPaths.AddRange(searcher.Execute().m_AssetPaths);
            }

            AssetGroupResult groupResult = CreateGroupResult();
            groupResult.m_GroupName = m_GroupName;
            assemblyResult.m_GroupResults.Add(groupResult);

            foreach (var filterOperation in m_FilterOperations)
            {
                //逐个二次过滤条件，检查搜索
                AssetOperationResult[] operationResults = filterOperation.Execute(searcherResult,groupResult);
                if(operationResults != null)
                {
                    groupResult.m_OperationResults.AddRange(operationResults);
                }
            }

            return groupResult;
        }


        [Serializable]
        public class AssetSearcher
        {
            [FieldLabel("文件夹路径")]
            [FormerlySerializedAs("folder")]
            public string m_Folder = "Assets";

            [FieldLabel("是否包含子文件夹")]
            [FormerlySerializedAs("includeSubfolder")]
            public bool m_IncludeSubfolder = true;

            [FieldLabel("文件名过滤器正则表达式")]
            [FormerlySerializedAs("fileNameFilterRegex")]
            public string m_FileNameFilterRegex = "";

            /// <summary>
            /// 执行，根据以上条件，筛选出符合条件资源路径
            /// </summary>
            /// <returns></returns>
            public AssetSearcherResult Execute()
            {
                string[] assets = DirectoryUtil.GetAssetsByFileNameFilter(m_Folder, m_IncludeSubfolder, m_FileNameFilterRegex, new string[] { ".meta" });
                AssetSearcherResult result = new AssetSearcherResult();
                result.m_AssetPaths.AddRange(assets);
                return result;
            }
        }
    }
}
