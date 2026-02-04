using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class LShop : BaseUI
{
    public Button btClose;

    protected override void Awake()
    {
        btClose.onClick.AddListener(Back);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        //Time.timeScale = 0;

        SimpleEventManager.Instance.PostEvent(EventIDSimple.pauseGameUI);
    }

    protected override void OnDisable()
    {
        //Time.timeScale = 1;
        SimpleEventManager.Instance.PostEvent(EventIDSimple.unPauseGameUI);
    }

    public override void Back()
    {
        base.Back();
        if (LGamePlay.Instance != null)
            LGamePlay.Instance.ReLoadFooter();
    }
}
