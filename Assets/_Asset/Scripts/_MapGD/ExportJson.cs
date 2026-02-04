using UnityEngine;

public static class ExportJson
{
    public static string linkExport = Application.dataPath;
    public static string linkImport = Application.dataPath;

    public static void Export(string folderPath, string fileName, string json)
    {
        // Tạo thư mục nếu chưa có
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        // Đường dẫn file
        string filePath = folderPath + fileName;

        // Ghi file
        System.IO.File.WriteAllText(filePath, json);

        Debug.Log("JSON saved to: " + filePath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static string Import(string path)
    {
        if (System.IO.File.Exists(path))
        {
            string jsonData = System.IO.File.ReadAllText(path);
            Debug.Log("Imported JSON from: " + jsonData);
            return jsonData;
        }
        else
        {
            Debug.LogError("File not found: " + path);
            return null;
        }
    }
}
