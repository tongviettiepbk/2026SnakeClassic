using DG.Tweening;
using UnityEngine;

public class DotItem : MonoBehaviour
{
    public GameObject dot;
    public SpriteRenderer spriteRender;

    public Color colorNormal;
    public Color colorMove;

    [Space(20)]
    public float sizeApearStartAnim = 0f;
    //public float sizeApearEndAnim = 1.1f;
    public float timeStartAnim = 0.1f;

    [Space(20)]
    public float dotSizeMove = 1.5f;
    public float dotSpeed = 0.25f;
    public float dotDelay = 0.05f;

    public int index;

    [Space(20)]
    public Animator animatorDot;
    public string animIdle = "Idle";
    public string animMove = "Move";

    public void Init(int index)
    {
        this.index = index;
        dot.transform.localScale = Vector3.one;
        spriteRender.color = colorNormal;
    }

    public void PlayAnimWin(float delay)
    {
        //dot.transform.DOKill();
        //spriteRender.DOKill();

        //var targetSize = new Vector3(dotSizeMove, dotSizeMove, dotSizeMove);

        //var speed = dotSpeed * 0.75f;
        //dot.transform.DOScale(targetSize, speed).OnComplete(() =>
        //{
        //    dot.transform.DOScale(Vector3.one, speed);
        //}).SetDelay(delay);
        //spriteRender.DOColor(colorMove, speed).OnComplete(() =>
        //{
        //    spriteRender.DOColor(colorNormal, speed);
        //}).SetDelay(delay);

        MapController.Instance.StartDelayAction(delay, () =>
        {
            PlayAnim();
        });
    }

    public void PlayAnim()
    {
        //dot.transform.DOKill();
        //spriteRender.DOKill();

        //dot.transform.localScale = new Vector3(sizeApearStartAnim, sizeApearStartAnim, sizeApearStartAnim);

        //spriteRender.color = colorMove;

        //dot.transform.DOScale(new Vector3(dotSizeMove, dotSizeMove, dotSizeMove), timeStartAnim).OnComplete(() =>
        //{
        //    dot.transform.DOScale(Vector3.one, dotSpeed).SetDelay(dotDelay);
        //    spriteRender.DOColor(colorNormal, dotSpeed).SetDelay(dotDelay);
        //});

        //dot.transform.localScale = new Vector3(dotSizeMove, dotSizeMove, dotSizeMove);

        animatorDot.SetTrigger(animMove);
       
    }
}
