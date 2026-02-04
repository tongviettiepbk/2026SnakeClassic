using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public enum LevelMode
{
    Normal = 0,
    Hard = 1,
    VeryHard = 2
}

public class LGamePlay : BaseUI
{
    public static LGamePlay Instance;

    [Space(20)]
    public Animation anim;
    public Button btRestart;
    public Button btShop;
    public Button btSetting;
    public Button btShowLine;
    public Button btHack;
    public Button btClickToCenter;
    public Button btPopupRemoveAds;
    public GameObject objRemoveAds;

    [Space(20)]
    public GameObject objTimeNormal;
    public GameObject objBgTimeSnow;
    public GameObject objBgTimeHard;

    public Sprite icClockFrezzy;
    public TMP_Text txLevel;
    public TMP_Text txTime;

    public TimeGamePlay objTime;

    [Space(20)]
    public PopupIngameBooster popup;

    public GameObject objClickToEnter;
    public GameObject objListBooster;
    public NoticeBGAnim bgRed;
    public GameObject bgIce;


    public int remainingTime;   // để refill
    public Image bgEndGame;

    public List<HeartAnim> lives;
    public List<ItemBooster> itemBoosters = new List<ItemBooster>();

    [Space(20)]
    public PanelTutBooster pannelTut;

    [Space(20)]
    public ToastIngame toastIngame;

    [Space(20)]
    public CanvasGroup canvasGroup;


    // biên
    public Transform transformCenter;
    private Coroutine coroutineTime;

    private int live;
    private float timeAnimFreezy = 2f;
    private float freezeUntilTime = 0f;
    private bool isFreezy = false;
    private float time;
    private LevelMode currentMode = LevelMode.Normal;
    private bool gameplayStarted = false;

    #region InitMethods

    private void Start()
    {
        live = GameConfig.LiveWhenStartGame;
        CheckLiveInGame();

        MediationAds.Instance.HideMRec();
        MediationAds.Instance.ShowBannerAd();
    }



    protected override void Awake()
    {
        Instance = this;
        btRestart.onClick.AddListener(ClickBtRestart);
        btShop.onClick.AddListener(ClickBtShop);
        btSetting.onClick.AddListener(ClickBtSetting);
        btClickToCenter.onClick.AddListener(ClickToCenter);
        btShowLine.onClick.AddListener(ClickShowLine);
        btHack.onClick.AddListener(ClickBtHack);
        btPopupRemoveAds.onClick.AddListener(ClickBtRemoveAds);
    }

    protected override void OnEnable()
    {
        currentMode = LevelMode.Normal;
        SetLevel();
        CheckFreezy(isFreezy);
        SetClock(currentMode);
        //CheckBooster();
        btRestart.interactable = false;

        SimpleEventManager.Instance.Register(EventIDSimple.startgame, OnStartGame);
        SimpleEventManager.Instance.Register(EventIDSimple.showBannerSucess, SetBanner);
        SimpleEventManager.Instance.Register(EventIDSimple.showBtRemoveAds, SetButton);

        SetPosYFooter();
        this.StartDelayAction(1, () =>
        {
            if (MapController.Instance.isHack == true)
            {
                btHack.gameObject.SetActive(true);
            }

        });

        objRemoveAds.gameObject.SetActive(!GameData.userData.shops.isPurchaseRemoveAds);
    }

