using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Consumables", menuName = "Trash Dash/Consumables Database")]
public class ConsumableDatabase : ScriptableObject {

    public Consumable[] consumables;
    //消耗品类型和消耗品对应关系字典
    static protected Dictionary<Consumable.ConsumableType, Consumable> _consumablesDict;
    /// <summary>
    /// 往字典中添加数据
    /// </summary>
    public void Load()
    {
        if(_consumablesDict == null)
        {
            _consumablesDict = new Dictionary<Consumable.ConsumableType, Consumable>();

            for(int i = 0; i < consumables.Length; ++i)
            {
                _consumablesDict.Add(consumables[i].GetConsumableType(), consumables[i]);
            }
        }
    }
    /// <summary>
    /// 根据消耗品类型查找消耗品
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Consumable GetConsumable(Consumable.ConsumableType type)
    {
        Consumable c;
        return _consumablesDict.TryGetValue(type, out c) ? c : null;
    }

}
