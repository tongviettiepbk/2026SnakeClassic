using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;


public class LWin : BaseUI
{
    public GameObject objWin;
    public CellReward cellprefabs;
    public ScrollRect scrollRect;
    public Button btNext;
    public TMP_Text txGoldReceive;
    public TMP_Text txLevel;
    public List<CellReward> cells = new List<CellReward>();

    [Space(20)]
    public MiniGameWin miniGame;
    public TMP_Text txtGoldClaim;
    public TMP_Text txtGoldClaimX2;
    public TMP_Text txtTitelX2;
    public Button btClaimX2;
    public float timeShowBtNext = 2f;

    [Space(20)]
    public ResourceGold resouceGold;

    private int GoldClaim;
    private int multiplyGold = 1;

    private bool isWin;
    private List<RewardData> rewards;

    private int goldClaimFinal;
    private bool isReceivedReward = false;
    private bool isDoingNextLevel = false;
    private bool isReceiveComplete = false;


    protected override void Awake()
    {
        base.Awake();
        btNext.onClick.AddListener(ClickBtNext);
        btClaimX2.onClick.AddListener(ClickClaimX2);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        isReceivedReward = false;
        btNext.gameObject.SetActive(false);

        this.StartDelayAction(timeShowBtNext, () =>
        {
            btNext.gameObject.SetActive(true);
            btNext.transform.localScale = Vector3.zero;
            btNext.transform.DOScale(1, 0.2f);
        });

        ShowBannerCollaps();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        HideBannerColaps();
    }

    private void ShowBannerCollaps()
    {
        if (MediationAds.Instance == null)
            return;

        MediationAds.Instance.HideBannerAd();
        MediationAds.Instance.adMobController.LoadCollapsibleBanner();
    }

    private void HideBannerColaps()
    {
        if (MediationAds.Instance == null)
            return;

        MediationAds.Instance.adMobController.DestroyBannerCollap();
        MediationAds.Instance.ShowBannerAd();
    }

    private void Update()
    {
        UpdataSpawnCoin();
    }

    public void Open(bool isWin, List<RewardData> rewards = null)
    {
        this.rewards = rewards;
        this.isWin = isWin;


        AudioManager.Instance.PlayCoinAppear();

        string format = LocalizeManager.Instance.GetLocalizeText("Level ");
        int level = GameData.userData.profile.currentStageId - 1;
        txLevel.text = string.Format(format + level);

        if (isWin)
        {
            int gold = 0;
            objWin.gameObject.SetActive(true);
            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (reward.type == ItemType.GOLD)
                {
                    gold = (int)reward.quantity;
                    GoldClaim = gold;
                }
            }
            txGoldReceive.text = gold.ToString();

            txtGoldClaimX2.text = (GoldClaim * 2).ToString();
            txtGoldClaim.text = gold.ToString();

            ItemUtils.Receive(ItemType.GOLD, GoldClaim, false, false);
        }
    }

    private void ClickClaimX2()
    {
        AudioManager.Instance.ResumeMusic();

        miniGame.StopGame();

        MediationAds.Instance.ShowRewardedVideoAd(GameConfig.ADS_PLACEMENT_WINX2, (result) =>
        {
            if (result == ShowResult.Finished)
            {
                ReceiveReward(GoldClaim * multiplyGold);
            }
            else if (result == ShowResult.Failed)
            {
                UIManager.Instance.ShowToastMessage(GameConfig.WATCH_VIDEOADS_UNSUCCESS);
            }
            else
            {
                UIManager.Instance.ShowToastMessage(GameConfig.WATCH_VIDEOADS_NEED_FULL);
            }
        });

    }

    private void ClickBtNext()
    {
        AudioManager.Instance.ResumeMusic();

        ReceiveReward(GoldClaim);
    }

    private void ReceiveReward(int goldReciveTemp)
    {
        if (isReceivedReward)
        {
            NextNow();
            return;
        }
            

        txGoldReceive.text = goldReciveTemp.ToString();

        isReceivedReward = true;

        goldClaimFinal = goldReciveTemp - GoldClaim;

        int spawnCoinInput = 10;
        float totalTimeAnimCoin = 10 * timeDelaySpawnCoin + 1f;


        this.StartDelayAction(1f, () =>
        {
            isReceiveComplete = true;
            ItemUtils.Receive(ItemType.GOLD, goldClaimFinal, false, false);
            resouceGold.LoadGold();
        });

        this.StartDelayAction(totalTimeAnimCoin + 0.5f, () =>
        {
            MediationAds.Instance.RequestShowInter(GameConfig.placementInterEndGameWin, () =>
            {
                UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_GAME, () =>
                {
                    UIManager.Instance.ClearAllUI();
                });
            });
        });

        ShowFxCoinWin(spawnCoinInput);
    }

    private void NextNow()
    {
        if(isDoingNextLevel)
            return;

        isDoingNextLevel = true;

        StopAllCoroutines();

        if (!isReceiveComplete)
        {
            isReceiveComplete = true;
            ItemUtils.Receive(ItemType.GOLD, goldClaimFinal, false, false);
            resouceGold.LoadGold();
        }

        MediationAds.Instance.RequestShowInter(GameConfig.placementInterEndGameWin, () =>
        {
            UIManager.Instance.FadeToLoadScene(GameConfig.SCENE_GAME, () =>
            {
                UIManager.Instance.ClearAllUI();
            });
        });
    }

    public void UpdateMuiltiGold(int multi)
    {
        multiplyGold = multi;
        txtGoldClaimX2.text = (GoldClaim * multi).ToString();
        txtTitelX2.text = LocalizeManager.Instance.GetLocalizeText("Claim x") + multi.ToString();
    }

    #region Fx Coin Win


    [Space(20)]
    public GameObject objFxCoinWin;
    public GameObject posEndCoin;
    public GameObject posStartCoin;

    private float timeDelaySpawnCoin = 0.1f;
    private bool isSpawningCoin = false;
    private int quantityCoinSpawn = 0;
    private int quantityCoinSpawnMax = 0;
    private float timeCountSpawn = 0.1f;

    private void UpdataSpawnCoin()
    {
        if (!isSpawningCoin)
            return;

        timeCountSpawn += Time.deltaTime;

        if (timeCountSpawn >= timeDelaySpawnCoin)
        {
            timeCountSpawn = 0f;

            if (quantityCoinSpawn >= quantityCoinSpawnMax)
            {
                isSpawningCoin = false;
            }

            quantityCoinSpawn++;
            var objCoinTemp = Instantiate(objFxCoinWin, this.transform);
            objCoinTemp.transform.position = posStartCoin.transform.position;
            objCoinTemp.gameObject.SetActive(true);

            var coinWinTemp = objCoinTemp.GetComponent<CoinWin>();
            coinWinTemp.MoveToEnd(posEndCoin.transform);
        }

    }

    private void ShowFxCoinWin(int quantityCoinTemp)
    {
        DebugCustom.ShowDebugColorRed("Show Fx Coin Win: " + quantityCoinTemp);

        this.quantityCoinSpawnMax = quantityCoinTemp;
        quantityCoinSpawn = 0;
        isSpawningCoin = true;
        timeCountSpawn = timeDelaySpawnCoin;
    }
    #endregion

}
