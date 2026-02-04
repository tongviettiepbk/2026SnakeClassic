using DG.Tweening;
using UnityEngine;

public class HeartBreakFx : BaseFx
{
    public GameObject objHeart;
    public float timeHide = 1f;

    protected override void OnEnable()
    {
        base.OnEnable();
        //var pos = objHeart.transform.position;

        //objHeart.transform.DOMoveY(pos.y + 4f, timeHide).OnComplete(() =>
        //{
        //    objHeart.SetActive(false);
        //});

        this.StartDelayAction(timeHide, () =>
        {
            objHeart.SetActive(false);
        });
    }
}
