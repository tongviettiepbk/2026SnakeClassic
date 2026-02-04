using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LevelDataArrow
{
    public int XSize;
    public int YSize;
    public ArrowData[] Arrows;
}

[System.Serializable]
public class ArrowData
{
    public int Dx;
    public int Dy;
    public int X;
    public int Y;
    public int[] Indices;
    public int BendCount;
    public int ColorIdx;
}

public class ConvertMapGD : MonoBehaviour
{

    private void Start()
    {

        
    }

    #region Connvert Map

    private void Convert()
    {
        //for (int i = 40; i < 3000; i++)
        //{
        //    string level = i.ToString();
        //    ConvertAndSave(level);
        //}

        //OpenAllFileInFolder();
    }

    public void ConvertAndSave(string levelId)
    {
        string folderPath = $"{Application.dataPath}/_Asset/MapData/LevelThief/" + "level_" + levelId + ".bytes";
        string jsonData = System.IO.File.ReadAllText(folderPath);

        DebugCustom.ShowDebug("map convert", jsonData);

        ConvertAndSave(levelId, jsonData);

        //LevelDataArrow level = JsonUtility.FromJson<LevelDataArrow>(jsonData);

        //MapInfo map = ConvertToGeckoMap(level);

        //string dataTemp = JsonConvert.SerializeObject(map, Formatting.Indented);

        //SaveJsonToMapGDDoing(levelId, dataTemp);
    }

    public void ConvertAndSave(string levelId, string jsonData)
    {
        LevelDataArrow dataClass = JsonUtility.FromJson<LevelDataArrow>(jsonData);

        MapInfo map = ConvertToGeckoMap(dataClass);

        string dataTemp = JsonConvert.SerializeObject(map, Formatting.Indented);

        SaveJsonToMapGDDoing(levelId, dataTemp);
    }

    public MapInfo ConvertToGeckoMap(LevelDataArrow level)
    {
        MapInfo map = new MapInfo();
        map.column = level.XSize;
        map.row = level.YSize;

        map.listGeckoInMap = new List<GeckoDataInMap>();
        map.listBrickInMap = new List<BrickDataInMap>();
        map.listExitsInMap = new List<ExitHoleDataInMap>();
        map.listStopInMap = new List<StopDataInMap>();
        map.listBarrierInMap = new List<BarrierDataInMap>();

        foreach (var a in level.Arrows)
        {
            if (a.Indices == null || a.Indices.Length == 0)
                continue;

            GeckoDataInMap g = new GeckoDataInMap();
            g.colorGecko = UnityEngine.Random.Range(0, 7);
            g.listNode = new List<int>(a.Indices);
            g.indexHead = a.Indices[0];

            map.listGeckoInMap.Add(g);
        }




        return map;
    }

    public static void SaveJsonToMapGDDoing(string fileName, string json)
    {
        DebugCustom.ShowDebug("Json new", json);
        // Thư mục bạn muốn lưu
        string folderPath = Application.dataPath + "/_Asset/MapData/MapThiefConvert2/";

        // Tạo thư mục nếu chưa có
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        // Đường dẫn file
        string filePath = folderPath + fileName + ".json";

        // Ghi file
        System.IO.File.WriteAllText(filePath, json);

        Debug.Log("JSON saved to: " + filePath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public void OpenAllFileInFolder()
    {
        string sourcePath = "Assets/_Asset/MapData/LevelThief2";
        string[] files = Directory.GetFiles(sourcePath, "*.bytes", SearchOption.AllDirectories);

        string saveFolder = Application.dataPath + "/ConvertedLevels";
        if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

        int count = 0;

        foreach (string filePath in files)
        {
            string json = File.ReadAllText(filePath);

            ConvertAndSave(count.ToString(), json);
            count++;
        }

        Debug.Log($"✔ Converted {count} Thief levels → MapInfo JSON");
    }

    #endregion

}

