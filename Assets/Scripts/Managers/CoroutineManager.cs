using System.Collections.Generic;
using UnityEngine;

public static class CoroutineManager
{
    private static Dictionary<int, WaitForSeconds> _waitDict = new(16);

    public static WaitForSeconds GetWaitForSec(int sec)
    {
        if (!_waitDict.TryGetValue(sec, out WaitForSeconds value))
        {
            _waitDict.Add(sec, new WaitForSeconds(sec));
            return _waitDict[sec];
        }
        else
        {
            return value;
        }
    }
}
