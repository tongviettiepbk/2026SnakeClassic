using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LUiUnlockFeature : BaseUI
{
    [Space(20)]
    public GameObject objBrick;
    public GameObject objBarrier;
    public GameObject objHole;
    public GameObject objStop;

    [Space(20)]
    public Button btBack;

    [Space(20)]
    public TMP_Text txtTitle;
    public TMP_Text txtDes;

    public float timeAvailableClose = 1f;
    private bool isAvailableClose = false;

    private string nameFeatureBrick = "Wall Ice";
    private string nameFeatureExit = "Hole Exit";
    private string nameStopPoint = "Stop Point";
    private string nameBarrierPoint = "Barrier Point";

    private string desFeatureBrick = "Ice Walls lose health when the snake successfully moves  and disappear at zero.";
    private string desFeatureExit = "Only a matching-color snake can escape through the Exit Hole.";
    private string desFeatureStopPoint = "The Stop tile lets the Snake pause. Tap again to continue moving.";
    private string desFeatureBarrierPoint = "The Barrier has two states: closed and open.";

    protected override void OnEnable()
    {
        base.OnEnable();

        isAvailableClose = false;
        btBack.gameObject.SetActive(false);
    }

    private void Start()
    {
        btBack.onClick.AddListener(() =>
        {
            if (isAvailableClose)
            {
                TutorialController.Instance.NextStep();
                Close();
            }

        });
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

    public void SetTypeBooster(IDTutorial type)
    {

        switch (type)
        {
            case IDTutorial.TUT_FEATURE_BRICK:

                txtTitle.text = LocalizeManager.Instance.GetLocalizeText(nameFeatureBrick);
                txtDes.text = LocalizeManager.Instance.GetLocalizeText(desFeatureBrick);

                objBrick.SetActive(true);
                timeAvailableClose = 5;

                break;
            case IDTutorial.TUT_FEATURE_HOLE_EXIT:

                txtTitle.text = LocalizeManager.Instance.GetLocalizeText(nameFeatureExit);
                txtDes.text = LocalizeManager.Instance.GetLocalizeText(desFeatureExit);
                objHole.SetActive(true);
                timeAvailableClose = 5;

                break;
            case IDTutorial.TUT_FEATURE_STOP:

                txtTitle.text = LocalizeManager.Instance.GetLocalizeText(nameStopPoint);
                txtDes.text = LocalizeManager.Instance.GetLocalizeText(desFeatureStopPoint);

                objStop.SetActive(true);
                timeAvailableClose = 5;

                break;
            case IDTutorial.TUT_FEATURE_BARRIER:


                txtTitle.text = LocalizeManager.Instance.GetLocalizeText(nameBarrierPoint);
                txtDes.text = LocalizeManager.Instance.GetLocalizeText(desFeatureBarrierPoint);

                objBarrier.SetActive(true);
                timeAvailableClose = 5;

                break;

        }

        OpenContinue();
    }

    public void OpenContinue()
    {
        this.StartDelayAction(timeAvailableClose, () =>
        {
            isAvailableClose = true;

            btBack.transform.localScale = Vector3.zero;
            btBack.gameObject.SetActive(true);

            btBack.transform.DOScale(1, 0.2f);
        });
    }

}
