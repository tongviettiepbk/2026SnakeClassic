using DG.Tweening;
using UnityEngine;

public class CoinWin : MonoBehaviour
{
    private float timeDelayAction = 0.5f;
    private float timeMove = 0.5f;
    private float timeShowFx = 1f;
    public void MoveToEnd(Transform posEnd)
    {
        this.StartDelayAction(timeDelayAction, () =>
        {
            this.transform.DOMove(posEnd.position, timeMove).OnComplete(() =>
            {
                AudioManager.Instance.PlayCoinCount();

                AudioManager.Instance.StartDelayAction(timeShowFx, () =>
                {
                    try
                    {
                        Destroy(this.gameObject);
                    }
                    catch
                    {

                    }
                    
                });
            });
        });

    }
}
