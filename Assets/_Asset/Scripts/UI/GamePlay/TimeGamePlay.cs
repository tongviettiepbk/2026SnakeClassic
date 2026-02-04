using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;

public class TimeGamePlay : MonoBehaviour
{
    public TMP_Text txTime;

    public float timeScaleChangeColor = 0.3f;

    public Animator animatorTime;

    private Tween tweenBlink;

    private bool isWarning = false;

    private void Start()
    {
        animatorTime.enabled = false;
    }

    public void PlayAnim()
    {
        if (isWarning)
        {
            return;
        }

        animatorTime.enabled = true;

        isWarning = true;

        AudioManager.Instance.PlayWarningTime();

        if (tweenBlink == null)
        {
            tweenBlink = txTime.DOColor(Color.red, timeScaleChangeColor)
                .SetLoops(-1, LoopType.Yoyo);
        }

    }

    public void StopAnim()
    {
        AudioManager.Instance.StopWarningTime();

        animatorTime.enabled = false;

        isWarning = false;

        if (tweenBlink != null)
        {
            tweenBlink.Kill();
            tweenBlink = null;
        }

        txTime.color = Color.white;
    }

    public void SetTextTime(int Time)
    {
        if (Time == 0)
        {
            txTime.text = "00:00";
        }
        else
        {
            int minutes = Time / 60;
            int seconds = Time % 60;
            txTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

}
