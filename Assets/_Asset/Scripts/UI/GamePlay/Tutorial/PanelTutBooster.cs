using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelTutBooster : MonoBehaviour
{
    public GameObject objShowTut;
    public Button btClick;

    [Space(20)]
    public GameObject objShowTutShowLine;
    public Button btClickShowLine;

    [Space(20)]
    public GameObject objBgBlack;
    public GameObject objBgWhite;
    public Image imgIconBooster;
    public GameObject objParentIconBooster;
    public GameObject objHand;
    public GameObject objFxTutStart;
    public GameObject objFxTutClick;

    [Space(20)]
    public Image iconButton;
    public Sprite iconLock;

    private float timeShowAlpha = 0.4f;

    private Action actionNext;
    private Booster type;
    private ItemData dataBooster;

    public void Start()
    {
        btClick.onClick.AddListener(() =>
        {
            ClickBtTut();
        });

        btClickShowLine.onClick.AddListener(() =>
        {
            ClickBtTut();
        });
    }

    private void OnEnable()
    {

    }

    private void ClickBtTut()
    {
        if (actionNext != null)
        {
            actionNext.Invoke();
        }
    }

    public void SetPosTut(Vector3 posTemp, Booster type)
    {
        objShowTutShowLine.SetActive(false);
        objShowTut.SetActive(false);
        objShowTut.transform.position = posTemp;
        objShowTutShowLine.transform.position = posTemp;

        if (type == Booster.SHOW_LINE)
        {
            
        }
        else
        {
            objShowTut.SetActive(true);
        }
    }

    public void SetActiveTut(Booster type, Action actionTemp)
    {
        this.type = type;
        this.actionNext = actionTemp;
        this.gameObject.SetActive(true);

        dataBooster = GetData(type);
        imgIconBooster.sprite = dataBooster.iconTut;

        StartTut();
    }

    private void StartTut()
    {
        iconButton.sprite = iconLock;
        objHand.gameObject.SetActive(false);
        objBgBlack.gameObject.SetActive(false);
        objBgWhite.gameObject.SetActive(true);
        objFxTutStart.gameObject.SetActive(true);


        this.StartDelayAction(1f, () =>
        {
            objParentIconBooster.transform.DOMove(objShowTut.transform.position, 0.35f);

            this.StartDelayAction(0.35f, () =>
            {
                ChoseBooster();
            });
        });
    }

    private void ChoseBooster()
    {
        objBgBlack.gameObject.SetActive(true);
        objBgBlack.GetComponent<Image>().DOFade(0.95f, 0.3f);

        objHand.gameObject.SetActive(true);
        objParentIconBooster.gameObject.SetActive(false);

        objFxTutClick.transform.position = objShowTut.transform.position;
        objFxTutClick.gameObject.SetActive(true);
        iconButton.sprite = dataBooster.icon;

        if(type == Booster.SHOW_LINE)
        {
            objShowTutShowLine.SetActive(true);
        }
        
    }

    public void SetOffTut()
    {
        this.actionNext = null;
        this.gameObject.SetActive(false);
    }

    private ItemData GetData(Booster type)
    {
        int typeConvert = (int)type;
        switch (type)
        {
            case Booster.BARRIER_DESTROY:
                typeConvert = (int)ItemType.BARRIER;
                break;
            case Booster.BRICK_DESTROY:
                typeConvert = (int)ItemType.BRICK_BROKEN;
                break;
            case Booster.ESCAPES:
                typeConvert = (int)ItemType.ESCAPE_GECKO;
                break;
            case Booster.TIMESTOP:
                typeConvert = (int)ItemType.TIME_STOP_ICE;
                break;
            case Booster.FIND_GECKO_MOVE:
                typeConvert = (int)ItemType.FIND_GECKO_MOVE;
                break;
        }


        return GameData.staticData.items.GetData(typeConvert);
    }
}
