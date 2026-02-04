using UnityEngine;

public class PingPongAlpha : MonoBehaviour
{
    public float alphaMin = 0.4f;
    public float alphaMax = 1f;
    public float flashSpeed = 1f;

    private bool isFlashingToMin;
    private float alpha;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        // Set initial color
        alpha = canvasGroup.alpha;

        // Flashing
        if (isFlashingToMin)
        {
            alpha = Mathf.MoveTowards(alpha, alphaMin, flashSpeed * Time.deltaTime);

            if (alpha <= alphaMin)
            {
                alpha = alphaMin;
                isFlashingToMin = false;
            }
        }
        else
        {
            alpha = Mathf.MoveTowards(alpha, alphaMax, flashSpeed * Time.deltaTime);

            if (alpha >= alphaMax)
            {
                alpha = alphaMax;
                isFlashingToMin = true;
            }
        }

        canvasGroup.alpha = alpha;
    }
}
