using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserProfileData : BaseUserData
{
    public const int DEFAULT_CAMPAIGN_STAGE_ID = 1;
    public const int MAX_CAMPAIGN_STAGE_ID = 100000;
    public int currentStageId { get; set; } = DEFAULT_CAMPAIGN_STAGE_ID;
    public bool passed { get; set; }

    public string userName { get; set; }
    public long exp { get; set; }
    public double firstTimeLogin { get; set; } = ConstantValues.defaultDateMiliseconds;
    public double lastDayLogin { get; set; } = ConstantValues.defaultDateMiliseconds;
    public double lastTimeBackUp { get; set; } = ConstantValues.defaultDateMiliseconds;
    public int loginDayIndex { get; set; }
    public int loginMonthIndex { get; set; } = -1;
    public List<string> usedGiftcodes { get; set; } = new List<string>();

    // Attendance
    public int consecutiveLoginDayIndex { get; set; } = 0;
    public bool isClaimedLoginReward { get; set; } = false;
    public bool isClaimedConsecutiveReward { get; set; } = false;
    public double lastDayReceiveConsecutiveReward { get; set; } = ConstantValues.defaultDateMiliseconds;

    public long countWatchAds = 0;


    // Tutorial
    public int passedTutorialStages { get; set; }
    public List<int> completedTutorials { get; set; } = new List<int>();

    protected override string GetDataKey()
    {
        return UserData.DATA_KEY_PROFILE;
    }

    public override void ValidateData()
    {
        if (currentStageId < DEFAULT_CAMPAIGN_STAGE_ID)
        {
            currentStageId = DEFAULT_CAMPAIGN_STAGE_ID;
            isDataChanged = true;
        }

        if (currentStageId > MAX_CAMPAIGN_STAGE_ID)
        {
            currentStageId = MAX_CAMPAIGN_STAGE_ID;
            passed = true;
            isDataChanged = true;
        }
        try
        {

            DateTime now = MasterInfo.Instance.time.GetTimeNow();
            double nowMiliseconds = now.ToMiliseconds();

            if (firstTimeLogin == ConstantValues.defaultDateMiliseconds)
                firstTimeLogin = nowMiliseconds;

            if (lastDayLogin > nowMiliseconds)
            {
                lastDayLogin = nowMiliseconds;
            }
            else if (lastDayLogin == ConstantValues.defaultDateMiliseconds)
            {
                if (isClaimedLoginReward)
                {
                    lastDayLogin = now.Date.ToMiliseconds();
                }
                else
                {
                    lastDayLogin = now.AddDays(-31).ToMiliseconds();
                }
            }

            isDataChanged = true;

        }
        catch { }
    }

    public override void RefreshNewDay()
    {
        base.RefreshNewDay();

        if (!MasterInfo.Instance.isFetchedTime)
            return;

        if (loginMonthIndex == -1)
        {
            loginMonthIndex = (int)(lastDayLogin.ToDateTime().Date - firstTimeLogin.ToDateTime().Date).TotalDays / 30;
        }

        DateTime today = MasterInfo.Instance.time.GetTimeNow().Date;

        int dayPassed = 0;
        if (lastDayReceiveConsecutiveReward != ConstantValues.defaultDateMiliseconds)
        {
            dayPassed = (today - lastDayReceiveConsecutiveReward.ToDateTime().Date).Days;
        }

        if (consecutiveLoginDayIndex > 0 && dayPassed >= 2)
        {
            consecutiveLoginDayIndex = 0;
        }
        else
        {
            if (isClaimedConsecutiveReward && dayPassed >= 1)
            {
                consecutiveLoginDayIndex++;
            }

            if (consecutiveLoginDayIndex >= 7)
            {
                consecutiveLoginDayIndex = 0;
            }
        }

        lastDayLogin = today.ToMiliseconds();
        isClaimedConsecutiveReward = false;

        if (isClaimedLoginReward)
        {
            isClaimedLoginReward = false;
            loginDayIndex++;

            if (loginDayIndex > 29)
            {
                loginDayIndex = 0;
                loginMonthIndex++;
            }
        }

        isDataChanged = true;
        DebugCustom.LogFormat("FirstDay={0} | LastDay={1} | MonthIndex={2}",
            firstTimeLogin.ToDateTime(), lastDayLogin.ToDateTime(), loginMonthIndex);
    }


    #region Profile
    public int GetAccountLevel()
    {
        return 0;
        //return GameConfig.Instance.profileData.GetLevelByExp(exp);
    }

    public float GetAccountLevelProgress()
    {
        return 0;
        //return GameConfig.Instance.profileData.GetLevelProgress(exp);
    }

    public void ReceiveExpAccount(int quantity)
    {
        //if (quantity > 0)
        //{
        //    int levelBefore = GetAccountLevel();

        //    exp += quantity;
        //    if (exp > GameConfig.Instance.profileData.MaxExp)
        //        exp = GameConfig.Instance.profileData.MaxExp;

        //    int levelAfter = GetAccountLevel();
        //    if (levelAfter > levelBefore)
        //    {
        //        AccountLevelUpData levelUpData = new AccountLevelUpData(levelBefore, levelAfter);
        //        EventDispatcher.Instance.PostEvent(EventID.ACCOUNT_LEVEL_UP, levelUpData);
        //    }

        //    isDataChanged = true;
        //    EventDispatcher.Instance.PostEvent(EventID.ACCOUNT_EXP_CHANGE, quantity);
        //}
    }

    public int GetTotalTalentPoints()
    {
        int accLevel = GetAccountLevel();
        int points = accLevel * 2;

#if UNITY_EDITOR
        return 1000;
#endif

        return points;
    }
    #endregion

    #region Giftcodes
    public bool IsUsedGiftcode(string giftcode)
    {
        return usedGiftcodes.Contains(giftcode);
    }

    public void UseGiftcode(string giftcode)
    {
        if (IsUsedGiftcode(giftcode) == false)
        {
            usedGiftcodes.Add(giftcode);
            isDataChanged = true;
        }
    }
    #endregion

    #region Rename
    public void Rename(string newName)
    {
        userName = newName;
        isDataChanged = true;
        //EventDispatcher.Instance.PostEvent(EventID.CHANGE_NAME);
    }
    #endregion

    #region Attendance
    public bool IsClaimedAttendanceDaily(int index)
    {
        bool isClaimed = (index < loginDayIndex || (index == loginDayIndex && isClaimedLoginReward));
        return isClaimed;
    }

    public bool IsClaimedAttendanceConsecutiveDaily(int index)
    {
        if (index < consecutiveLoginDayIndex)
            return true;

        if (index == consecutiveLoginDayIndex)
            return isClaimedConsecutiveReward;

        return false;
    }

    public void ClaimAttendanceRewardDaily()
    {
        if (isClaimedLoginReward)
            return;
        isClaimedLoginReward = true;
        isDataChanged = true;
    }

    public void ClaimAttendanceRewardConsecutive()
    {
        //if (isClaimedConsecutiveReward)
        //    return;

        //isClaimedConsecutiveReward = true;
        //lastDayReceiveConsecutiveReward = MasterInfo.Instance.time.GetTimeNow().Date.ToMiliseconds();

        //isDataChanged = true;
    }

    #endregion

    public void IncreaseTimeWatchAds()
    {
        countWatchAds++;
        isDataChanged = true;
        //Debug.Log("IncreaseTimeWatchAds => CountWatchAds = " + countWatchAds);
    }

    #region Stage
    public void EndStage(bool isWin)
    {
        if (isWin)
        {
            if (currentStageId < MAX_CAMPAIGN_STAGE_ID)
            {
                currentStageId++;
                passed = false;
            }
            else
            {
                currentStageId = MAX_CAMPAIGN_STAGE_ID;
                passed = true;
            }
        }
        else
        {
            passed = false;
        }

        GameData.userData.Save();
        //isDataChanged = true;
    }

    #endregion
}
