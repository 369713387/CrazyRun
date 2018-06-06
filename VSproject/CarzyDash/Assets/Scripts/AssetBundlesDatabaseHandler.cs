using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This class handles listing the bundles and distributing
/// them to the databases to load.
/// </summary>
public class AssetBundlesDatabaseHandler
{
    static public void Load()
    {
        CoroutineHandler.StartStaticCoroutine(AsyncLoad());
    }

    static IEnumerator AsyncLoad()
    {

        // Android store streams assets in a compressed archive, so different file system.
#if !UNITY_EDITOR
#if UNITY_ANDROID
        AssetBundleManager.BaseDownloadingURL = Application.streamingAssetsPath + "/AssetBundles/"+Utility.GetPlatformName()+"/";
#else
        AssetBundleManager.BaseDownloadingURL = "file://" + Application.streamingAssetsPath + "/AssetBundles/"+Utility.GetPlatformName()+"/";
#endif
#else
        AssetBundleManager.BaseDownloadingURL = "file://" + Application.streamingAssetsPath + "/AssetBundles/" + Utility.GetPlatformName() + "/";
#endif

        var request = AssetBundleManager.Initialize();
        if (request != null)
            yield return CoroutineHandler.StartStaticCoroutine(request);

        // In editor we can directly get all the bundles but in final build, we need to read them from the manifest.
#if UNITY_EDITOR
        string[] bundles;
        if (AssetBundleManager.SimulateAssetBundleInEditor)
            bundles = AssetDatabase.GetAllAssetBundleNames();
        else
            bundles = AssetBundleManager.AssetBundleManifestObject.GetAllAssetBundles();
#else
        string[] bundles = AssetBundleManager.AssetBundleManifestObject.GetAllAssetBundles();
#endif

        List<string> characterPackage = new List<string>();
        List<string> themePackage = new List<string>();

        for (int i = 0; i < bundles.Length; ++i)
        {
            if (bundles[i].StartsWith("characters/"))
                characterPackage.Add(bundles[i]);
            else if (bundles[i].StartsWith("themes/"))
                themePackage.Add(bundles[i]);
        }

        yield return CoroutineHandler.StartStaticCoroutine(CharacterDatabase.LoadDatabase(characterPackage));
        yield return CoroutineHandler.StartStaticCoroutine(ThemeDatabase.LoadDatabase(themePackage));
    }
}
