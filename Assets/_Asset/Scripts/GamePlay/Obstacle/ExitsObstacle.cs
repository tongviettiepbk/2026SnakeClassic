using UnityEngine;

public class ExitsObstacle : Obstacle
{
    public ParticleSystem exitEffect;

    private ExitHoleDataInMap dataInMap;

    private void OnEnable()
    {
        if (exitEffect != null)
        {
            exitEffect.Stop();
        }
    }

    public void Init(ExitHoleDataInMap dataTemp, MapController mapController)
    {
        this.mapController = mapController;
        this.dataInMap = dataTemp;

        this.indexInMap = dataInMap.indexInMap;
        this.value = dataInMap.color;

    }

    public void OpenFx(int lenghth)
    {
        if(exitEffect == null)
        {
            return;
        }

        exitEffect.Play();
        this.StartDelayAction(lenghth * 0.035f, () =>
        {
            exitEffect.Stop();
        });
    }
}
