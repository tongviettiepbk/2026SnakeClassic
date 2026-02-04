using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Falcon.FalconCore.Editor.Payloads;
using Falcon.FalconCore.Editor.Repositories;
using Falcon.FalconCore.Editor.Services;
using Falcon.FalconCore.Editor.Utils;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEditor;
using UnityEngine;

namespace Falcon.FalconCore.Editor.Models
{
    public class FPlugin
    {
        private const string UnityPackageExtension = ".unitypackage";

        public FPlugin(BitBucObj obj)
        {
            var tokens = obj.Path.Split('/');
            PluginName = tokens[tokens.Length - 1];
            string[] tokens1 = PluginName.Split(' ');
            PluginShortName = tokens1[tokens1.Length - 1];

            BitBucObj remoteConfigLink = null;
            var pluginVersions = new HashSet<BitBucObj>();

            foreach (var value in BitBucCall.OfUrl(obj.Links.Self.HRef))
            {
                var href = value.Links.Self.HRef;
                if (href != null && href.EndsWith("config.txt"))
                    remoteConfigLink = value;
                else if (href != null && !href.EndsWith(".meta")) pluginVersions.Add(value);
            }

            RemoteConfig = JsonUtil.FromJson<FPluginMeta>(new HttpRequest
            {
                RequestType = HttpMethod.Get,
                URL = remoteConfigLink.Links.Self.HRef
            }.InvokeAndGet());
            UpdateRemoteConfigFromBigBucObjs(pluginVersions);

            UpdateInstalledConfig();
        }

        private void UpdateRemoteConfigFromBigBucObjs(HashSet<BitBucObj> bitBucObjs)
        {
            foreach (var url in bitBucObjs)
            {
                if (url.Path.EndsWith(PluginShortName + "-" + RemoteConfig.version +
                                      UnityPackageExtension)) PluginUrl = url.Links.Self.HRef;
            }

            if (PluginUrl == null) PluginUrl = bitBucObjs.First().Links.Self.HRef;
        }

        private void UpdateInstalledConfig()
        {
            var directory =
                Directory.GetDirectories(FalconCoreFileUtils.ApplicationDataPath, PluginShortName,
                    SearchOption.AllDirectories);

            if (directory.Length == 0) Installed = false;
            else
                try
                {
                    Installed = true;
                    InstalledDirectory = directory[0].Contains("Release") ? directory[1] : directory[0];
                    InstalledConfig = JsonUtil.FromJson<FPluginMeta>(File.ReadAllText(
                        InstalledDirectory + Path.DirectorySeparatorChar + "config.txt"));
                }
                catch (Exception)
                {
                    Installed = false;
                }
        }

        public FPluginMeta InstalledConfig { get; private set; }

        public string PluginName { get; }
        public string PluginShortName { get; private set; }
        public FPluginMeta RemoteConfig { get; private set; }

        public string PluginUrl { get; private set; }
        public bool Installed { get; private set; }
        public bool IsDownloading { get; private set; }
        public string InstalledDirectory { get; private set; }
        public int progress;

        public IEnumerator Install()
        {
            var requireJson = RemoteConfig.Require;
            var requirePlugins = new List<FPlugin>();
            if (requireJson != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in requireJson)
                {
                    FPlugin plugin;
                    while (!FPluginRepo.TryGet(keyValuePair.Key, out plugin))
                    {
                        yield return null;
                    }

                    if (plugin.InstalledConfig == null ||
                        string.CompareOrdinal(plugin.InstalledConfig.version, keyValuePair.Value) < 0)
                    {
                        requirePlugins.Add(plugin);
                    }
                }
            }

            if (requirePlugins.Count == 0)
            {
                yield return UnsafeInstall();
                yield break;
            }

            var requirePluginString = new StringBuilder();
            requirePluginString
                .Append("The following plugins are required for" + PluginShortName + ":").AppendLine();
            foreach (var plugin in requirePlugins)
                requirePluginString.Append("  - ").Append(plugin.PluginShortName).AppendLine();
            requirePluginString.Append("Please install/update them first!");
            new EditorMainThreadAction(() =>
                {
                    EditorUtility.DisplayDialog("Additional plugin require!!!", requirePluginString.ToString(), "Ok");
                }
            ).Schedule();
        }

        void DeleteFileOrFolder(string path)
        {
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
        }

        private IEnumerator UnsafeInstall()
        {
            var tempFolder = FalconCoreFileUtils.ApplicationDataPath + "/../Temp/" + PluginShortName +
                             UnityPackageExtension;

            var fileGetRequest = new FileGetRequest(PluginUrl, tempFolder);
            new Thread(fileGetRequest.Invoke).Start();
            CoreLogger.Instance.Info("Downloading " + PluginShortName);
            IsDownloading = true;
            while (!fileGetRequest.Done)
            {
                progress = fileGetRequest.progress;
                yield return null;
            }

            progress = 100;
            yield return new WaitForSecondsRealtime(0.3f);
            InstalledConfig = RemoteConfig;
            Installed = true;
            InstalledDirectory = Path.Combine(FalconCoreFileUtils.GetFalconPluginFolder(), PluginName);
            IsDownloading = false;
            CoreLogger.Instance.Info("Downloading complete, preparing to import");
            new EditorMainThreadAction(() => AssetDatabase.ImportPackage(tempFolder, true)).Schedule();
        }

        public void UnInstall()
        {
            CoreLogger.Instance.Info("Start uninstallation");
            if (InstalledDirectory.Contains("FalconAdjust"))
            {
                DeleteFileOrFolder("Assets/Adjust");
            }

            if (InstalledDirectory.Contains("FalconAppsflyer"))
            {
                DeleteFileOrFolder("Assets/Appsflyer");
            }

            DeleteFileOrFolder(InstalledDirectory);
            Installed = false;
            InstalledConfig = null;
            InstalledDirectory = null;
            AssetDatabase.Refresh();
        }

        public bool IsFalconCore()
        {
            return string.CompareOrdinal(PluginShortName, "FalconCore") == 0;
        }

        public bool InstalledNewest()
        {
            if (string.CompareOrdinal(InstalledConfig.version, RemoteConfig.version) < 0) return false;
            return true;
        }
    }
}