    protected override void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.startgame, OnStartGame);
        SimpleEventManager.Instance.Unregister(EventIDSimple.showBannerSucess, SetBanner);
        SimpleEventManager.Instance.Unregister(EventIDSimple.showBtRemoveAds, SetButton);
        StopAllCoroutines();
        coroutineTime = null;
    }

    public void Init()
    {
        CheckActiveBooster();
    }

    private void ResetFreezeState()
    {
        freezeUntilTime = 0f;
        isFreezy = false;
        CheckFreezy(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            RequestMoveFail();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            //PlayAnim();
        }
    }

    public void SetLevel()
    {
        txTime.gameObject.SetActive(true);
        txLevel.gameObject.SetActive(true);
        string format = LocalizeManager.Instance.GetLocalizeText("LEVEL ");
        int level = GameData.userData.profile.currentStageId;
        txLevel.text = string.Format(format + level);
    }

    public void SetTime(float timeGame)
    {
#if UNITY_EDITOR
        //timeGame = 75f;
#endif
        this.time = timeGame;
        objTime.SetTextTime((int)timeGame);

        // Xử lý booster Show Line
        if (MapController.Instance.levelCurrent < GameConfig.lvlTutShowLineGecko)
        {
            btShowLine.gameObject.SetActive(false);
        }
        else
        {
            btShowLine.gameObject.SetActive(true);
        }
    }

    //0-normal, 1-hard , 2 veryhard
    public void SetTypeLevel(int typeLevel)
    {
        currentMode = (LevelMode)typeLevel;

        SetClock(currentMode);

        if (currentMode == LevelMode.Hard || currentMode == LevelMode.VeryHard)
        {
            this.StartDelayAction(0.4f, () =>
            {
                LLevelHard ui = UIManager.Instance.LoadUI(UIKey.LEVELHARD) as LLevelHard;
            });
            SimpleEventManager.Instance.PostEvent(EventIDSimple.hardModeStart, this);
        }




    }
    #endregion

    #region SetPos
    private void SetBanner(object param)
    {
        GameConfig.isAdsOn = true;
    }

    private void SetPosYFooter()
    {
        if (!GameConfig.isAdsOn)
        {
            return;
        }

        RectTransform rt = objListBooster.GetComponent<RectTransform>();
        if (rt == null)
        {
            return;
        }

        Vector2 pos = rt.anchoredPosition;
        pos.y += GameConfig.posYBanner;
        rt.anchoredPosition = pos;
    }

    #endregion

    #region Button
    private void ClickBtRestart()
    {
        UIManager.Instance.LoadUI(UIKey.RESTART);
        //SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);
    }

    private void ClickBtShop()
    {
        UIManager.Instance.LoadUI(UIKey.SHOP);
        //SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);
    }

    private void ClickBtSetting()
    {
        UIManager.Instance.LoadUI(UIKey.PAUSE);
        //SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);
    }

    public void ShowClickToCenter(bool isShow)
    {
        objClickToEnter.SetActive(isShow);
    }

    private void ClickToCenter()
    {
        CameraController.Instance.RecenterCamera();
        ShowClickToCenter(false);
    }

    private void ClickShowLine()
    {
        MapController.Instance.ShowDirectionLine();
    }

    private void ClickBtRemoveAds()
    {
        UIManager.Instance.LoadUI(UIKey.ADS);
    }
    #endregion

    #region Boostter

    private void CheckBooster()
    {
        //if (MapController.Instance.isStartGame)
        //{
        //    for (int i = 0; i < itemBoosters.Count; i++)
        //    {
        //        var item = itemBoosters[i];
        //        item.canvasGroup.blocksRaycasts = false;
        //    }
        //}
        //else
        //{
        //    for (int i = 0; i < itemBoosters.Count; i++)
        //    {
        //        var item = itemBoosters[i];
        //        item.canvasGroup.blocksRaycasts = true;
        //    }
        //}
    }
    private void CheckActiveBooster()
    {
        for (int i = 0; i < itemBoosters.Count; i++)
        {
            var item = itemBoosters[i];
            long quantity = ItemUtils.GetQuantityHave(item.type);
            item.Initiliaze(quantity);
        }
    }

    public void DoneUseBooster(ItemType type)
    {
        ItemUtils.Consume(type, 1);
        ReLoadFooter();
    }

    public void ReLoadFooter()
    {
        for (int i = 0; i < itemBoosters.Count; i++)
        {
            var itemBooster = itemBoosters[i];
            itemBooster.lastType = ItemType.NONE;

            long quantity = ItemUtils.GetQuantityHave(itemBooster.type);
            itemBooster.Reload(quantity);
        }

        objListBooster.gameObject.SetActive(true);
        popup.gameObject.SetActive(false);
    }

    public void HideBooster(Booster type)
    {
        for (int i = 0; i < itemBoosters.Count; i++)
        {
            var booster = itemBoosters[i];
            if (booster.typeBooster == type)
            {
                booster.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Time-GamePLay
    private void OnStartGame(object param)
    {
        DebugCustom.ShowDebugColorRed("Start Game");
        ResetFreezeState();
        gameplayStarted = true;
        btRestart.interactable = true;
        CheckBooster();
        remainingTime = (int)time;
        for (int i = 0; i < itemBoosters.Count; i++)
        {
            var itemBooster = itemBoosters[i];
            itemBooster.ReloadIcon(itemBooster.type);
            itemBooster.canvasGroup.blocksRaycasts = true;
        }

        if (coroutineTime != null)
        {
            StopCoroutine(coroutineTime);
        }

        DebugCustom.ShowDebug("OnStartGame", time);
        coroutineTime = StartCoroutine(CoroutineTime(remainingTime));
    }

    private IEnumerator CoroutineTime(float totalSeconds)
    {
        remainingTime = Mathf.CeilToInt(totalSeconds);

        while (remainingTime > 0)
        {
            if (Time.time < freezeUntilTime)
            {
                //Debug.LogFormat($"[TIMER] Frozen... Time.time={Time.time:F2}  freezeUntil={freezeUntilTime:F2}");

                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (isFreezy)
            {
                isFreezy = false;
                CheckFreezy(isFreezy);
            }
            objTime.SetTextTime(remainingTime);

            yield return new WaitForSeconds(1f);
            if (remainingTime <= 10 && remainingTime > 0)
            {
                objTime.PlayAnim();
            }
            else
            {
                objTime.StopAnim();
            }

            remainingTime--;
        }

        objTime.SetTextTime(0);

        DebugCustom.ShowDebug("1");
        objTime.StopAnim();
        OnTimeOut();
    }

    private void OnTimeOut()
    {
        UIManager.Instance.LoadUI(UIKey.OUTOFTIME, isPauseMusic: true);
    }

    private void CheckFreezy(bool isActiveFreezy)
    {
        bgIce.SetActive(isActiveFreezy);
        objBgTimeSnow.SetActive(isActiveFreezy);
    }

    public void StopTimer()
    {
        objTime.StopAnim();
        if (coroutineTime != null)
        {
            StopCoroutine(coroutineTime);
            coroutineTime = null;
        }
    }

    public void CountinueTimer()
    {
        if (!gameplayStarted)
        {
            return;
        }

        if (this == null)
        {
            return;
        }

        if (gameObject == null)
        {
            return;
        }

        if (coroutineTime == null)
        {
            DebugCustom.ShowDebug("CountinueTimer", remainingTime);
            coroutineTime = StartCoroutine(CoroutineTime(remainingTime));
        }
    }

    public void StartFreezeTime()
    {
        isFreezy = true;
        var item = GameData.staticData.items.GetData(ItemType.TIME_STOP_ICE);

        bool isFreezing = Time.time < freezeUntilTime;
        float freezeDuration = item.value;
        if (!isFreezing)
        {
            freezeDuration += timeAnimFreezy;
        }

        freezeUntilTime = Mathf.Max(freezeUntilTime, Time.time) + freezeDuration;

        MapController.Instance.DoneUserBooster(ItemType.TIME_STOP_ICE);

        Vector3 tempVector3 = CameraController.Instance.GetCenterMap();
        tempVector3.y = 0;
        FxController.Instance.ShowClock(tempVector3, () =>
        {
            CheckFreezy(isFreezy);
            for (int i = 0; i < itemBoosters.Count; i++)
            {
                var itemsBooster = itemBoosters[i];
                if (itemsBooster.type == ItemType.TIME_STOP_ICE)
                {
                    this.StartDelayAction(2f, () =>
                        {
                            itemsBooster.btBooster.interactable = true;

                        });
                }
            }
        });
    }

    public void RefillTime(int addTime)
    {
        txTime.transform.localScale = Vector3.one;
        remainingTime += addTime;

        DebugCustom.ShowDebug("RefillTime", remainingTime);

        StopTimer();
        CountinueTimer();
    }

    public void Revival(int time, int heart)
    {
        txTime.transform.localScale = Vector3.one;
        live += heart;
        remainingTime += time;

        CheckLiveInGame();
        DebugCustom.ShowDebug("Revival", remainingTime);

        StopTimer();
        CountinueTimer();
    }

    #endregion

    private void CheckLiveInGame()
    {
        for (int i = 0; i < lives.Count; i++)
        {
            var heart = lives[i];

            heart.gameObject.SetActive(true);

            heart.transform.localScale = Vector3.one;
            heart.transform.localRotation = Quaternion.identity;

            var cg = heart.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
            }
            DG.Tweening.DOTween.Kill(heart.transform, true);
        }

        for (int i = live; i < lives.Count; i++)
        {
            lives[i].gameObject.SetActive(false);
        }

        if (live == 0)
        {
            //this.StartDelayAction(0.5f, () =>
            //{
            //    UIManager.Instance.LoadUI(UIKey.REVIVAL);
            //});
            UIManager.Instance.LoadUI(UIKey.REVIVAL);
        }
    }

    public void RequestMoveFail()
    {
        live--;
        if (live < 0)
        {
            live = 0;
        }
        bgRed.gameObject.SetActive(true);
        CheckLiveInGame();
    }

    private void ClickBtHack()
    {
        MapController.Instance.SetEndGame(true);
    }

    public void PlayAnim()
    {
        if (anim == null)
            return;

        anim.Play("open-ui-gameplay");
    }

    private void SetClock(LevelMode levelMode)
    {
        objBgTimeHard.SetActive(false);
        objTimeNormal.SetActive(true);

        switch (levelMode)
        {
            case LevelMode.Normal:
                objBgTimeHard.SetActive(false);
                objTimeNormal.SetActive(true);
                break;

            case LevelMode.Hard:
                objBgTimeHard.SetActive(true);
                objTimeNormal.SetActive(false);
                break;

            case LevelMode.VeryHard:
                objBgTimeHard.SetActive(true);
                objTimeNormal.SetActive(false);
                break;
        }

    }

    public int GetLiveNumber()
    {
        return live;
    }

    #region Tutorial

    public void ActiveHandBoosterTut(Booster type)
    {
        ItemBooster buttonBooster = null;

        if (type != Booster.SHOW_LINE)
        {
            buttonBooster = itemBoosters.Find(x => x.typeBooster == type);
            pannelTut.SetPosTut(buttonBooster.transform.position, type);
        }
        else
        {
            DebugCustom.ShowDebug("active hand show line");
            pannelTut.SetPosTut(btShowLine.transform.position, type);
        }


        pannelTut.SetActiveTut(type, () =>
        {
            pannelTut.SetOffTut();
            if (type != Booster.SHOW_LINE)
            {
                buttonBooster.ClickBtBooster();
            }
            else
            {
                ClickShowLine();
            }


            switch (TutorialController.Instance.idTutCurrent)
            {
                case IDTutorial.BOOSTER_ESCAPSE:
                case IDTutorial.BOOSTER_DESTROY_BRICK:
                case IDTutorial.BOOSTER_DESTROY_BARRIER:

                    popup.DisableBtClose(true);
                    TutorialController.Instance.NextStep();
                    break;

                case IDTutorial.BOOSTER_STOP_TIME:
                    //Done Tut Bosster

                    TutorialController.Instance.SaveTutCompelete(MapController.Instance.levelCurrent);
                    MapController.Instance.SetOffTut();


                    break;
                case IDTutorial.TUT_BOOSTER_FIND:
                    //Done Tut Bosster

                    TutorialController.Instance.SaveTutCompelete(MapController.Instance.levelCurrent);
                    MapController.Instance.SetOffTut();
                    MapController.Instance.mainCamera.isLockCaremaByTut = false;

                    break;
                case IDTutorial.SHOW_LINE_GECKO:
                    TutorialController.Instance.SaveTutCompelete(MapController.Instance.levelCurrent);
                    MapController.Instance.SetOffTut();
                    btShowLine.gameObject.SetActive(true);
                    break;
            }


        });

    }


    #endregion

    #region Remove Ads

    private void SetButton(object param)
    {
        objRemoveAds.gameObject.SetActive(!GameData.userData.shops.isPurchaseRemoveAds);
    }

    #endregion

    #region Hack

    public void HideCanvas()
    {
        if (canvasGroup.alpha == 1)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            canvasGroup.alpha = 1;
        }

    }

    #endregion
}
