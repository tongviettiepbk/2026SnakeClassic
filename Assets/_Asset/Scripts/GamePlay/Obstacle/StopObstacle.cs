using UnityEngine;

public class StopObstacle : Obstacle
{
    private StopDataInMap dataInMap;
    public bool isContentHeadGecko = false;

    public void Init(StopDataInMap dataTemp, MapController mapController)
    {
        this.mapController = mapController;
        this.dataInMap = dataTemp;
        isContentHeadGecko = false;

        this.indexInMap = dataInMap.indexInMap;
        this.value = dataInMap.value;
    }


    public void SetContentGecko(bool isContent)
    {
        this.isContentHeadGecko = isContent;
    }

    public bool IsContentHeadGecko()
    {
        return isContentHeadGecko;
    }
}
