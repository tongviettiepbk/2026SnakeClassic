using DG.Tweening;
using System.Collections.Generic;
using System.Security.Claims;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LUiUnlockBooster : BaseUI
{
    [Space(20)]
    public GameObject objBoosterEscapse;
    public GameObject objBoosterBarrier;
    public GameObject objBoosterBrick;
    public GameObject objShowDirection;

    [Space(20)]
    public Button btClaim;

    [Space(20)]
    public TMP_Text txtNameBooster;
    public TMP_Text txtDescription;
    public Image icon;

    private bool isAvailableClose = false;

    public GameObject objIcon;

    private float timeAvailableClose = 3f;

    protected override void OnEnable()
    {
        base.OnEnable();
        btClaim.gameObject.SetActive(false);

        isAvailableClose = false;

    }


    private void Start()
    {
        btClaim.onClick.AddListener(ClickBtClaim);
    }

    private void ClickBtClaim()
    {
        DebugCustom.ShowDebugColorRed("Click BT Claim");
        if (isAvailableClose)
        {
            TutorialController.Instance.NextStep();
            Close();
        }
    }

    public override void Close()
    {
        AudioManager.Instance.ResumeMusic();
        if (this != null && this.gameObject != null)
        {

            if (isLoadFromResources)
            {
                UIManager.Instance.HideUI(this);
            }

            gameObject.SetActive(false);
            EndClose();

        }
    }

    public void SetTypeBooster(Booster type)
    {
        var data = GetData(type);

        txtNameBooster.text = data.GetName();
        txtDescription.text = data.GetDesTut();
        icon.sprite = data.iconTut;

        objIcon.SetActive(false);

        switch (type)
        {
            case Booster.BARRIER_DESTROY:
                objBoosterBarrier.SetActive(true);
                timeAvailableClose = 3f;
                break;
            case Booster.BRICK_DESTROY:

                objBoosterBrick.SetActive(true);
                timeAvailableClose = 4f;
                break;
            case Booster.ESCAPES:

                objBoosterEscapse.SetActive(true);
                timeAvailableClose = 3f;
                break;
            case Booster.FIND_GECKO_MOVE:
                objIcon.SetActive(true);

                timeAvailableClose = 1f;
                break;
            case Booster.TIMESTOP:
                objIcon.SetActive(true);

                timeAvailableClose = 1f;
                break;
            case Booster.SHOW_LINE:
                objShowDirection.SetActive(true);

                timeAvailableClose = 1f;
                break;
        }

        ActiveBtClaim();
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
            case Booster.SHOW_LINE:
                typeConvert = (int)ItemType.SHOW_LINE;
                break;
        }


        return GameData.staticData.items.GetData(typeConvert);
    }

    private void ActiveBtClaim()
    {
        this.StartDelayAction(timeAvailableClose, () =>
        {
            isAvailableClose = true;
            btClaim.transform.localScale = Vector3.zero;
            btClaim.gameObject.SetActive(true);
            btClaim.transform.DOScale(1, 0.2f);
        });
    }
}
