using System;
using System.Collections;
using UnityEngine;

public enum AppStatus
{
    NORMAL = 0,
    SHOULD_UPDATE = 1,
    FORCE_UPDATE = 2,
}

public class MasterInfo : Singleton<MasterInfo>
{
    public TimeManager time;


    public static int SHOULD_UPDATE_VERSION = 0;
    public static int FORCE_UPDATE_VERSION = 0;

    public bool isFetchedTime = true;
    public bool isFetchedConfig = true;

    private DateTime today = DateTime.Now;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public AppStatus GetAppStatus()
    {
        return AppStatus.NORMAL;
    }

    public bool CheckForceUpdate()
    {
        return false;
    }

    public bool FullCheck()
    {
        return true;
    }

    private bool CheckGameVersion()
    {
        if (CheckRemoteConfig())
        {
            if (GetAppStatus() != AppStatus.NORMAL)
            {
                UIManager.Instance.ShowToastMessage(ToastMessageType.UPDATE_NEW_VERSION);
                return false;
            }
        }

        return true;
    }

    private bool CheckTime()
    {
        if (isFetchedTime == false)
        {
            UIManager.Instance.ShowToastMessage(ToastMessageType.NETWORK_CONNECTION_FAILED);
            return false;
        }

        return true;
    }

    private bool CheckRemoteConfig()
    {
        if (isFetchedConfig == false)
        {
            UIManager.Instance.ShowToastMessage(ToastMessageType.NETWORK_CONNECTION_FAILED);
            return false;
        }

        return true;
    }
    #region Test
    public DateTime GetTimeNow()
    {
        return today;
    }

    public void AdvanceOneDay()
    {
        today = today.AddDays(1);
        Debug.Log("fake time: " + today.ToString("dddd, yyyy-MM-dd"));
    }
    #endregion
}
