using UnityEngine;

public class AutoGenMap : MonoBehaviour
{
    public MapGeneratorBasic genMapBasic;
    public MapGrid2dHelper mapGrid2DHelper;

    [Space(20)]
    [Header("Info Map Here")]
    public float rateGeckoInMap = 0.7f;
    public int minLenGecko = 3;
    public int maxLenGecko = 15;

    [Space(20)]
    public string folderPath = "/_Asset/MapData/AutoGen/";
    public string fileName = "AutoGenMap.json";

    [Space(20)]
    [Header("Map Edit Here")]
    public TextAsset mapEdit;

    [Space(20)]
    [Header("Dung Khi Mo Rong Map")]
    public int expandRow;
    public int expandCol;

    private string pathFull;

    private void Start()
    {
        //MapBasic();
        //GenMapWithBaseMap();
        //ExpandMap();

        //EditColorGecko();
        ExpandRowMap();
    }

    private void MapBasic()
    {
        pathFull = Application.dataPath + "/_Asset/MapData/AutoGen/";
        genMapBasic = new MapGeneratorBasic(mapGrid2DHelper);
        var data = genMapBasic.GenerateMapRateGecko(rateGeckoInMap, minLenGecko, maxLenGecko);

        var dataScore = MapDifficultyCalculator.Compute(data);
        data.score = dataScore.finalScore;

        Debug.Log(JsonUtility.ToJson(data, true));
        ExportJson.Export(pathFull, fileName, JsonUtility.ToJson(data, true));
    }

    private void GenMapWithBaseMap()
    {
        var dataBase = JsonUtility.FromJson<MapInfo>(mapEdit.text);

        pathFull = Application.dataPath + "/_Asset/MapData/AutoGen/";
        genMapBasic = new MapGeneratorBasic(mapGrid2DHelper);
        var data = genMapBasic.GenerateRateGeckoWithBaseMap(dataBase, rateGeckoInMap, minLenGecko, maxLenGecko);

        var dataScore = MapDifficultyCalculator.Compute(data);
        data.score = dataScore.finalScore;

        Debug.Log(JsonUtility.ToJson(data, true));
        ExportJson.Export(pathFull, fileName, JsonUtility.ToJson(data, true));
    }

    private void ExpandMap()
    {
        var dataBase = JsonUtility.FromJson<MapInfo>(mapEdit.text);

        pathFull = Application.dataPath + "/_Asset/MapData/AutoGen/";
        genMapBasic = new MapGeneratorBasic(mapGrid2DHelper);

        var data = genMapBasic.ExpandMapInfo4Dir(dataBase, expandRow, expandCol);

        var dataScore = MapDifficultyCalculator.Compute(data);
        data.score = dataScore.finalScore;

        Debug.Log(JsonUtility.ToJson(data, true));
        ExportJson.Export(pathFull, fileName, JsonUtility.ToJson(data, true));
    }

    private void ExpandRowMap()
    {
        var dataBase = JsonUtility.FromJson<MapInfo>(mapEdit.text);

        pathFull = Application.dataPath + "/_Asset/MapData/AutoGen/";
        genMapBasic = new MapGeneratorBasic(mapGrid2DHelper);

        var data = genMapBasic.ExpandMapInfoTop(dataBase, expandRow);

        var dataScore = MapDifficultyCalculator.Compute(data);
        data.score = dataScore.finalScore;

        Debug.Log(JsonUtility.ToJson(data, true));
        ExportJson.Export(pathFull, fileName, JsonUtility.ToJson(data, true));
    }

    private void EditColorGecko()
    {
        var dataBase = JsonUtility.FromJson<MapInfo>(mapEdit.text);

        pathFull = Application.dataPath + "/_Asset/MapData/AutoGen/";
      
        for(int i = 0; i < dataBase.listGeckoInMap.Count; i++)
        {
            var geckoTemp = dataBase.listGeckoInMap[i];
            geckoTemp.colorGecko = Random.Range(0, 7);
        }

        var dataScore = MapDifficultyCalculator.Compute(dataBase);
        dataBase.score = dataScore.finalScore;

        Debug.Log(JsonUtility.ToJson(dataBase, true));
        ExportJson.Export(pathFull, fileName, JsonUtility.ToJson(dataBase, true));
    }
}
