using UnityEngine;
using UnityEditor;

namespace AssetBundles{
    public class AssetbundlesMenuItems {
        const string k_SimulationMode = "Assets/AssetBundles/Simulation Mode";
        const string k_BuildAssetBundles = "Assets/AssetBundles/Build AssetBundles";

        [MenuItem(k_SimulationMode)]
        public static void ToggleSimulationMode()
        {
            AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        }

        [MenuItem(k_SimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(k_SimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
            return true;
        }

        [MenuItem(k_BuildAssetBundles)]
        static public void BuildAssetBundles()
        {
            BuildScript.BuildAssetBundles();
        }
    }
}
