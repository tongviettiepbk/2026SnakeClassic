using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UserData
{
    public const string DATA_KEY_PROFILE = "key_user_profile";
    public const string DATA_KEY_SHOP = "key_user_shop";
    public const string DATA_KEY_ITEMS = "key_user_items";

    public UserProfileData profile;
    public UserShopData shops;
    public UserItemData items;

    public bool isDataReplaced;
    public bool isDisableSaveDataLocal;

    private List<BaseUserData> listData;
    private float lastTimeSaveData;

    [JsonIgnore] public bool isNewUser { get; private set; }

    #region Data
    public void ValidateData()
    {
        if (listData == null)
        {
            listData = new List<BaseUserData>();
            listData.Add(profile);
            listData.Add(items);
            listData.Add(shops);
        }

        for (int i = 0; i < listData.Count; i++)
        {
            try
            {
                listData[i].ValidateData();
            }
            catch (Exception e)
            {
                DebugCustom.LogError(e.Message);
                DebugCustom.LogFormat("i={0}, data={1}", i, JsonConvert.SerializeObject(listData[i]));
                continue;
            }
        }
    }

    public void Save(bool isForceSave = true)
    {
        DebugCustom.Log("[UserData] Save");
        if (isDisableSaveDataLocal)
        {
            return;
        }

        float interval = 1f;
#if UNITY_EDITOR
        interval = 1f;
#endif

        if (isForceSave || Time.realtimeSinceStartup - lastTimeSaveData > interval)
        {
            lastTimeSaveData = Time.realtimeSinceStartup;
            bool savePrefs = false;

            for (int i = 0; i < listData.Count; i++)
            {
                BaseUserData data = listData[i];
                if (data.Save(isForceSave))
                {
                    savePrefs = true;
                }
                ;
            }

            if (savePrefs)
            {
                PlayerPrefs.Save();
            }
        }
    }

    public void ReplaceWithNewData(UserData newData)
    {
        if (newData != null)
        {
            try
            {
                GameData.Reset();
                GameData.userData = newData;
                GameData.userData.isDataReplaced = true;
                DebugCustom.Log("[Replace] NewUserData=" + JsonConvert.SerializeObject(GameData.userData));
            }
            catch { }
        }
    }
    #endregion

    #region Time
    public void CheckNewDay()
    {
        //if (MasterInfo.Instance.isFetchedTime)
        //{
        //    DateTime now = MasterInfo.Instance.time.GetTimeNow();
        //    if (now.Date > profile.lastDayLogin.ToDateTime().Date)
        //    {
        //        Debug.LogFormat("[NewDay] lastDayLogin={0}, now={1}", profile.lastDayLogin.ToDateTime(), now);
        //        RefreshNewDay();
        //    }
        //}
    }

    public void RefreshNewDay()
    {
        //if (MasterInfo.Instance.isFetchedTime == false)
        //{
        //    return;
        //}

        //DebugCustom.Log("[NewDay]");
        //DateTime now = MasterInfo.Instance.time.GetTimeNow();
        //DateTime lastDayLogin = GameData.userData.profile.lastDayLogin.ToDateTime();

        //if (now.Year != lastDayLogin.Year)
        //{
        //    RefreshNewYear();
        //}

        //if (now.Year != lastDayLogin.Year || now.Month != lastDayLogin.Month)
        //{
        //    RefreshNewMonth();
        //}

        //string lastWeek = MasterInfo.Instance.time.GetWeekRangeString(lastDayLogin);
        //string curWeek = MasterInfo.Instance.time.GetCurrentWeekRangeString();
        //if (curWeek != lastWeek)
        //{
        //    RefreshNewWeek();
        //}

        //for (int i = 0; i < listData.Count; i++)
        //{
        //    listData[i].RefreshNewDay();
        //}

        //EventDispatcher.Instance.PostEvent(EventID.NEW_DAY);
    }

    public void RefreshNewWeek()
    {
        //DebugCustom.Log("[UserData] Refresh New Week");

        //for (int i = 0; i < listData.Count; i++)
        //{
        //    listData[i].RefreshNewWeek();
        //}

        //EventDispatcher.Instance.PostEvent(EventID.NEW_WEEK);
    }

    public void RefreshNewMonth()
    {
        //DebugCustom.Log("[UserData] Refresh New Month");

        //for (int i = 0; i < listData.Count; i++)
        //{
        //    listData[i].RefreshNewMonth();
        //}

        //EventDispatcher.Instance.PostEvent(EventID.NEW_MONTH);
    }

    public void RefreshNewYear()
    {
        //DebugCustom.Log("[UserData] Refresh New Year");

        //for (int i = 0; i < listData.Count; i++)
        //{
        //    listData[i].RefreshNewYear();
        //}

        //EventDispatcher.Instance.PostEvent(EventID.NEW_YEAR);
    }
    #endregion

    #region Load
    public void Load()
    {
        if (isDataReplaced == false)
        {
            LoadProfile();
            LoadShop();
            LoadItems();
        }

        // LoadDone
        ValidateData();
        isDisableSaveDataLocal = false;

        if (isNewUser)
        {
            RefreshNewDay();
        }

        if (isNewUser)
        {
            ItemUtils.Receive(ItemType.GOLD, GameConfig.goldNewUser, showPopup: false, playSfx: false);
            Save(true);
            return;
        }

        if (isDataReplaced)
        {
            Save(true);
            isDataReplaced = false;
        }
    }

    private void LoadProfile()
    {
        string prefs = PlayerPrefs.GetString(DATA_KEY_PROFILE);
        if (string.IsNullOrEmpty(prefs))
        {
            profile = new UserProfileData();
            isNewUser = true;
        }
        else
        {
            try
            {
                profile = JsonConvert.DeserializeObject<UserProfileData>(prefs);
            }
            catch
            {
                profile = new UserProfileData();
                isNewUser = true;
                DebugCustom.LogError("LoadProfile");
            }
        }

        DebugCustom.Log("profile=" + JsonConvert.SerializeObject(profile));
    }

    private void LoadShop()
    {
        string prefs = PlayerPrefs.GetString(DATA_KEY_SHOP);
        if (string.IsNullOrEmpty(prefs))
        {
            shops = new UserShopData();
        }
        else
        {
            try
            {
                shops = JsonConvert.DeserializeObject<UserShopData>(prefs);
            }
            catch
            {
                shops = new UserShopData();
                DebugCustom.LogError("LoadShop:");
            }
        }

        DebugCustom.Log("shops=" + JsonConvert.SerializeObject(shops));
    }

    private void LoadItems()
    {
        string prefs = PlayerPrefs.GetString(DATA_KEY_ITEMS);
        if (string.IsNullOrEmpty(prefs))
        {
            items = new UserItemData();
            items.Initialize();
        }
        else
        {
            try
            {
                items = JsonConvert.DeserializeObject<UserItemData>(prefs);
            }
            catch
            {
                items = new UserItemData();
                items.Initialize();
                DebugCustom.LogError("LoadItems");
            }
        }

        DebugCustom.Log("items=" + JsonConvert.SerializeObject(items));
    }
    #endregion

}
