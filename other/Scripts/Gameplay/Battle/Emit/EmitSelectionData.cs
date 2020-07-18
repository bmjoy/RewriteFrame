using Eternity.FlatBuffer.Enums;
using System.Collections.Generic;

namespace Gameplay.Battle.Emit
{
    /// <summary>
    /// 发射口数据
    /// </summary>
    public class EmitData
    {
        /// <summary>
        /// 绑点类型
        /// </summary>
        public BindNodeType NodeType { get; set; }

        /// <summary>
        /// 绑点点索引
        /// </summary>
        public int NodeIndex { get; set; }
    }

    /// <summary>
    /// 发射口选择
    /// </summary>
    public class EmitSelectionData
    {
        private Dictionary<int, List<EmitData>> assignEmitDic = new Dictionary<int, List<EmitData>>();

        public void AddOrUpdateEmit(int assignIndex,EmitData emitData)
        {
            AddOrUpdateEmit(assignIndex, new EmitData[] { emitData });
        }

        public void AddOrUpdateEmit(int assignIndex,EmitData[] emitDatas)
        {
            if (!assignEmitDic.TryGetValue(assignIndex, out List<EmitData> data))
            {
                data = new List<EmitData>();
                assignEmitDic.Add(assignIndex, data);
            }else
            {
                data.Clear();
            }
            data.AddRange(emitDatas);
        }

        public void RemoveEmit(int assignIndex)
        {
            if(assignEmitDic.ContainsKey(assignIndex))
            {
                assignEmitDic.Remove(assignIndex);
            }
        }

        public bool ContainsEmit(int assignIndex)
        {
            return assignEmitDic.ContainsKey(assignIndex);
        }

        public EmitData[] GetEmits(int assignIndex)
        {
            if (assignEmitDic.TryGetValue(assignIndex, out List<EmitData> data))
            {
                return data.ToArray();
            }
            return null;
        }

        public EmitData GetEmit(int assignIndex)
        {
            if (assignEmitDic.TryGetValue(assignIndex, out List<EmitData> data) && data.Count>0)
            {
                return data[0];
            }
            return null;
        }

        public void Clear()
        {
            assignEmitDic.Clear();
        }
    }
}
