using UnityEngine;

public class HandHummer : HandClock
{
    protected override void OnEnable()
    {
        objFxIceStun.gameObject.SetActive(false);
        this.StartDelayAction(timeActiveFx, () =>
        {
            objFxIceStun.gameObject.SetActive(true);
            AudioManager.Instance.PlayHummerCollision();
        });

        AudioManager.Instance.PlayHummerRotation();
    }

    public override void DoneAnim()
    {
        base.DoneAnim();
        FxController.Instance.DoneAnimHammer();

        this.StartDelayAction(0.5f, () =>
        {
            gameObject.SetActive(false);
        });
    }
}
