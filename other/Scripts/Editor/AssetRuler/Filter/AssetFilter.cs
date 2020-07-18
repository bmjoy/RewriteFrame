using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeyoutechEditor.Core.AssetRuler
{
    //资源过滤器
    public class AssetFilter : ScriptableObject
    {
        /// <summary>
        /// 是否匹配过去条件
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public virtual bool IsMatch(string assetPath)
        {
            return false;
        }
    }
}
