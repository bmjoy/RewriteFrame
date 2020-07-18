using UnityEngine;
using UnityEditor;
using Eternity.Share.Config.TimeLine.Datas.Attributes;
using System;
using Eternity.Share.Config.ActionDatas;
using System.Reflection;
using System.Collections.Generic;

namespace EternityEditor.Battle.Timeline
{
    public class TimelineDrawerConfig
    {
        public ActionCategory Category { get; set; } = ActionCategory.Skill;
        public float TimeLength { get; set; } = 5.0f;

        public float TimeStep { get; set; } = 0.05f;
        public int TrackLineHeight { get; set; } = 60;
        public int WidthForSecond { get; set; } = 200;

        public Vector2 GroupScrollPos { get; set; } = Vector2.zero;

        public float TimeStepWidth { get => TimeStep * WidthForSecond; }
        public float ScrollPosX { get => GroupScrollPos.x; }
        public float ScrollPosY { get => GroupScrollPos.y; }

        public void ShowMenu(Action<ActionData> callback)
        {
            Assembly assembly = typeof(ActionData).Assembly;

            List<Type> actionDataTypes = new List<Type>();
            foreach(var type in assembly.GetTypes())
            {
                if(!type.IsAbstract && type.IsSubclassOf(typeof(ActionData)))
                {
                    ActionUsage actionUsage = type.GetCustomAttribute<ActionUsage>();
                    if(actionUsage!=null)
                    {
                        if(actionUsage.Category == ActionCategory.All || actionUsage.Category == Category)
                        {
                            actionDataTypes.Add(type);
                        }
                    }
                }
            }
            GenericMenu menu = new GenericMenu();
            foreach(var type in actionDataTypes)
            {
                ActionUsage actionUsage = type.GetCustomAttribute<ActionUsage>();
                menu.AddItem(new GUIContent($"{actionUsage.MenuPrefix}/{actionUsage.Name}"), false, () =>
                {
                    ActionData data= (ActionData)Activator.CreateInstance(type);
                    callback(data);
                });
            }
            menu.ShowAsContext();
        }
    }
}
