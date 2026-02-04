using Newtonsoft.Json;
using UnityEngine;

public class CaculateScore : MonoBehaviour
{
    public TextAsset mapJson;

    private string pathRead = Application.dataPath + "/_Asset/MapData/MapGDDoing/Tuan/";
    private string pathSave = Application.dataPath + "/_Asset/MapData/MapGDDoing/TuanScore/";

    private void Start()
    {

        for (int i = 0; i <13; i++)
        {
            //string levelId = i.ToString("D2") + ".json";
            string levelId = i.ToString() + ".json";
            string dataTemp = ExportJson.Import(pathRead + levelId);

            MapInfo mapInfoTemp = new MapInfo();
            mapInfoTemp = JsonConvert.DeserializeObject<MapInfo>(dataTemp);

            //for(int j= 0; j < mapInfoTemp.listGeckoInMap.Count; j++)
            //{
            //    mapInfoTemp.listGeckoInMap[j].colorGecko = Random.Range(0, 7);
            //}

            var dataScore = MapDifficultyCalculator.Compute(mapInfoTemp);
            mapInfoTemp.score = dataScore.finalScore;

            var dataUpdate = JsonConvert.SerializeObject(mapInfoTemp, Formatting.Indented);

            ExportJson.Export(pathSave, levelId, dataUpdate);
        }
    }
}
