using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
/// <summary>
/// 处理来自AssetBundle的加载数据以处理游戏的不同主题
/// </summary>
public class ThemeDatabase{

    static protected Dictionary<string, ThemeData> themeDataList;
    static public Dictionary<string, ThemeData> dictionnary { get { return themeDataList; } }

    static protected bool m_Loaded = false;
    public static bool loaded { get { return m_Loaded; } }

    static public ThemeData GetThemeData(string type)
    {
        ThemeData list;
        if (themeDataList == null || !themeDataList.TryGetValue(type, out list))
            return null;

        return list;
    }

    static public IEnumerator LoadDatabase(List<string> packages)
    {
        //如果不为null，字典就已经载入了
        if(themeDataList == null)
        {
            themeDataList = new Dictionary<string, ThemeData>();

            foreach (string s in packages)
            {
                AssetBundleLoadAssetOperation op = AssetBundleManager.LoadAssetAsync(s, "themeData", typeof(ThemeData));
                yield return CoroutineHandler.StartStaticCoroutine(op);

                ThemeData list = op.GetAsset<ThemeData>();
                if (list != null)
                {
                    themeDataList.Add(list.themeName, list);
                }
            }

            m_Loaded = true;
        }
    }
}
