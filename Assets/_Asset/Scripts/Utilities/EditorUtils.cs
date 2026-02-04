
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EditorUtils : MonoBehaviour
{
    #region Fast Open Scenes
    [MenuItem("Editor Utils/Open Scene/Bootstrap &0")]
    public static void OpenSceneBootstrap()
    {
        OpenScene("Bootstrap");
    }

    [MenuItem("Editor Utils/Open Scene/Root &1")]
    public static void OpenSceneRoot()
    {
        OpenScene(GameConfig.SCENE_ROOT);
    }

    [MenuItem("Editor Utils/Open Scene/Home &2")]
    public static void OpenSceneHome()
    {
        OpenScene(GameConfig.SCENE_LOBBY);
    }

    [MenuItem("Editor Utils/Open Scene/Game &3")]
    public static void OpenSceneGame()
    {
        OpenScene(GameConfig.SCENE_GAME);
    }

    [MenuItem("Editor Utils/Open Scene/GD &4")]
    public static void OpenSceneGD()
    {
        OpenScene(GameConfig.SCENE_GD);
    }

    private static void OpenScene(string sceneName)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/_Asset/Scenes/" + sceneName + ".unity");
        }
    }
    #endregion

    #region Player Prefs
    //[MenuItem("Editor Utils/New Day")]
    //public static void NewDay()
    //{
    //    DateTime lastDay = GameData.userData.profile.lastDayLogin.Date;
    //    DateTime prevDay = lastDay.AddDays(-1f);
    //    GameData.userData.profile.lastDayLogin = prevDay;
    //    GameData.userData.campaign.lastTimeGetAfkRewards = prevDay;
    //    GameData.userData.profile.isDataChanged = true;
    //}

    //[MenuItem("Editor Utils/Missing Scripts")]
    //public static void GetMissingScriptInScene()
    //{
    //    MonoBehaviour[] scripts = Resources.FindObjectsOfTypeAll<MonoBehaviour>();

    //    foreach (MonoBehaviour script in scripts)
    //    {
    //        if (script == null)
    //        {
    //            DebugCustom.LogFormat("Missing=" + script.gameObject.name);
    //        }
    //    }
    //}
    #endregion

}
#endif