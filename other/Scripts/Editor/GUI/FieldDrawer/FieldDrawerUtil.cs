using Eternity.Share.Config.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace LeyoutechEditor.EGUI.FieldDrawer
{
    public static class FieldDrawerUtil
    {
        private static readonly Dictionary<Type, Type> sm_DrawerTypeDic = new Dictionary<Type, Type>();
        private static bool sm_IsInit = false;
        private static void InitFieldDrawer()
        {
            if (sm_IsInit) return;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(AFieldDrawer)))
                    {
                        TargetFieldType targetFieldType = type.GetCustomAttribute<TargetFieldType>();
                        if (targetFieldType != null)
                        {
                            sm_DrawerTypeDic.Add(targetFieldType.TargetType, type);
                        }
                    }
                }
            }
            sm_IsInit = true;
        }

        private static Type GetDrawerType(Type fieldType)
        {
            InitFieldDrawer();

            if (!sm_DrawerTypeDic.TryGetValue(fieldType, out Type drawerType))
            {
                if (fieldType.IsValueType && fieldType.IsEnum)
                {
                    drawerType = sm_DrawerTypeDic[typeof(Enum)];
                }else if(fieldType.IsClass && !fieldType.IsArray && !typeof(IList).IsAssignableFrom(fieldType))
                {
                    drawerType = sm_DrawerTypeDic[typeof(System.Object)];
                }
            }

            return drawerType;
        }

        private static FieldInfo[] GetFieldInfos(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            List<FieldInfo> list = new List<FieldInfo>();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.IsPublic)
                {
                    FieldHide fieldHide = fieldInfo.GetCustomAttribute<FieldHide>();
                    if (fieldHide == null)
                    {
                        list.Add(fieldInfo);
                    }
                }
                else
                {
                    FieldShow fieldShow = fieldInfo.GetCustomAttribute<FieldShow>();
                    if (fieldShow != null)
                    {
                        list.Add(fieldInfo);
                    }
                }
            }

            list.Sort((item1, item2) =>
            {
                var item1Attr = item1.GetCustomAttribute<FieldOrder>();
                var item2Attr = item2.GetCustomAttribute<FieldOrder>();
                int order1 = item1Attr != null ? item1Attr.Order : 9999;
                int order2 = item2Attr != null ? item2Attr.Order : 9999;
                return order1.CompareTo(order2);
            });
            return list.ToArray();
        }

        public static FieldData[] GetTypeFieldDrawer(Type type)
        {
            List<FieldData> fieldDatas = new List<FieldData>();
            foreach (var fieldInfo in GetFieldInfos(type))
            {
                FieldData fieldData = new FieldData()
                {
                    Name = fieldInfo.Name
                };

                Type drawerType = GetDrawerType(fieldInfo.FieldType);
                if (drawerType!=null)
                {
                    fieldData.Drawer = (AFieldDrawer)Activator.CreateInstance(drawerType, fieldInfo);
                }

                fieldDatas.Add(fieldData);
            }
            return fieldDatas.ToArray();
        }
    }
}
