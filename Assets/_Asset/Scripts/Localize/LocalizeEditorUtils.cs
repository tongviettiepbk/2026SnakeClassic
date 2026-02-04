#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System;

public class LocalizeEditorUtils : Editor
{
    private const string PATH_ENGLISH_JSON_FILE = "Json/Localize/English";

    //[MenuItem("Editor Utils/Localize/Scan Texts In Scene")]
    //public static void ScanTextInScene()
    //{
    //    TextAsset textAsset = Resources.Load<TextAsset>(PATH_ENGLISH_JSON_FILE);
    //    Dictionary<string, string> curDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);

    //    Text[] texts = Resources.FindObjectsOfTypeAll<Text>() as Text[];
    //    for (int i = 0; i < texts.Length; i++)
    //    {
    //        Text tx = texts[i];

    //        if (tx.hideFlags == HideFlags.NotEditable || tx.hideFlags == HideFlags.HideAndDontSave)
    //        {
    //            continue;
    //        }

    //        if (EditorUtility.IsPersistent(tx.gameObject))
    //        {
    //            continue;
    //        }

    //        if (string.IsNullOrEmpty(tx.text))
    //        {
    //            continue;
    //        }

    //        int number = 0;
    //        if (int.TryParse(tx.text, System.Globalization.NumberStyles.AllowThousands, null, out number))
    //        {
    //            continue;
    //        }

    //        if (curDictionary.ContainsKey(tx.text) == false)
    //        {
    //            DebugCustom.Log("NewKey=" + tx.text);
    //            curDictionary.Add(tx.text, string.Empty);
    //        }

    //        LocalizeText localizeComponent = tx.GetComponent<LocalizeText>();
    //        if (localizeComponent == null)
    //        {
    //            tx.gameObject.AddComponent<LocalizeText>();
    //        }
    //    }

    //    WriteLanguageFile(JsonConvert.SerializeObject(curDictionary));
    //    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    //}

    //[MenuItem("Editor Utils/Localize/Validate Localize Files")]
    [MenuItem("Editor Utils/Localize/Đồng bộ các file ngôn ngữ")]
    public static void ValidateLocalizeFiles()
    {
        string path = "Json/Localize/";
        TextAsset[] files = Resources.LoadAll<TextAsset>(path);

        TextAsset jsonEnglish = Resources.Load<TextAsset>(path + "0-English");
        Dictionary<string, string> dictEnglish = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonEnglish.text);

        TextAsset[] additionalFiles = Resources.LoadAll<TextAsset>("Json/LocalizeMiniFiles/");
        for (int i = 0; i < additionalFiles.Length; i++)
        {
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(additionalFiles[i].text);
            var e = dict.GetEnumerator();
            while (e.MoveNext())
            {
                string key = e.Current.Key;
                string value = e.Current.Value;

                if (key.StartsWith("/*"))
                {
                    continue;
                }

                //if (dictEnglish.ContainsKey(key))
                //{
                //    DebugCustom.LogError("Exists key=" + key);
                //}

                dictEnglish[key] = value;
            }
        }

        for (int i = 0; i < files.Length; i++)
        {
            try
            {
                string[] splits = files[i].name.Split('-');
                string id = splits[0];
                string fileName = splits[1];

                if (fileName == "English")
                {
                    continue;
                }

                DebugCustom.Log("Validate " + fileName);
                Dictionary<string, string> dictLocalize = JsonConvert.DeserializeObject<Dictionary<string, string>>(files[i].text);
                Dictionary<string, string> verifyDict = new Dictionary<string, string>();
                List<string> emptyKeys = new List<string>();

                var e = dictEnglish.GetEnumerator();
                while (e.MoveNext())
                {
                    string key = e.Current.Key;

                    if (dictLocalize.ContainsKey(key))
                    {
                        string value = dictLocalize[key];

                        if (string.IsNullOrEmpty(key) == false && string.IsNullOrEmpty(value))
                        {
                            emptyKeys.Add(key);
                        }
                        else
                        {
                            verifyDict[key] = dictLocalize[key];
                        }
                    }
                    else
                    {
                        emptyKeys.Add(key);
                    }
                }

                for (int j = 0; j < emptyKeys.Count; j++)
                {
                    verifyDict[emptyKeys[j]] = "";
                }

                string path1 = string.Format("Assets/_Assets/Resources/Json/Localize/{0}.json", id + "-" + fileName);
                StreamWriter writer1 = new StreamWriter(path1, false);
                writer1.WriteLine(JsonConvert.SerializeObject(verifyDict, Formatting.Indented));
                writer1.Close();

#if UNITY_EDITOR
                AssetDatabase.ImportAsset(path1);
#endif
            }
            catch
            {
                Debug.LogError("Cannot validate =" + files[i]);
            }
        }

