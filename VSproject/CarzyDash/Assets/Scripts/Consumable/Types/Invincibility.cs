using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invincibility : Consumable
{
    #region 实现父类继承的抽象函数
    public override string GetConsumableName()
    {
        return "Invincible";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.INVINCIBILITY;
    }

    public override int GetPremiumCost()
    {
        return 5;
    }

    public override int GetPrice()
    {
        return 1500;
    }
    #endregion

    #region 实现父类的虚函数
    /// <summary>
    /// 开始特效计时器
    /// </summary>
    /// <param name="c"></param>
    public override void Started(CharacterInputController c)
    {
        base.Started(c);
        c.characterCollider.SetInvincible(duration);       
    }
    /// <summary>
    /// 打开特效显示
    /// </summary>
    /// <param name="c"></param>
    public override void Tick(CharacterInputController c)
    {
        base.Tick(c);
        c.characterCollider.SetInvincibleExplicit(true);
    }
    /// <summary>
    /// 关闭特效显示
    /// </summary>
    /// <param name="c"></param>
    public override void Ended(CharacterInputController c)
    {
        base.Ended(c);
        c.characterCollider.SetInvincibleExplicit(false);
    }
    #endregion
}
