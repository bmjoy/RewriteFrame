using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumUtil
{
    public static int ToInt(this System.Enum e)
    {
        return e.GetHashCode();
    }

}