using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using UnityEngine;

public class GameConfigManager : MonoBehaviour
{
    // Các biến tĩnh để lưu trữ giá trị cấu hình
    public static int levelMinShowInter = 12;
    public static int INTER_COOLDOWN = 60;
    public static float timeWaitPopupShowInter = 1.5f;

    void Awake()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return;
#endif
        DontDestroyOnLoad(this.gameObject);
        InitializeFirebaseAndRemoteConfig();
    }

    private void InitializeFirebaseAndRemoteConfig()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase đã được khởi tạo và sẵn sàng sử dụng
                FirebaseApp app = FirebaseApp.DefaultInstance;

                // Lấy instance của Remote Config
                FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;

                // 1. Đặt các giá trị mặc định cho Remote Config
                // Đây là các giá trị sẽ được sử dụng nếu không có giá trị từ server
                // hoặc khi remote config chưa được fetch thành công.
                System.Collections.Generic.Dictionary<string, object> defaults =
                    new System.Collections.Generic.Dictionary<string, object>();

                defaults.Add("level_min_show_inter", 12);
                defaults.Add("inter_cooldown", 60);
                defaults.Add("time_wait_popup_show_inter", 1.5f);

                remoteConfig.SetDefaultsAsync(defaults);

                // 2. Cấu hình cài đặt Fetch
                // Trong môi trường phát triển, bạn có thể đặt MinimumFetchIntervalInMilliseconds thấp
                // để thường xuyên nhận được các bản cập nhật.
                // Trong môi trường production, bạn nên đặt giá trị cao hơn (ví dụ: 12 giờ)
                // để tránh bị giới hạn tốc độ (throttling).
                ConfigSettings configSettings = new ConfigSettings
                {
                    MinimumFetchIntervalInMilliseconds = (ulong)TimeSpan.FromHours(12).TotalMilliseconds,
                    FetchTimeoutInMilliseconds = (ulong)TimeSpan.FromMinutes(1).TotalMilliseconds
                };
                // Khi phát triển, bạn có thể sử dụng TimeSpan.Zero để luôn lấy cấu hình mới nhất
                // configSettings.MinimumFetchIntervalInMilliseconds = TimeSpan.Zero.TotalMilliseconds; // Dùng khi phát triển

                remoteConfig.SetConfigSettingsAsync(configSettings).ContinueWithOnMainThread(task => {
                    Debug.Log("Remote Config settings configured.");
                    // Sau khi cấu hình, tiến hành fetch và activate
                    FetchAndActivateRemoteConfig();
                });

            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void FetchAndActivateRemoteConfig()
    {
        FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        Debug.Log("Fetching Remote Config data...");
        // FetchAsync lấy dữ liệu từ server.
        // Tiếp theo, ActivateAsync kích hoạt dữ liệu đã tải xuống, làm cho nó có sẵn để sử dụng.
        remoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
        {
            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.LogError($"Fetch encountered an error: {fetchTask.Exception}");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch complete!");
                remoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                {
                    if (activateTask.IsCanceled)
                    {
                        Debug.Log("Activate canceled.");
                    }
                    else if (activateTask.IsFaulted)
                    {
                        Debug.LogError($"Activate encountered an error: {activateTask.Exception}");
                    }
                    else if (activateTask.IsCompleted)
                    {
                        Debug.Log("Remote Config activated!");
                        // 3. Sử dụng các giá trị cấu hình đã lấy
                        UpdateConfigValues();
                    }
                });
            }
        });
    }

    private void UpdateConfigValues()
    {
        FirebaseRemoteConfig remoteConfig = FirebaseRemoteConfig.DefaultInstance;

        levelMinShowInter = (int)remoteConfig.GetValue("level_min_show_inter").LongValue;
        INTER_COOLDOWN = (int)remoteConfig.GetValue("inter_cooldown").LongValue;
        timeWaitPopupShowInter = (float)remoteConfig.GetValue("time_wait_popup_show_inter").DoubleValue;

        GameConfig.levelMinShowInter = levelMinShowInter;
        GameConfig.INTER_COOLDOWN = INTER_COOLDOWN;
        GameConfig.timeWaitPopupShowInter = timeWaitPopupShowInter;

        Debug.Log($"Updated Config Values:");
        Debug.Log($"levelMinShowInter: {levelMinShowInter}");
        Debug.Log($"INTER_COOLDOWN: {INTER_COOLDOWN}");
        Debug.Log($"timeWaitPopupShowInter: {timeWaitPopupShowInter}");
    }
}