using DG.Tweening;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum LobbyViewType
{
    HOME,
    ACHIEVEMENT,
    SHOP,
    NONE,
}

public class LobbyManager : Singleton<LobbyManager>
{
    public Transform groupViews;
    public LobbyView[] viewPrefabs;
    public GameObject objFooter;

    [Header("View Objects ")]
    public RectTransform viewHome;
    public RectTransform viewShop;
    public Canvas canvas;
    public GraphicRaycaster raycaster;
    private bool dragging = false;
    private float startDragX;
    private float startHomeX;
    private float startShopX;

    [Header("Footer Buttons")]
    public FooterButton[] footerButtons;
    public GameObject objHeaderHome;
    public LobbyViewType curViewType = LobbyViewType.NONE;

    public bool isIgnoreSwipe = false;

    private float screenWidth;

    private List<LobbyView> views = new List<LobbyView>();

    protected void Awake()
    {
        //EventDispatcher.Instance.RegisterListener(EventID.TIMER, CheckNotify);
        //AudioManager.Instance.PlayMusicBg();

        screenWidth = ((RectTransform)transform).rect.width;
        LayoutViews();
    }

    private void OnEnable()
    {
        //AudioManager.Instance.PlayMusic(AudioTag.BGM_LOBBY);
        ShowView(LobbyViewType.HOME, true);
        SimpleEventManager.Instance.Register(EventIDSimple.showBannerSucess, SetBanner);
        SetPosYFooter();

       
    }

    private void Start()
    {
        MediationAds.Instance.HideMRec();
        MediationAds.Instance.ShowBannerAd();
        isIgnoreSwipe = false;
    }

    private void OnDisable()
    {
        SimpleEventManager.Instance.Unregister(EventIDSimple.showBannerSucess, SetBanner);
    }

    private void Update()
    {
        Cheat();

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (UIManager.Instance.Back())
            {
                return;
            }
        }

        if(isIgnoreSwipe == false)
        {
            DetectDrag();
        }
        
    }

    private void LayoutViews()
    {
        viewHome.anchoredPosition = new Vector2(0, 0);
        viewShop.anchoredPosition = new Vector2(-screenWidth, 0);
    }

    private void SetBanner(object param)
    {
        GameConfig.isAdsOn = true;
    }

    private void SetPosYFooter()
    {
        if (GameConfig.isAdsOn)
        {
            RectTransform rt = objFooter.GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;
            pos.y += GameConfig.posYBanner;
            rt.anchoredPosition = pos;
        }
    }

    public void ShowView(LobbyViewType type, bool instant = false)
    {

        if (curViewType != type)
        {
            curViewType = type;
            HighlightFooter(true);
        }
        else
        {
            curViewType = type;
            HighlightFooter(false);
        }



        float targetOffsetX = 0f;

        if (type == LobbyViewType.HOME)
        {
            targetOffsetX = 0f;
        }
        else if (type == LobbyViewType.SHOP)
        {
            targetOffsetX = screenWidth;
        }

        if (instant)
        {
            MoveInstant(targetOffsetX);
        }
        else
        {
            SlideTo(targetOffsetX);
        }



    }

    private void HighlightFooter(bool isPlayAnim)
    {
        for (int i = 0; i < footerButtons.Length; i++)
        {
            var button = footerButtons[i];
            button.CheckStatus(isPlayAnim);
        }
    }

    public LobbyView GetView(LobbyViewType type)
    {
        for (int i = 0; i < views.Count; i++)
        {
            if (views[i].type == type)
            {
                return views[i];
            }
        }

        return null;
    }


    #region SwipeViews

    private void DetectDrag()
    {
        if (IsClickOnUIBlock())
        {
            return;
        }

        // Bắt đầu kéo
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            startDragX = Input.mousePosition.x;

            startHomeX = viewHome.anchoredPosition.x;
            startShopX = viewShop.anchoredPosition.x;
        }

        // Đang kéo → view chạy theo tay
        if (dragging && Input.GetMouseButton(0))
        {
            float delta = Input.mousePosition.x - startDragX;

            float newHomeX = startHomeX + delta;
            float newShopX = startShopX + delta;

            // Clamp đúng theo hướng kéo của bạn:
            newHomeX = Mathf.Clamp(newHomeX, 0, screenWidth);
            newShopX = Mathf.Clamp(newShopX, -screenWidth, 0);

            viewHome.anchoredPosition = new Vector2(newHomeX, 0);
            viewShop.anchoredPosition = new Vector2(newShopX, 0);
        }

        // snap
        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;

            float homeX = viewHome.anchoredPosition.x;

            if (curViewType == LobbyViewType.HOME)
            {
                // target HOME.x = 0
                if (homeX > screenWidth * 0.1f)
                {
                    ShowView(LobbyViewType.SHOP);
                }
                else
                {
                    ShowView(LobbyViewType.HOME);
                }
            }
            else if (curViewType == LobbyViewType.SHOP)
            {
                // target SHOP = screenWidth
                if (homeX < screenWidth * 0.9f)
                {
                    ShowView(LobbyViewType.HOME);
                }
                else
                {
                    ShowView(LobbyViewType.SHOP);
                }
            }
        }
    }

    private void MoveInstant(float offset)
    {
        viewHome.anchoredPosition = new Vector2(offset, 0);
        viewShop.anchoredPosition = new Vector2(offset - screenWidth, 0);
    }

    private void SlideTo(float offset)
    {
        float duration = 0.25f;

        viewHome.DOAnchorPosX(offset, duration).SetEase(Ease.OutCubic);
        viewShop.DOAnchorPosX(offset - screenWidth, duration).SetEase(Ease.OutCubic);
    }

    private bool IsClickOnUIBlock()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            GameObject obj = results[i].gameObject;

            if (obj == null)
            {
                continue;
            }

            if (obj.transform.IsChildOf(objFooter.transform))
            {
                return true;
            }

            if (objHeaderHome != null && obj.transform.IsChildOf(objHeaderHome.transform))
            {
                return true;
            }
        }

        return false;
    }

    #endregion
    private void Cheat()
    {
#if UNITY_EDITOR

#endif
    }
}
