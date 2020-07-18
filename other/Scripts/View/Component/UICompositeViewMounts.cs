using System.Collections.Generic;
using UnityEngine;

public class UICompositeViewMounts : MonoBehaviour
{
    /// <summary>
    /// 背景容器
    /// </summary>
    public Transform BackBox;
    /// <summary>
    /// 热键容器
    /// </summary>
    public Transform HotkeyBox;
    /// <summary>
    /// 标题容器
    /// </summary>
    public Transform TitleBox;
    /// <summary>
    /// 列表容器
    /// </summary>
    public Transform ListBox;
    /// <summary>
    /// Tip容器
    /// </summary>
    public Transform TipBox;
    /// <summary>
    /// 分页容器
    /// </summary>
    public Transform PageBox;
    /// <summary>
    /// 排序容器
    /// </summary>
    public Transform SortBox;
    /// <summary>
    /// 模型容器
    /// </summary>
    public Transform ModelBox;
    /// <summary>
    /// 其它容器
    /// </summary>
    public Transform OtherBox;

    /// <summary>
    /// 挂点列表
    /// </summary>
    private List<Transform> m_MountPoints;

    /// <summary>
    /// 排序列表A
    /// </summary>
    private List<int> s_SortListA = new List<int>();

    /// <summary>
    /// 排序列表B
    /// </summary>
    private List<int> s_SortListB = new List<int>();

    /// <summary>
    /// 获取所有已排序的挂点
    /// </summary>
    /// <returns></returns>
    public List<Transform> GetOrderMountPoints()
    {
        if(m_MountPoints==null)
        {
            m_MountPoints = new List<Transform>();
            m_MountPoints.AddRange(new Transform[] { BackBox, HotkeyBox, TitleBox, ListBox, TipBox, PageBox, SortBox, ModelBox, OtherBox });

            m_MountPoints.Sort((a, b) =>
            {
                s_SortListA.Clear();
                while (a != transform)
                {
                    s_SortListA.Add(a.GetSiblingIndex());
                    a = a.parent;
                }

                s_SortListB.Clear();
                while(b!=transform)
                {
                    s_SortListB.Add(b.GetSiblingIndex());
                    b = b.parent;
                }

                int count = Mathf.Min(s_SortListA.Count, s_SortListB.Count);
                for (int i = 0; i < count; i++)
                {
                    int A = s_SortListA[s_SortListA.Count - 1 - i];
                    int B = s_SortListB[s_SortListB.Count - 1 - i];

                    if (A < B)
                        return -1;
                    else if (A > B)
                        return 1;
                }

                return 0;
            });
        }

        return m_MountPoints;
    }
}
