using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections.Generic;
namespace AssetBundles
{
    public class LoadedAssetBundle
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    public class AssetBundleManager : MonoBehaviour
    {
        public enum LogMode { All, JustErrors };
        public enum LogType { Info, Warning, Error };

        static LogMode s_LogMode = LogMode.All;
        //下载更新的服务器地址
        static string s_BaseDownloadingURL = "";
        static string[] s_ActiveVariants = { };
        //资源清单
        static AssetBundleManifest s_AssetBundleManifest;
#if UNITY_EDITOR
        static int s_SimulateAssetBundleInEditor = -1;
        const string k_SimulateAssetBundles = "SimulateAssetBundles";
#endif

        static Dictionary<string, LoadedAssetBundle> s_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static Dictionary<string, UnityWebRequest> s_DownloadingWebrequests = new Dictionary<string, UnityWebRequest>();
        static Dictionary<string, string> s_DownloadingErrors = new Dictionary<string, string>();
        static List<AssetBundleLoadOperation> s_InProgressOperations = new List<AssetBundleLoadOperation>();
        static Dictionary<string, string[]> s_Dependencies = new Dictionary<string, string[]>();
        #region 封装属性
        public static LogMode logMode
        {
            get { return s_LogMode; }
            set { s_LogMode = value; }
        }

        public static string BaseDownloadingURL
        {
            get
            {
                return s_BaseDownloadingURL;
            }

            set
            {
                s_BaseDownloadingURL = value;
            }
        }

        public static string[] ActiveVariants
        {
            get
            {
                return s_ActiveVariants;
            }

            set
            {
                s_ActiveVariants = value;
            }
        }

        public static AssetBundleManifest AssetBundleManifestObject
        {
            get
            {
                return s_AssetBundleManifest;
            }

            set
            {
                s_AssetBundleManifest = value;
            }
        }
        #endregion
        /// <summary>
        /// 输出Assetbundle的信息
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="text"></param>
        private static void Log(LogType logType,string text)
        {
            if (logType == LogType.Error)
            {
                Debug.LogError("[AssetBundleManager]" + text);
            }else if(s_LogMode == LogMode.All)
            {
                Debug.Log("[AssetBundleManager]" + text);
            }
        }

#if UNITY_EDITOR
        // 标记，以指示我们是否想在编辑器中模拟assetbundle，而不需要构建它们。
        public static bool SimulateAssetBundleInEditor
        {
            get
            {
                if (s_SimulateAssetBundleInEditor == -1)
                    s_SimulateAssetBundleInEditor = EditorPrefs.GetBool(k_SimulateAssetBundles, true) ? 1 : 0;

                return s_SimulateAssetBundleInEditor != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != s_SimulateAssetBundleInEditor)
                {
                    s_SimulateAssetBundleInEditor = newValue;
                    EditorPrefs.SetBool(k_SimulateAssetBundles, value);
                }
            }
        }
#endif
        /// <summary>
        /// 返回StreamingAssets的路径
        /// </summary>
        /// <returns></returns>
        private static string GetStreamingAssetsPath()
        {
            if (Application.isEditor)
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            if (Application.isMobilePlatform || Application.isConsolePlatform)
                return Application.streamingAssetsPath;
            return "file://" + Application.streamingAssetsPath;
        }

        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = GetStreamingAssetsPath() + relativePath;
        }

        public static void SetSourceAssetBundleURL(string absolutePath)
        {
            BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
        }

        public static void SetDevelopmentAssetBundleServer()
        {
#if UNITY_EDITOR
            // 我们在编辑模式下，我们不需要设置URL
            if (SimulateAssetBundleInEditor)
                return;
#endif
            TextAsset urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
            string url = (urlFile != null) ? urlFile.text.Trim() : null;
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("Development Server URL could not be found.");
            }
            else
            {
                SetSourceAssetBundleURL(url);
            }
        }
        //获取加载的AssetBundle 当成功下载所有依赖项时才返回vaild对象
        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName,out string error)
        {
            if(s_DownloadingErrors.TryGetValue(assetBundleName,out error))
            {
                return null; 
            }

            LoadedAssetBundle bundle;
            s_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle == null)
            {
                return null;
            }
            //只有bundle本身是必需的
            string[] dependencies;
            if(!s_Dependencies.TryGetValue(assetBundleName,out dependencies))
            {
                return bundle;
            }
            //确保加载所有依赖项
            foreach (var dependency in dependencies)
            {
                if (s_DownloadingErrors.TryGetValue(assetBundleName, out error))
                    return bundle;
                //等待所有的被依赖的assetBundles被加载
                LoadedAssetBundle dependentBundle;
                s_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if(dependentBundle == null)
                {
                    return null;
                }
            }

            return bundle;
        }
        /// <summary>
        /// 对AssetBundle加载清单进行的操作
        /// </summary>
        /// <returns></returns>
        static public AssetBundleLoadManifestOperation Initialize()
        {
            return Initialize(Utility.GetPlatformName());
        }

        /// <summary>
        /// Initialize AssetBundleManifest
        /// </summary>       
        static public AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
        {
            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);
#if UNITY_EDITOR
            Log(LogType.Info, "Simulation Mode:" + (SimulateAssetBundleInEditor ? "Enable" : "Disable"));

            if (SimulateAssetBundleInEditor)
                return null;
#endif
            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            s_InProgressOperations.Add(operation);
            return operation;
        }

        //加载AssetBundle 和他的依赖项
        static protected void LoadAssetBundle(string assetBundleName,bool isLoadingAssetBundleManifest = false)
        {
            Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);

