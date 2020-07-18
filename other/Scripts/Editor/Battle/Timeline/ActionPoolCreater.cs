using Eternity.Share.Config.ActionDatas;
using Eternity.Share.Config.DurationActionDatas;
using Eternity.Share.Config.EventActionDatas;
using Eternity.Share.Config.GroupActionDatas;
using Eternity.Share.Config.TimeLine.Datas;
using Leyoutech.Core.Timeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace EternityEditor.Battle.Timeline
{
    public static class ActionPoolCreater
    {
        private static readonly string CLASS_FILE_FORMAT =
    @"
/*
This file was create by tool.
Please don't change it by your self.
*/

using Leyoutech.Core.Pool;
using Leyoutech.Core.Timeline;
using Eternity.FlatBuffer.Enums;
#USING_NAMESPACES

namespace Gameplay.Battle.Timeline
{
    public static partial class ActionPool
    {
        static ActionPool()
        {
            Creater = GetActionPool;
        }

        public static IActionPool GetActionPool(ActionID actionID)
        {
            switch(actionID)
            {
#ACTION_ID_TO_ACTION_POOLS
            }

            return null;
        }
    }

#ACTION_POOLS

}
";

        private static readonly string ID_TO_ACTION_POOL_FORMAT =
    @"
                case ActionID.#ACTION_ID_NAME:
                    return new #ACTION_POOL_NAME();
";
        private static readonly string ACTION_POOL_FORMAT =
    @"
    public class #ACTION_POOL_NAME : IActionPool
    {
        private ObjectPool<#ACTION_NAME> m_Pool = null;

        public #ACTION_POOL_NAME()
        {
            m_Pool = new ObjectPool<#ACTION_NAME>(5);
        }

        public AActionItem GetAction()
        {
            return m_Pool.Get();
        }

        public void ReleaseAction(AActionItem actionItem)
        {
            m_Pool.Release((#ACTION_NAME)actionItem);
        }

        public void Clear()
        {
            m_Pool.Clear();
            m_Pool = null;
        }
    }
";
        private static readonly string USING_NAMESPACE_FORMAT = "using #NAMESPACE_NAME;";

        private static readonly string USING_NAMESPACES_KEY = "#USING_NAMESPACES";
        private static readonly string ACTION_ID_TO_ACTION_POOLS_KEY = "#ACTION_ID_TO_ACTION_POOLS";
        private static readonly string ACTION_POOLS_KEY = "#ACTION_POOLS";

        private static readonly string NAMESPACE_NAME_KEY = "#NAMESPACE_NAME";
        private static readonly string ACTION_NAME_KEY = "#ACTION_NAME";
        private static readonly string ACTION_POOL_NAME_KEY = "#ACTION_POOL_NAME";
        private static readonly string ACTION_ID_NAME_KEY = "#ACTION_ID_NAME";



        [MenuItem("Custom/Battle/Create Action Pool")]
        public static void CreaterActionPool()
        {
            Assembly dataAssembly = typeof(ActionData).Assembly;
            var actionDataTypes = from type in dataAssembly.GetTypes()
                                  where type.IsSubclassOf(typeof(EventActionData)) || type.IsSubclassOf(typeof(GroupActionData)) || type.IsSubclassOf(typeof(DurationActionData))
                                  select type;

            Dictionary<ActionID, Type> actionDataTypeDic = new Dictionary<ActionID, Type>();
            foreach (var type in actionDataTypes)
            {
                ActionData actionData = (ActionData)dataAssembly.CreateInstance(type.FullName);
                if (actionData.Platform == ActionPlatform.All || actionData.Platform == ActionPlatform.Client)
                {
                    actionDataTypeDic.Add(actionData.Id, type);
                }
            }

            Assembly actionAssembly = typeof(AActionItem).Assembly;
            var actionTypes = (from type in actionAssembly.GetTypes() where type.IsSubclassOf(typeof(AActionItem)) && !type.IsAbstract select type).ToArray();

            StringBuilder actionPoolSB = new StringBuilder();
            List<string> namespaceList = new List<string>();
            StringBuilder idToActionSB = new StringBuilder();
            foreach (var kvp in actionDataTypeDic)
            {
                string actionIDName = kvp.Key.ToString();
                string actionTypeName = kvp.Value.Name.Replace("Data", "Action");
                string actionPoolName = actionTypeName + "Pool";

                var at = (from type in actionTypes where type.Name == actionTypeName select type).ToArray();
                if (at != null && at.Length > 0)
                {
                    Type actionType = at[0];

                    namespaceList.Add(USING_NAMESPACE_FORMAT.Replace(NAMESPACE_NAME_KEY, actionType.Namespace));

                    string poolContent = ACTION_POOL_FORMAT.Replace(ACTION_NAME_KEY, actionTypeName).Replace(ACTION_POOL_NAME_KEY, actionPoolName);
                    actionPoolSB.Append(poolContent);

                    string idToActionContent = ID_TO_ACTION_POOL_FORMAT.Replace(ACTION_ID_NAME_KEY, actionIDName)
                        .Replace(ACTION_POOL_NAME_KEY, actionPoolName);
                    idToActionSB.Append(idToActionContent);
                }
            }
            namespaceList = namespaceList.Distinct().ToList();
            string fileContent = CLASS_FILE_FORMAT.Replace(USING_NAMESPACES_KEY, string.Join("\r\n", namespaceList.ToArray()));
            fileContent = fileContent.Replace(ACTION_ID_TO_ACTION_POOLS_KEY, idToActionSB.ToString());
            fileContent = fileContent.Replace(ACTION_POOLS_KEY, actionPoolSB.ToString());

            string classPath = Application.dataPath + "/Scripts/Gameplay/Battle/Timeline/ActionPool.cs";
            File.WriteAllText(classPath, fileContent);
        }
    }
}

