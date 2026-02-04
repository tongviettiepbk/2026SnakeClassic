using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum FadeColor
{
    White,
    Black
}

public class UIManager : Singleton<UIManager>
{
    public string UI_PREFAB_PATH = "UI/";

    //public CellReward prefabCellReward;
    public PopupNotice notice;
    public PopupReward reward;
    public PopupToastMessage toast;
    public PopupWaiting waiting;
    public PopupItemInfo itemInfo;
    public TooltipRewards tooltipRewards;

    public GameObject shieldUI;
    public Image imgFade;
    public RectTransform groupScreenOverlayUI;

    private bool isFading;
    private Dictionary<string, BaseUI> cachedUIs = new Dictionary<string, BaseUI>();
    private Stack<BaseUI> activeUIs = new Stack<BaseUI>();

    private const int DEFAULT_SORTING_ORDER_OVERLAY = 1000;
    private const int DEFAULT_SORTING_ORDER = 5;
    private const int SORTING_ORDER_STEP = 20;

    protected void Awake()
    {
        DontDestroyOnLoad(this);

        try
        {
            QualitySettings.vSyncCount = 0;
            DebugCustom.ShowDebugColorRed("systemMemorySize", SystemInfo.systemMemorySize);
            if (SystemInfo.systemMemorySize > 3500)
            {
                Application.targetFrameRate = 60;
            }
            else
            {
                Application.targetFrameRate = 30;
                GameConfig.isLowGraphic = true;
                //GameConfig.MOVE_OUT_ZIGZAG = false;
                //GameConfig.SMOOTH_CORNER = false;
            }
                
        }
        catch
        {

        }
        

    }

    void Start()
    {
        float width = groupScreenOverlayUI.sizeDelta.x;
        float height = groupScreenOverlayUI.sizeDelta.x;
        Debug.Log($"SCREEN SIZE: {width}, {height}");

        if (width > height)
        {
            GameConfig.RATIO = width / height;
        }
        else
        {
            GameConfig.RATIO = height / width;

        }
        GameConfig.IS_TABLET = GameConfig.RATIO <= 1.6f;
    }

