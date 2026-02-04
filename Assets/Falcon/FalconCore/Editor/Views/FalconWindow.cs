using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Falcon.FalconCore.Editor.Models;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Editor.Services;
using Falcon.FalconCore.Editor.Utils;
using Falcon.FalconCore.Scripts.Services.GameObjs;
using UnityEditor;
using UnityEngine;

namespace Falcon.FalconCore.Editor.Views
{
    namespace Falcon
    {
        /**
         * View is merged with controller, for being lazy ._.|||
         */
        public class FalconWindow : EditorWindow
        {
            string updateStr = "Update";
            string downloadingStr = "Downloading";

            float buttonWidth = 120;
            float buttonHeight = 20;

            private Texture2D trashIcon;

            private void Awake()
            {
                trashIcon = new Texture2D(2, 2);
                trashIcon.LoadImage(File.ReadAllBytes(FalconCoreFileUtils.GetFalconPluginFolder() +
                                                      @"/FalconCore/Editor/images/trash.png"));
            }

            private void OnDisable()
            {
                if (!Application.isPlaying) DestroyImmediate(FGameObj.Instance.gameObject);
            }

            private void OnGUI()
            {
                if (FKeyService.FKeyValid())
                    RenderPluginMenu();
                else if (FKeyService.Validating)
                    RenderWaitingMenu("Logging In, please wait");
                else
                    RenderLoginMenu();
            }

            [MenuItem("Falcon/FalconMenu", priority = 0)]
            public static void ShowWindow()
            {
                var window = GetWindow<FalconWindow>("Falcon Modules", true);
                window.minSize = new Vector2(460, 600);
                window.maxSize = new Vector2(460, 800);

                window.Show();
            }

            private void RenderWaitingMenu(string message)
            {
                GUIVertical(() =>
                {
                    GUILayout.Space(20);

                    GUILayout.Label(message);
                });
            }

            private void RenderPluginMenu()
            {
                ICollection<FPlugin> plugins;
                if (!FPluginRepo.TryGetAll(out plugins))
                {
                    RenderWaitingMenu("Plugin are being Loaded. please wait!!!");
                }
                else
                {
                    GUIVertical(() =>
                    {
                        GUILayout.Space(20);
                        GUILayout.Label("Loaded " + plugins.Count + "/" + FPluginRepo.RemotePluginCount +
                                        " plugin, some may are still being loaded");
                    });

                    GUIHorizon(() =>
                    {
                        if (GUILayout.Button("Refresh", GUILayout.Width(100), GUILayout.Height(20)))
                        {
                            FPluginRepo.ClearCache();
                            new Thread(FPluginRepo.Init).Start();
                        }

                        GUILayout.Space(20);
                        if (GUILayout.Button("LogOut", GUILayout.Width(100), GUILayout.Height(20)))
                        {
                            FKeyService.RemoveFKey();
                        }
                    });

                    foreach (var plugin in plugins) RenderPluginItem(plugin);
                }
            }

            private void RenderPluginItem(FPlugin plugin)
            {
                GUIVertical(() => { GUILayout.Space(20); });

                GUIHorizon(() =>
                {
                    SpaceFirst();
                    if (!plugin.Installed)
                    {
                        RenderUninstalledPlugin(plugin);
                    }
                    else
                    {
                        if (!plugin.InstalledNewest())
                            RenderOldPlugin(plugin);
                        else
                            RenderNewestPlugin(plugin);
                    }

                    SpaceEnd();
                });
            }

            private void SpaceFirst()
            {
                GUILayout.Space(10);
            }

            private void SpaceEnd()
            {
                GUILayout.Space(5);
            }

            private void RenderUninstalledPlugin(FPlugin plugin)
            {
                GUILayout.Label(plugin.PluginShortName + " v" + plugin.RemoteConfig.version);

                if (plugin.IsDownloading)
                {
                    GUI.enabled = false;
                    GUILayout.Button(downloadingStr + " " + plugin.progress + "%",
                        GUILayout.Width(buttonWidth + buttonHeight), GUILayout.Height(buttonHeight));
                    GUI.enabled = true;
                }
                else
                {
                    if (GUILayout.Button("Install", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                        new EditorSequence(plugin.Install()).Start();
                    GUI.enabled = false;
                    GUILayout.Button(trashIcon, GUILayout.Width(buttonHeight), GUILayout.Height(buttonHeight));

                    GUI.enabled = true;
                }
            }

            private void RenderOldPlugin(FPlugin plugin)
            {
                GUILayout.Label(plugin.PluginShortName + " v" + plugin.RemoteConfig.version +
                                " (current v" + plugin.InstalledConfig.version + ")");
                if (plugin.IsDownloading)
                {
                    GUI.enabled = false;
                    GUILayout.Button(downloadingStr + " " + plugin.progress + "%",
                        GUILayout.Width(buttonWidth + buttonHeight), GUILayout.Height(buttonHeight));
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = true;
                    if (GUILayout.Button(updateStr, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                        new EditorSequence(plugin.Install()).Start();

                    GUI.enabled = !plugin.IsFalconCore();

                    if (GUILayout.Button(trashIcon, GUILayout.Width(buttonHeight), GUILayout.Height(buttonHeight)))
                    {
                        plugin.UnInstall();
                    }

                    GUI.enabled = true;
                }
            }

            private void RenderNewestPlugin(FPlugin plugin)
            {
                GUILayout.Label(plugin.PluginShortName + " v" + plugin.RemoteConfig.version);
                GUI.enabled = false;
                GUILayout.Button("Install", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight));

                GUI.enabled = !plugin.IsFalconCore();

                if (GUILayout.Button(trashIcon, GUILayout.Width(buttonHeight), GUILayout.Height(buttonHeight)))
                    plugin.UnInstall();

                GUI.enabled = true;
            }

            private void GUIHorizon(Action action)
            {
                GUILayout.BeginHorizontal();
                action.Invoke();
                GUILayout.EndHorizontal();
            }

            private void GUIVertical(Action action)
            {
                GUILayout.BeginVertical();
                action.Invoke();
                GUILayout.EndVertical();
            }


            #region Login

            private string userInputFalconKey;

            private void RenderLoginMenu()
            {
                //Module Login
                GUIVertical(() =>
                {
                    GUILayout.Space(20);
                    GUILayout.Label("Falcon Key : ");
                    userInputFalconKey = GUILayout.TextField(userInputFalconKey);
                    GUILayout.Space(5);
                    if (GUILayout.Button("Login", GUILayout.Width(100), GUILayout.Height(20)))
                    {
                        FKeyService.ValidateFKey(userInputFalconKey);
                    }
                });
                GUILayout.BeginVertical();


                GUILayout.EndVertical();
            }

            #endregion
        }
    }
}