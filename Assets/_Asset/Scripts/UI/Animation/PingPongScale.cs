using UnityEngine;

public class PingPongScale : MonoBehaviour
{
    public bool isAutoAnimating;
    public bool isLoop;
    public float scaleSpeed = 3f;
    public float scaleRate = 1.3f;

    private RectTransform rectTransform;
    private bool isIncrease;
    private bool isNormalize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (isAutoAnimating)
        {
            Normalize();
            PlayAnimation(true);
        }
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        if (isIncrease)
        {
            rectTransform.localScale = Vector3.MoveTowards(rectTransform.localScale, Vector3.one * scaleRate, scaleSpeed * Time.deltaTime);

            if (rectTransform.localScale.x >= scaleRate)
            {
                isIncrease = false;
                isNormalize = true;
            }
        }
        else if (isNormalize)
        {
            rectTransform.localScale = Vector3.MoveTowards(rectTransform.localScale, Vector3.one, scaleSpeed * Time.deltaTime);

            if (rectTransform.localScale.x <= 1f)
            {
                if (isLoop)
                {
                    isNormalize = false;
                    isIncrease = true;
                }
                else
                {
                    Normalize();
                }
            }
        }
    }

    private void Normalize()
    {
        rectTransform.localScale = Vector3.one;
        isIncrease = false;
        isNormalize = false;
    }

    public void PlayAnimation(bool isLoop = false)
    {
        Normalize();
        this.isLoop = isLoop;
        isIncrease = true;
    }
}