    public BaseUI LoadUI(string key, bool isBackable = true, bool isPoolingWhenClose = false, bool isOverlay = true, bool isPauseMusic = false)
    {
        BaseUI obj = null;
        if (isPauseMusic)
        {
            AudioManager.Instance.PauseMusic();
        }

        if (cachedUIs.ContainsKey(key))
        {
            obj = cachedUIs[key];

            if (obj == null)
            {
                DebugCustom.LogError("Error obj null in dictionary (key=" + key + "). Remove and reload.");
                cachedUIs.Remove(key);
            }
            else
            {
                obj.transform.SetParent(null);
            }
        }

        if (obj == null)
        {
            BaseUI prefab = Resources.Load<BaseUI>(UI_PREFAB_PATH + key);

            if (prefab == null)
            {
                DebugCustom.LogError("UI key not found=" + key);
                return null;
            }

            obj = Instantiate(prefab);
            obj.gameObject.name = key;
            cachedUIs.Add(key, obj);
        }

        if (activeUIs.Contains(obj) == false)
            activeUIs.Push(obj);

        Canvas canvas = obj.GetComponent<Canvas>();
        if (canvas != null)
        {
            if (isOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                List<BaseUI> remainingUIs = activeUIs.Where(x => x != null && x.isOverlay).ToList();
                canvas.sortingOrder = DEFAULT_SORTING_ORDER_OVERLAY + ((remainingUIs.Count + 1) * SORTING_ORDER_STEP);
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.sortingLayerName = ConstantValues.SORTING_LAYER_OVERLAY;

                List<BaseUI> remainingUIs = activeUIs.Where(x => x != null && x.isOverlay == false).ToList();
                canvas.sortingOrder = DEFAULT_SORTING_ORDER + ((remainingUIs.Count + 1) * SORTING_ORDER_STEP);
            }
        }

        obj.isOverlay = isOverlay;
        obj.isBackable = isBackable;
        obj.isPoolingWhenClose = isPoolingWhenClose;
        obj.isLoadFromResources = true;
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void HideUI(BaseUI uiObject)
    {
        if (!cachedUIs.ContainsKey(uiObject.gameObject.name) && uiObject.isPoolingWhenClose)
        {
            cachedUIs.Add(uiObject.gameObject.name, uiObject);
        }

        BaseUI lastestPopup = activeUIs.Peek();
        if (lastestPopup != null)
        {
            if (lastestPopup == uiObject)
            {
                activeUIs.Pop();

                if (!uiObject.isPoolingWhenClose)
                {
                    if (cachedUIs.ContainsKey(uiObject.gameObject.name))
                    {
                        cachedUIs.Remove(uiObject.gameObject.name);
                    }

                    Destroy(uiObject.gameObject);
                }
                else
                {
                    uiObject.transform.parent = groupScreenOverlayUI;
                }
            }
            else
            {
                DebugCustom.Log(string.Format("HideUI={0}, LastestUI={1}", uiObject.name, lastestPopup.name));

                if (cachedUIs.ContainsKey(uiObject.gameObject.name))
                {
                    cachedUIs.Remove(uiObject.gameObject.name);
                }

                Destroy(uiObject.gameObject);
            }
        }
    }

    public bool Back()
    {
        if (isFading || waiting.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (tooltipRewards.gameObject.activeInHierarchy)
        {
            tooltipRewards.gameObject.SetActive(false);
            return true;
        }

        if (itemInfo.gameObject.activeInHierarchy)
        {
            itemInfo.gameObject.SetActive(false);
            return true;
        }

        List<BaseUI> overlayUIs = activeUIs.Where(x => x.isOverlay).OrderByDescending(x => x.GetComponent<Canvas>()?.sortingOrder).ToList();
        if (overlayUIs.Count > 0)
        {
            BaseUI backUI = activeUIs.Peek();
            if (backUI == overlayUIs[0])
            {
                backUI.Back();
            }

            return true;
        }

        if (notice.IsBackable())
        {
            notice.Back();
            return true;
        }

        if (reward.IsBackable())
        {
            reward.Back();
            return true;
        }

        if (activeUIs.Count > 0)
        {
            BaseUI popup = activeUIs.Peek();
            if (popup != null)
            {
                popup.Back();
                return true;
            }
        }

        return false;
    }

    public bool IsAnyActiveUI()
    {
        return activeUIs.Count > 0;
    }

    public void HideAllActiveUI()
    {
        List<BaseUI> popups = activeUIs.ToList();
        for (int i = 0; i < popups.Count; i++)
        {
            var p = popups[i];
            if (p != null)
            {
                p.Close();
            }
        }
    }

    public void ActiveShield(bool isOn, float delayDeactive = 0f, bool isBattle = false)
    {
        shieldUI.SetActive(isOn);

        if (isOn)
        {
            float timeDelayDeactive = delayDeactive > 0f ? delayDeactive : 10f;
            this.StartDelayAction(timeDelayDeactive, () =>
            {
                if (shieldUI.activeInHierarchy)
                {
                    shieldUI.SetActive(false);
                }

            });
        }
    }

    #region Message

    public void ShowNotice(string content, bool isLocalizeContent = true, PopupNoticeType popupType = PopupNoticeType.YesNo, bool isBackable = true,
        TextAlignmentOptions textAnchor = TextAlignmentOptions.Center, string title = "Notice", string labelYes = "Confirm", string labelNo = "Cancel", bool titleToUpper = true,
        UnityAction yesCallback = null, UnityAction noCallback = null, UnityAction closeCallback = null)
    {
        if (isLocalizeContent)
        {
            content = LocalizeManager.Instance.GetLocalizeText(content);
        }

        title = LocalizeManager.Instance.GetLocalizeText(title, isToUpper: titleToUpper);
        labelYes = LocalizeManager.Instance.GetLocalizeText(labelYes);
        labelNo = LocalizeManager.Instance.GetLocalizeText(labelNo);

        notice.Show(content, popupType, isBackable, textAnchor, title, labelYes, labelNo, yesCallback, noCallback, closeCallback);
    }

    public void ShowToastMessage(string content, bool isLocalize = true)
    {
        if (isLocalize)
        {
            content = LocalizeManager.Instance.GetLocalizeText(content);
        }

        toast.Show(content);
    }

    public void ShowToastMessage(ToastMessageType type)
    {
        string content = string.Empty;

        switch (type)
        {
            case ToastMessageType.DEFAULT_ERROR: content = "An error occurred. Please try again later."; break;
            case ToastMessageType.NETWORK_CONNECTION_FAILED: content = "Unable to connect. Please check your network connection."; break;
            case ToastMessageType.INSUFFICIENT_RESOURCES: content = "Not Enough Gold."; break;
            case ToastMessageType.SUSPENDED_ACCOUNT: content = "Your account has been suspended."; break;
            case ToastMessageType.SUSPENDED_ACCOUNT_AND_BLOCK_DEVICE: content = "Your account and your device have been suspended."; break;
            case ToastMessageType.UPDATE_NEW_VERSION: content = "Please update the lastest game version."; break;
            case ToastMessageType.SERVER_MAINTANCE: content = "Server is under maintenance."; break;

            case ToastMessageType.PURCHASE_SUCCESS: content = "Purchase was successfully."; break;
            case ToastMessageType.COME_BACK_TOMORROW: content = "Please come back tomorrow."; break;
            case ToastMessageType.COMING_SOON: content = "Coming soon next updated."; break;
            case ToastMessageType.HEART_FULL: content = "Heart is Full"; break;

        }

        ShowToastMessage(content, isLocalize: true);
    }
    #endregion

    #region Waiting
    public void ShowWaiting(bool isOn, bool isTimeOut = true)
    {
        if (isOn)
        {
            int timeOut = isTimeOut ? 20 : 0;
            waiting.Show(timeOut);
        }
        else
        {
            waiting.Close();
        }
    }
    #endregion

    #region Fade
    public void FadeToLoadScene(string sceneName = null, UnityAction actionBeforeLoad = null)
    {
        Fade(color: FadeColor.Black, toMaxCallback: () =>
        {
            if (actionBeforeLoad != null)
                actionBeforeLoad();

            if (!string.IsNullOrEmpty(sceneName))
            {
                GameConfig.sceneNext = sceneName;
                SceneManager.LoadScene(GameConfig.SCENE_LOADING);
            }
        });
    }

    public void Fade(FadeColor color = FadeColor.White, float fadingSpeedToMax = 7f, float fadingSpeedBackToMin = 1f,
        UnityAction toMaxCallback = null, UnityAction toMinCallback = null)
    {
        if (isFading == false)
        {
            isFading = true;
            ActiveShield(true);
            StartCoroutine(CoroutineFade(color, fadingSpeedToMax, fadingSpeedBackToMin, toMaxCallback, toMinCallback));
        }
    }

    private IEnumerator CoroutineFade(FadeColor color, float fadingSpeedToMax, float fadingSpeedBackToMin,
        UnityAction toMaxCallback, UnityAction toMinCallback)
    {
        imgFade.color = color == FadeColor.White ? Color.white : Color.black;
        Color c = imgFade.color;
        c.a = 0f;
        imgFade.color = c;
        imgFade.gameObject.SetActive(true);
        bool isFadingToMax = true;

        while (isFading)
        {
            if (isFadingToMax)
            {
                c.a = Mathf.MoveTowards(c.a, 1f, fadingSpeedToMax * Time.deltaTime);
                imgFade.color = c;

                if (c.a >= 0.95f)
                {
                    c.a = 1f;
                    imgFade.color = c;
                    isFadingToMax = false;

                    if (toMaxCallback != null)
                    {
                        yield return new WaitForEndOfFrame();
                        toMaxCallback();
                    }
                }
            }
            else
            {
                c.a = Mathf.MoveTowards(c.a, 0f, fadingSpeedBackToMin * Time.deltaTime);
                imgFade.color = c;

                if (c.a <= 0.05f)
                {
                    c.a = 0f;
                    imgFade.color = c;
                    isFading = false;

                    if (toMinCallback != null)
                    {
                        yield return new WaitForEndOfFrame();
                        toMinCallback();
                    }

                    ActiveShield(false);
                    imgFade.gameObject.SetActive(false);
                }
            }

            yield return null;
        }
    }

    public void ClearAllUI()
    {
        activeUIs.Clear();

        foreach (var kv in cachedUIs)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value.gameObject);
            }
        }

        cachedUIs.Clear();
    }

    #endregion

    #region Rewards
    public void ShowItemInfo(RewardData data, bool showButtonUse = false)
    {
        if (data != null)
        {
            if (itemInfo.gameObject.activeSelf)
            {
                itemInfo.Close();
            }

            itemInfo.Open(data, showButtonUse);
        }
    }

    public void ShowRewards(List<RewardData> rewards, bool playSfx = true)
    {
        if (rewards != null)
        {
            reward.Open(rewards, playSfx);
        }
    }
    #endregion

}