        string pathRaw = "Assets/_Assets/Resources/Json/LocalizeEnglishEmpty.json";
        StreamWriter wr = new StreamWriter(pathRaw, false);
        wr.WriteLine(JsonConvert.SerializeObject(dictEnglish, Formatting.Indented));
        wr.Close();
#if UNITY_EDITOR
        AssetDatabase.ImportAsset(pathRaw);
#endif
    }

    private static void WriteLanguageFile(string content)
    {
#if UNITY_EDITOR
        string path = "Assets/_Assets/Resources/Json/Localize/English.json";
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(content);
        writer.Close();
        AssetDatabase.ImportAsset(path);
#endif
    }

    //[MenuItem("Editor Utils/Localize/Scan File And Translate")]
    [MenuItem("Editor Utils/Localize/Dịch tự động")]
    public static void ScanFileAndTranslate()
    {
        string msg = string.Empty;

        List<TextAsset> files = Resources.LoadAll<TextAsset>("Json/Localize/").OrderBy(x => int.Parse(x.name.Split('-')[0])).ToList();

        for (int i = 0; i < files.Count; i++)
        {
            if (i > 1)
            {
                string[] splits = files[i].name.Split('-');
                string fileName = splits[1];

                Dictionary<string, string> dictLocalize = JsonConvert.DeserializeObject<Dictionary<string, string>>(files[i].text);
                List<KeyValuePair<string, string>> pairNeedTranslate = dictLocalize.Where(x => string.IsNullOrEmpty(x.Key) == false && string.IsNullOrEmpty(x.Value)).ToList();

                if (pairNeedTranslate.Count > 0)
                {
                    msg += string.Format("{0} - {1} lines", fileName, pairNeedTranslate.Count);
                    msg += "\n";
                }
            }
        }

        if (EditorUtility.DisplayDialog("Auto Translate", msg, "Ok", "Cancel"))
        {
            LocalizeManager.Instance.ScanFileAndTranslate();
        }
    }

    //[MenuItem("Editor Utils/Localize/Skills to File")]
    //public static void SkillToFile()
    //{
    //    Dictionary<string, string> dict = new Dictionary<string, string>();
    //    List<BaseSkillData> skillFiles = Resources.LoadAll<BaseSkillData>("Heroes").ToList();
    //    for (int i = 0; i < skillFiles.Count; i++)
    //    {
    //        BaseSkillData file = skillFiles[i];
    //        for (int j = 0; j < file.formatDescription.Length; j++)
    //        {
    //            string desc = file.formatDescription[j];
    //            dict[desc] = "";
    //        }
    //    }

    //    string path = "Assets/_Assets/Resources/Json/LocalizeMiniFiles/0-skills.json";
    //    StreamWriter writer1 = new StreamWriter(path, false);
    //    writer1.WriteLine(JsonConvert.SerializeObject(dict, Formatting.Indented));
    //    writer1.Close();
    //    AssetDatabase.ImportAsset(path);
    //}

    //[MenuItem("Editor Utils/Localize/VerifyEnglish")]
    //public static void VerifyEnglish()
    //{
    //    TextAsset fileEnglish = Resources.Load<TextAsset>("Json/Localize/0-English");
    //    Dictionary<string, string> dictEnglish = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileEnglish.text);
    //    TextAsset fileSkill = Resources.Load<TextAsset>("Json/LocalizeMiniFiles/0-skills");
    //    Dictionary<string, string> dictSkills = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileSkill.text);
    //    var e = dictSkills.GetEnumerator();
    //    while (e.MoveNext())
    //    {
    //        string key = e.Current.Key;
    //        if (string.IsNullOrEmpty(key) == false && dictEnglish.ContainsKey(key))
    //        {
    //            dictEnglish.Remove(key);
    //        }
    //    }

    //    string path = "Assets/_Assets/Resources/Json/Localize/0-English.json";
    //    StreamWriter writer1 = new StreamWriter(path, false);
    //    writer1.WriteLine(JsonConvert.SerializeObject(dictEnglish, Formatting.Indented));
    //    writer1.Close();
    //    AssetDatabase.ImportAsset(path);
    //}
}
#endif