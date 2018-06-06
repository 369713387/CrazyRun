using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主要用于定义角色的一些数据容器，该脚本绑定在prefabs上，定义域角色相关的所有的数据
/// </summary>
public class Character : MonoBehaviour {

    public string characterName;
    public int cost;
    public int diamondCost;

    public CharacterAccessories[] accessories;

    public Animator animator;
    public Sprite icon;

    [Header("Sound")]
    public AudioClip jumpSound;
    public AudioClip hitSound;
    public AudioClip deathSound;

    /// <summary>
    /// 设置人物的装饰
    /// </summary>
    /// <param name="accessory"></param>
    public void SetupAccessory(int accessory)
    {
        //-1值为什么装饰都没有
        for(int i = 0; i < accessories.Length; ++i)
        {
            accessories[i].gameObject.SetActive(i == PlayerData.instance.usedAccessory);
        }
    }
}
