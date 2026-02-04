using UnityEngine;

public class HandClock : MonoBehaviour
{
    public Animator aniController;
    public GameObject objFxIceStun;
    public float timeActiveFx = 1.5f;

    protected virtual void OnEnable()
    {
        objFxIceStun.gameObject.SetActive(false);
        this.StartDelayAction(timeActiveFx, () =>
        {
            objFxIceStun.gameObject.SetActive(true);
            AudioManager.Instance.PlayFreezeExploder();
        });

        AudioManager.Instance.PlayFreezeStart();
    }

    public void Init(FxController fxController)
    {

    }


    public virtual void DoneAnim()
    {
        FxController.Instance.DoneAnimClock();
    }

}
