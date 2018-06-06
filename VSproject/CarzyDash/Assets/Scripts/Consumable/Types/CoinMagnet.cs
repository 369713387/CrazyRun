using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMagnet : Consumable
{
    protected readonly Vector3 k_HalfExtentsBox = new Vector3(20.0f, 1.0f, 1.0f);
    protected const int k_LayerMask = 1 << 8;

    #region 实现父类继承的抽象函数
    public override string GetConsumableName()
    {
        return "Magnet";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.COIN_MAG;
    }

    public override int GetPremiumCost()
    {
        return 0;
    }

    public override int GetPrice()
    {
        return 750;
    }
    #endregion

    protected Collider[] returnColls = new Collider[20];

    public override void Tick(CharacterInputController c)
    {
        base.Tick(c);
        //收集范围内的碰撞体集合，并返回相应的个数
        int nb = Physics.OverlapBoxNonAlloc(c.characterCollider.transform.position, k_HalfExtentsBox, returnColls,c.characterCollider.transform.rotation,k_LayerMask);

        for(int i = 0;i<nb; ++i)
        {
            Coin returnCoin = returnColls[i].GetComponent<Coin>();

            if(returnCoin != null && !returnCoin.isDiamond && !c.characterCollider.magnetCoins.Contains(returnCoin.gameObject))
            {
                returnColls[i].transform.SetParent(c.transform);
                c.characterCollider.magnetCoins.Add(returnColls[i].gameObject);
            }
        }

    }
}
