using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;


public class EncryptMap
{
    private static readonly string JSON_PATH  = "Assets/_Asset/MapData/MapFinal";
    private static readonly string BYTES_PATH = "Assets/_Asset/Resources/MapBytes";

    [MenuItem("Tools/Encrypt Map (Test)")]
    public static void TestEncrypt()
    {
        if (!Directory.Exists(BYTES_PATH))
            Directory.CreateDirectory(BYTES_PATH);

        foreach (var file in Directory.GetFiles(JSON_PATH, "*.json"))
        {
            string json = File.ReadAllText(file);
            string encrypted = AESUtil.EncryptAES(json);

            string fileName = Path.GetFileNameWithoutExtension(file);
            File.WriteAllText($"{BYTES_PATH}/{fileName}.json.bytes", encrypted);
        }

        AssetDatabase.Refresh();
        Debug.Log("🔐 AES encrypted map files generated!");
    }
}
