/*===============================
 * Purpose: 公共函数
 * Time: 2019/4/9 15:53:11
================================*/

using UnityEngine;

public static class TransformUtil
{
    public static T FindUIObject<T>(Transform root, string path)
    {
        return root.Find(path).GetComponent<T>();
    }
}