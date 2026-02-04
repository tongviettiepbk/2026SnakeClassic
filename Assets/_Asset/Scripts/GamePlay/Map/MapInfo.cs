using System.Collections.Generic;


[System.Serializable]
public class MapInfo 
{
    public float score;
    public int timeLimit;
    public int typeMap;

    public int column;
    public int row;
    public List<GeckoDataInMap> listGeckoInMap;
    public List<BrickDataInMap> listBrickInMap;
    public List<ExitHoleDataInMap> listExitsInMap;
    public List<StopDataInMap> listStopInMap;
    public List<BarrierDataInMap> listBarrierInMap;
}

[System.Serializable]
public class GeckoDataInMap
{
    public int colorGecko;
    public List<int> listNode;
    public int indexHead;
}

[System.Serializable]
public class BrickDataInMap
{
    public int indexInMap;
    public int hp;
}

[System.Serializable]
public class ExitHoleDataInMap
{
    public int indexInMap;
    public int color;
}

[System.Serializable]
public class StopDataInMap
{
    public int indexInMap;
    public int value;
}

[System.Serializable]
public class BarrierDataInMap
{
    public int indexInMap;
    public int isOpen;
}