using UnityEngine;
using UnityEditor;

public class BundleAndBuild
{
    [MenuItem("Trash Dash Debug/Build with Bundle/Build")]
    static void Build()
    {
        AssetBundles.BuildScript.BuildStandalonePlayer();
    }

    [MenuItem("Trash Dash Debug/Build with Bundle/Build and Run")]
    static void BuildAndRun()
    {
        AssetBundles.BuildScript.BuildStandalonePlayer(true);
    }
}
