using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    public class AssetOperation : ScriptableObject
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="filterResult"></param>
        /// <param name="operationResult"></param>
        /// <returns></returns>
        public virtual AssetOperationResult Execute(AssetFilterResult filterResult,AssetOperationResult operationResult)
        {
            return operationResult;
        }
    }
}
