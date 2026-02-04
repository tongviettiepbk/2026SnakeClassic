using UnityEngine;

public class LightIngame : MonoBehaviour
{
    public Light light;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameConfig.isLowGraphic)
        {
            light.shadows = LightShadows.None;
        }
        else
        {
            light.shadows = LightShadows.Soft;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
