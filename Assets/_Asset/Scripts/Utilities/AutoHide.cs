using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float timeHide = 2f;
    private void OnEnable()
    {
        this.StartDelayAction(timeHide, () => { this.gameObject.SetActive(false); });
    }
}
