using UnityEngine;

public class SpriteRendererFlashing : MonoBehaviour
{
    public float alphaMin = 0.4f;
    public float alphaMax = 1f;
    public float flashSpeed = 1f;

    private SpriteRenderer spRenderer;

    private void Awake()
    {
        spRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (spRenderer)
        {
            float alpha = Mathf.PingPong(Time.realtimeSinceStartup * flashSpeed, alphaMax - alphaMin) + alphaMin;

            Color c = spRenderer.color;
            c.a = alpha;
            spRenderer.color = c;
        }
    }
}
