using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraLife : Consumable
{
    protected const int k_MaxLives = 3;//最大生命个数
    protected const int k_CoinValue = 10;//一个心可以换多少个coin

    #region 实现父类继承的抽象函数
    public override string GetConsumableName()
    {
        return "Life";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.EXTRALIFE;
    }

    public override int GetPremiumCost()
    {
        return 5;
    }

    public override int GetPrice()
    {
        return 2000;
    }
    #endregion

    #region 实现父类的虚函数
    /// <summary>
    /// 额外生命值是否生效
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public override bool CanBeUsed(CharacterInputController c)
    {
        if(c.currentLife == c.maxLife)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 拾取额外生命道具后生效后的操作
    /// </summary>
    /// <param name="c"></param>
    public override void Started(CharacterInputController c)
    {
        base.Started(c);
        if(c.currentLife< k_MaxLives)
        {
            c.currentLife += 1;
        }
        else
        {
            c.coins += k_CoinValue;
        }
    }

    #endregion

}
