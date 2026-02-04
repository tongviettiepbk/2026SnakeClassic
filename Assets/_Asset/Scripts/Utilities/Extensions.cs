using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public enum ColorText
{
    NONE,

    GREEN,
    WHITE
}

public static class Extensions
{

    #region LIST
    public static void RemoveDuplicates(this List<Vector3> list, float epsilon = 0.004f)
    {
        List<Vector3> unique = new List<Vector3>();

        foreach (var p in list)
        {
            if (!unique.Any(u => Vector3.Distance(u, p) < epsilon))
                unique.Add(p);
        }

        list.Clear();
        list.AddRange(unique);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void ForEach<T>(this T[] array, Action<T> callback)
    {
        for (int i = 0; i < array.Length; i++)
        {
            callback.Invoke(array[i]);
        }
    }

    public static T GetUIItem<T>(this IList<T> uiItems, GameObject goPrefab, GameObject goContent) where T : MonoBehaviour
    {
        var ui = uiItems.FirstOrDefault(x => !x.gameObject.activeSelf);
        if (ui == null)
        {
            ui = goPrefab.CreateGameObject<T>(goContent);
            uiItems.Add(ui);
        }
        return ui;
    }

    public static void HideUIItems<T>(this IList<T> uiItems) where T : MonoBehaviour
    {
        foreach (var item in uiItems)
        {
            item.gameObject.SetActive(false);
        }
    }

    #endregion

    #region GAME OBJECT
    public static GameObject CreateGameObject(this GameObject gPrefab, GameObject gContent, bool autoActive = true)
    {
        GameObject gObj = GameObject.Instantiate(gPrefab);

        gObj.transform.SetParent(gContent.transform);
        gObj.transform.localPosition = Vector3.zero;
        gObj.transform.localScale = Vector3.one;

        if (autoActive) gObj.SetActive(true);

        return gObj;
    }

    public static T CreateGameObject<T>(this GameObject gPrefab, GameObject gContent, bool autoActive = true)
    {
        GameObject gObj = CreateGameObject(gPrefab, gContent, autoActive);
        return gObj.GetComponent<T>();
    }
    #endregion

    #region COROUTINE

    public static void StartDelayAction(this MonoBehaviour mono, float time, Action callback)
    {
        mono.StartCoroutine(Delay(callback, time));
    }

    public static void StartActionEndOfFrame(this MonoBehaviour mono, Action callback)
    {
        mono.StartCoroutine(DelayEndOfFrame(callback));
    }

    private static IEnumerator Delay(Action callBack, float time)
    {
        yield return Yielder.Get(time);
        callBack.Invoke();
    }

    private static IEnumerator DelayEndOfFrame(Action callBack)
    {
        yield return new WaitForEndOfFrame();
        callBack.Invoke();
    }

    public static void StartActionWaitUntil(this MonoBehaviour mono, Func<bool> predicate, Action callBack)
    {
        mono.StartCoroutine(DelayWaitUntil(predicate, callBack));
    }

    private static IEnumerator DelayWaitUntil(Func<bool> predicate, Action callBack)
    {
        yield return new WaitUntil(predicate);
        callBack.Invoke();
    }

    public static IEnumerator StopIEnumerator(this MonoBehaviour mono, IEnumerator ie)
    {
        if (ie != null)
        {
            mono.StopCoroutine(ie);
        }
        return null;
    }

    public static IEnumerator StartIEnumerator(this MonoBehaviour mono, IEnumerator ie)
    {
        var nie = ie;
        mono.StartCoroutine(nie);
        return nie;
    }
    #endregion

    #region TRANSFORM
    public static Vector3 GetWorldPosition(this RectTransform rect)
    {
        return rect.TransformPoint(rect.transform.position);
    }

    public static void FaceYAxisTo(this Transform trans, Vector3 endPoint)
    {
        Vector3 dir = (endPoint - trans.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        trans.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    public static void FaceXAxisTo(this Transform transform, Vector3 endPoint)
    {
        Vector3 dir = endPoint - transform.position;
        Vector3 v = Vector3.zero;
        v.z = Vector3.SignedAngle(Vector3.right, dir, Vector3.forward);
        transform.eulerAngles = v;
    }
    #endregion

    #region STRING
    public static string ToStringRate(this float num, ColorText color = ColorText.GREEN)
    {
        float percent = num * 100f;
        string s = string.Empty;

        //if (color == ColorText.NONE)
        //{
        //    s = string.Format("{0}%", percent);
        //}
        //else
        //{
        //    string hexCode = string.Empty;

        //    switch (color)
        //    {
        //        case ColorText.WHITE:
        //            {
        //                hexCode = GameUtils.GetColorHexCode(GameConfig.Instance.colorWhite);
        //            }
        //            break;

        //        default:
        //            {
        //                hexCode = GameUtils.GetColorHexCode(GameConfig.Instance.colorGreen);
        //            }
        //            break;
        //    }

        //    s = string.Format("<color=#{0}>{1}%</color>", hexCode, percent);
        //}

        return s;
    }

    public static string ToShortString(this long num)
    {
        if (num < 1000000)
        {
            return num.ToString("n0");
        }
        else if (num < 1000000000)
        {
            int integer = (int)num / 1000000;
            int decim = (int)num % 1000000;

            if (decim >= 10000)
            {
                return (num / (float)1000000).ToString("f2") + "M";
            }
            else
            {
                return integer.ToString() + "M";
            }
        }
        else
        {
            return (num / (float)1000000000).ToString("f2") + "B";
        }

        //if (num < 100000)
        //{
        //    return num.ToString("n0");
        //}
        //else if (num < 10000000)
        //{
        //    return (num / 1000).ToString() + "K";
        //}
        //else
        //{
        //    return (num / 1000000).ToString() + "M";
        //}
    }

    public static string ToShortString(this int num)
    {
        return ((long)num).ToShortString();
    }

    public static string ToShortString(this float num)
    {
        return ((long)num).ToShortString();
    }
    #endregion

    #region TIMESPAN
    public static string GetFormattedTimerShort(this TimeSpan ts)
    {
        return string.Format("{0:00}:{1:00}:{2:00}", ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds);
    }

    public static string GetFormattedTimerLong(this TimeSpan timeSpan)
    {
        string s = string.Empty;

        //if (timeSpan.Days == 0)
        //{
        //    if (timeSpan.Hours < 1)
        //    {
        //        s = timeSpan.Minutes >= 2 ? string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Minutes"), timeSpan.Minutes)
        //        : string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Minute"), timeSpan.Minutes);
        //    }
        //    else
        //    {
        //        s = timeSpan.Hours >= 2 ? string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Hours"), timeSpan.Hours)
        //        : string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Hour"), timeSpan.Hours);
        //    }
        //}
        //else if (timeSpan.Days == 1)
        //{
        //    s = timeSpan.Hours >= 2 ? string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Day {1} Hours"), timeSpan.Days, timeSpan.Hours)
        //        : string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Day {1} Hour"), timeSpan.Days, timeSpan.Hours);
        //}
        //else if (timeSpan.Days > 1)
        //{
        //    s = timeSpan.Hours >= 2 ? string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Days {1} Hours"), timeSpan.Days, timeSpan.Hours)
        //        : string.Format(LocalizeManager.Instance.GetLocalizeText("{0} Days {1} Hour"), timeSpan.Days, timeSpan.Hours);
        //}

        return s;
    }

    public static string ToStringTimer<T>(this T seconds) where T : IConvertible
    {
        string formattedTime = string.Empty;
        double secondsAsDouble = Convert.ToDouble(seconds);
        TimeSpan ts = TimeSpan.FromSeconds(secondsAsDouble);

        if (ts.Hours >= 1f)
        {
            formattedTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }
        else
        {
            formattedTime = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }

        return formattedTime;
    }

    public static string ToStringTimerTwo<T>(this T seconds) where T : IConvertible
    {
        string formattedTime = string.Empty;
        double secondsAsDouble = Convert.ToDouble(seconds);
        TimeSpan ts = TimeSpan.FromSeconds(secondsAsDouble);

        if (ts.Days >= 1f)
        {
            formattedTime = string.Format("{0:00}d {1:00}h", ts.Days, ts.Hours);
        }
        else if (ts.Hours >= 1f)
        {
            formattedTime = string.Format("{0:00}h {1:00}m", ts.Hours, ts.Minutes);
        }
        else
        {
            formattedTime = string.Format("{0:00}m {1:00}s", ts.Minutes, ts.Seconds);
        }

        return formattedTime;
    }

    public static double ToMiliseconds(this DateTime time)
    {
        DateTime beginDate = new DateTime(1970, 1, 1);
        TimeSpan ts = time - beginDate;
        return ts.TotalMilliseconds;
    }

    public static DateTime ToDateTime(this double miliseconds)
    {
        return new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(miliseconds);
    }

    public static string ToStringDDMMYYYY(this DateTime time)
    {
        return time.ToString("dd/MM/yyyy");
    }
    #endregion

    //public static float GetTopPosition(this ScrollRect scrollRect)
    //{
    //    Canvas.ForceUpdateCanvases();

    //    var rect = scrollRect.GetComponent<RectTransform>();
    //    return scrollRect.horizontal ? (-rect.rect.width / 2) : (rect.rect.height / 2);
    //}
    //#endregion

    public static void SetAlpha(this Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    public static string ToStringNumber(this double num)
    {
        if (num < 1000000)
        {
            return num.ToString("n0");
        }
        else if (num < 1000000000)
        {
            int integer = (int)num / 1000000;
            int decim = (int)num % 1000000;

            if (decim >= 10000)
            {
                return (num / (float)1000000).ToString("f2") + "M";
            }
            else
            {
                return integer.ToString() + "M";
            }
        }
        else
        {
            return (num / (float)1000000000).ToString("f2") + "B";
        }
    }
}