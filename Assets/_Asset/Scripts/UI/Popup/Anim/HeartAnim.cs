using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HeartAnim : MonoBehaviour
{
    public Image img;
    private Tween tween;

    public void PlayAnim()
    {
        if (img == null)
        {
            return;
        }

        Color c = img.color;
        c.a = 1f;
        img.color = c;

        tween?.Kill();

        tween = img
            .DOFade(0f, 0.15f)
            .SetLoops(5, LoopType.Yoyo)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