#if UNITY_EDITOR
            // 如果我们在编辑器模拟模式下，我们不需要真正加载assetBundle及其依赖项。
            if (SimulateAssetBundleInEditor)
                return;
#endif

            if (!isLoadingAssetBundleManifest)
            {
                if (s_AssetBundleManifest == null)
                {
                    Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }

            // 检查assetBundle是否已经被处理过。
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

            // 加载dependencies.
            if (!isAlreadyProcessed && !isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }
        /// <summary>
        /// 重新映射ASB变体名字
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        static protected string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = s_AssetBundleManifest.GetAllAssetBundlesWithVariant();

            string[] split = assetBundleName.Split(',');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;

            //将所有带有变体的assetBundle循环，以找到最适合的变体assetBundle
            for(int i = 0;i< bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split(',');
                if (curSplit[0] != split[0])
                {
                    continue;
                }

                int found = System.Array.IndexOf(s_ActiveVariants, curSplit[1]);

                //如果没有发现有效的变体。我们还是要用第一个
                if(found == -1)
                {
                    found = int.MaxValue - 1;
                }

                if (found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if(bestFit == int.MaxValue - 1)
            {
                Debug.LogWarning("Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex]);
            }

            if(bestFitIndex != -1)
            {
                return bundlesWithVariant[bestFitIndex];
            }
            return assetBundleName;
        }

        //调用WWW下载assetBundle
        static protected bool LoadAssetBundleInternal(string assetBundleName,bool isLoadingAssetBundleManifest) {
            // Already loaded.
            LoadedAssetBundle bundle;
            s_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                bundle.m_ReferencedCount++;
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
            if (s_DownloadingWebrequests.ContainsKey(assetBundleName))
                return true;

            UnityWebRequest download;
            string url = s_BaseDownloadingURL + assetBundleName;

            // For manifest assetbundle, always download it as we don't have hash for it.
            if (isLoadingAssetBundleManifest)
                download = UnityWebRequest.GetAssetBundle(url);
            else
                download = UnityWebRequest.GetAssetBundle(url, s_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

            //download.SendWebRequest();
            s_DownloadingWebrequests.Add(assetBundleName, download);

            return false;
        }

        // 加载所有依赖项
        static protected void LoadDependencies(string assetBundleName)
        {
            if (s_AssetBundleManifest == null)
            {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = s_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            s_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
                LoadAssetBundleInternal(dependencies[i], false);
        }

        // 卸载 assetbundle and its dependencies.
        static public void UnloadAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
                return;
#endif

            UnloadAssetBundleInternal(assetBundleName);
            UnloadDependencies(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies;
            if (!s_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency);
            }

            s_Dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if (bundle == null)
                return;

            if (--bundle.m_ReferencedCount == 0)
            {
                bundle.m_AssetBundle.Unload(false);
                s_LoadedAssetBundles.Remove(assetBundleName);

                Log(LogType.Info, assetBundleName + " has been unloaded successfully");
            }
        }
        //不断从网络上检查需要加载asb
        void Update()
        {
            // 收集 all the finished WWWs.
            var keysToRemove = new List<string>();

            if (s_DownloadingWebrequests.Count > 0)
            {
                foreach (var keyValue in s_DownloadingWebrequests)
                {
                    UnityWebRequest download = keyValue.Value;

                    // If downloading fails.
                    if (!string.IsNullOrEmpty(download.error))
                    {
                        s_DownloadingErrors.Add(keyValue.Key, string.Format("Failed downloading bundle {0} from {1}: {2}", keyValue.Key, download.url, download.error));
                        keysToRemove.Add(keyValue.Key);
                        continue;
                    }

                    // If downloading succeeds.
                    if (download.isDone)
                    {
                        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(download);
                        if (bundle == null)
                        {
                            s_DownloadingErrors.Add(keyValue.Key, string.Format("{0} is not a valid asset bundle.", keyValue.Key));
                            keysToRemove.Add(keyValue.Key);
                            continue;
                        }

                        s_LoadedAssetBundles.Add(keyValue.Key, new LoadedAssetBundle(bundle));
                        keysToRemove.Add(keyValue.Key);
                    }
                }
            }

            // Remove the finished WWWs.
            foreach (var key in keysToRemove)
            {
                UnityWebRequest download = s_DownloadingWebrequests[key];
                s_DownloadingWebrequests.Remove(key);
                download.Dispose();
            }

            // Update all in progress operations
            for (int i = 0; i < s_InProgressOperations.Count;)
            {
                if (!s_InProgressOperations[i].Update())
                {
                    s_InProgressOperations.RemoveAt(i);
                }
                else
                    i++;
            }
        }

        // Load asset from the given assetBundle.
        static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

            AssetBundleLoadAssetOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if (assetPaths.Length == 0)
                {
                    Debug.LogError("There is no asset with name \"" + assetName + "\" in " + assetBundleName);
                    return null;
                }

                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                Object target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                operation = new AssetBundleLoadAssetOperationSimulation(target);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

                s_InProgressOperations.Add(operation);
            }

            return operation;
        }

        // Load level from the given assetBundle.
        static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

            AssetBundleLoadOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                operation = new AssetBundleLoadLevelSimulationOperation(assetBundleName, levelName, isAdditive);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

                s_InProgressOperations.Add(operation);
            }

            return operation;
        }
    } // End of AssetBundleManager.
}
