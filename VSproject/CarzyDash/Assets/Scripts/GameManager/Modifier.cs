using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 快速的更改游戏的模式
/// </summary>
public class Modifier
{
    public virtual void OnRunStart(GameState state)
    {
    }
    public virtual void OnRunTick(GameState state)
    {

    }

    /// <summary>
    /// 如果游戏的某个屏幕应该被显示，则返回true。如果返回了false则会直接返回到主界面
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public virtual bool OnRunEnd(GameState state)
    {
        return true;
    }
}
//下面是一些简单的应该的例子
public class LimitedLengthRun : Modifier
{
    public float distance;

    public LimitedLengthRun(float dist)
    {
        distance = dist;
    }

    public override void OnRunTick(GameState state)
    {
        if (state.trackManager.worldDistance >= distance)
        {
            //state.trackManager.characterController.currentLife = 0;
        }
    }

    public override void OnRunStart(GameState state)
    {

    }

    public override bool OnRunEnd(GameState state)
    {
        state.QuitToLoadout();
        return false;
    }
}

public class SeededRun : Modifier
{
    int m_Seed;

    protected const int k_DaysInAWeek = 7;

    public SeededRun()
    {
        m_Seed = System.DateTime.Now.DayOfYear / k_DaysInAWeek;
    }

    public override void OnRunStart(GameState state)
    {
        state.trackManager.trackSeed = m_Seed;
    }

    public override bool OnRunEnd(GameState state)
    {
        state.QuitToLoadout();
        return false;
    }
}

public class SingleLifeRun : Modifier
{
    public override void OnRunTick(GameState state)
    {
        //if (state.trackManager.characterController.currentLife > 1)
        //state.trackManager.characterController.currentLife = 1;
    }


    public override void OnRunStart(GameState state)
    {

    }

    public override bool OnRunEnd(GameState state)
    {
        state.QuitToLoadout();
        return false;
    }
}
