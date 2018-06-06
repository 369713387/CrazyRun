using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Socre2Multiplier : Consumable
{
    #region 实现父类继承的抽象函数
    public override string GetConsumableName()
    {
        return "x2";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.SCORE_MULTIPLAYER;
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

    #region 复写父类的虚函数
    public override void Started(CharacterInputController c)
    {
        base.Started(c);
        m_SinceStart = 0;
        c.trackManager.modifyMultiply += MultiplyModify;
    }

    public override void Ended(CharacterInputController c)
    {
        base.Ended(c);

        c.trackManager.modifyMultiply -= MultiplyModify;
    }
    #endregion
    /// <summary>
    /// 实现分数加倍
    /// </summary>
    /// <param name="multi"></param>
    /// <returns></returns>
    protected int MultiplyModify(int multi)
    {
        return multi * 2;
    }
}
