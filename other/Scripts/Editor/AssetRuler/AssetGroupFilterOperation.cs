using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeyoutechEditor.Core.AssetRuler
{
    [CreateAssetMenu(fileName = "group_filter_operation", menuName = "Asset Ruler/Asset Address/Group Filter&Operation二级筛选配置", order = 1)]
    public class AssetGroupFilterOperation : ScriptableObject
    {
        [Header("true删除保证下一个二次筛选器不重复过滤")]
        [FieldLabel("是否移除筛选器条目数据")]
        [FormerlySerializedAs("removeMatchFilterItem")]
        public bool m_RemoveMatchFilterItem = true;

        [EnumLabel("二级晒选器允许条件类型")]
        [FormerlySerializedAs("filterComposeType")]
        public AssetComposeType m_FilterComposeType = AssetComposeType.All;

        /// <summary>
        /// 过滤器列表
        /// </summary>
        [FormerlySerializedAs("assetFilters")]
        public List<AssetFilter> m_AssetFilters = new List<AssetFilter>();

        [EnumLabel("规则条件类型")]
        [FormerlySerializedAs("operationComposeType")]
        public AssetComposeType m_OperationComposeType = AssetComposeType.All;

        /// <summary>
        /// 筛选完毕后的规则操作列表
        /// </summary>
        [FormerlySerializedAs("assetOperations")]
        public List<AssetOperation> m_AssetOperations = new List<AssetOperation>();


        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="searcherResult"></param>
        /// <returns></returns>
        private AssetFilterResult ExecuteFilter(AssetSearcherResult searcherResult)
        {
            AssetFilterResult filterResult = new AssetFilterResult();
            for(int i =searcherResult.m_AssetPaths.Count-1;i>=0;--i)
            {
                string assetPath = searcherResult.m_AssetPaths[i];
                if (IsMatchFilter(assetPath))
                {
                    if(m_RemoveMatchFilterItem)
                    {
                        searcherResult.m_AssetPaths.RemoveAt(i);//为下个二级筛选器剔除重复
                    }
                    filterResult.m_AssetPaths.Add(assetPath);
                }
            }
            return filterResult;
        }

        //满足条件吗
        private bool IsMatchFilter(string assetPath)
        {
            if(m_AssetFilters == null || m_AssetFilters.Count == 0)
            {
                return true;
            }

            foreach (var filter in m_AssetFilters)
            {
                if (filter == null)
                {
                    continue;
                }

                if (filter.IsMatch(assetPath))
                {
                    if (m_FilterComposeType == AssetComposeType.Any)
                    {
                        return true;
                    }
                }
                else
                {
                    if (m_FilterComposeType == AssetComposeType.All)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public AssetOperationResult[] Execute(AssetSearcherResult searcherResult,AssetGroupResult groupResult)
        {
            AssetFilterResult filterResult = ExecuteFilter(searcherResult);
            List<AssetOperationResult> operationResults = new List<AssetOperationResult>();

            List<AssetOperationResult> results = new List<AssetOperationResult>();
            if (m_OperationComposeType == AssetComposeType.All)
            {
                //男的&& 大于25 && 上过学的
                AssetOperationResult operationResult = null;
                foreach (var assetOperation in m_AssetOperations)
                {
                    if (operationResult == null)
                    {
                        operationResult = assetOperation.Execute(filterResult, null);
                        if (operationResult != null)
                        {
                            results.Add(operationResult);
                        }
                    }
                    else
                    {
                        assetOperation.Execute(filterResult, operationResult);
                    }
                }
            }
            else
            {
                foreach (var assetOperation in m_AssetOperations)
                {
                    //男的|| 大于25 ||  上过学的
                    AssetOperationResult operationResult = assetOperation.Execute(filterResult, null);
                    if (operationResult != null)
                    {
                        results.Add(operationResult);
                    }
                }
            }

            return results.ToArray();
        }
    }
}
