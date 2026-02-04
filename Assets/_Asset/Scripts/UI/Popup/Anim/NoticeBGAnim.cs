using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class NoticeBGAnim : MonoBehaviour
{
    public Image bg;
    public float totalDuration = 1f;
    public Ease ease;

    private Tween tweenBg;

    private void OnEnable()
    {
        if (bg == null) return;

        // Set alpha = 0
        Color c = bg.color;
        c.a = 0f;
        bg.color = c;
        tweenBg?.Kill();

        float step = totalDuration / 4f;

        Sequence seq = DOTween.Sequence();
        seq.Append(bg.DOFade(255f / 255f, step).SetEase(ease))
           .Append(bg.DOFade(0f, step).SetEase(ease))
           .Append(bg.DOFade(255f / 255f, step).SetEase(ease))
           .Append(bg.DOFade(0f, step).SetEase(ease))
           .OnComplete(() =>
           {
               gameObject.SetActive(false);
           });

        tweenBg = seq;
    }
}
