using UnityEngine;
using AssetBundles;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This allows us to store a database of all characters currently in the bundles, indexed by name.
/// 这允许我们按名称索引存储当前在包中所有角色的数据库。
/// </summary>
public class CharacterDatabase
{
    static protected Dictionary<string, Character> m_CharactersDict;

    static public Dictionary<string, Character> dictionary { get { return m_CharactersDict; } }

    static protected bool m_Loaded = false;
    static public bool loaded { get { return m_Loaded; } }

    static public Character GetCharacter(string type)
    {
        Character c;
        if (m_CharactersDict == null || !m_CharactersDict.TryGetValue(type, out c))
            return null;

        return c;
    }

    static public IEnumerator LoadDatabase(List<string> packages)
    {
        if (m_CharactersDict == null)
        {
            m_CharactersDict = new Dictionary<string, Character>();

            foreach (string s in packages)
            {
                AssetBundleLoadAssetOperation op = AssetBundleManager.LoadAssetAsync(s, "character", typeof(GameObject));
                yield return CoroutineHandler.StartStaticCoroutine(op);

                Character c = op.GetAsset<GameObject>().GetComponent<Character>();
                if (c != null)
                {
                    m_CharactersDict.Add(c.characterName, c);
                }
            }

            m_Loaded = true;
        }
    }
}