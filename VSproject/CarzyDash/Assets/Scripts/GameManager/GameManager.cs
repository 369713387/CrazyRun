using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 游戏管理器是一个状态机，根据当前的游戏状态，它将在状态之间切换。
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public static GameManager instance { get { return s_Instance; } }
    protected static GameManager s_Instance;

    public AState[] states;
    //返回栈顶的游戏状态
    public AState topState { get { if (m_StateStack.Count == 0) return null; return m_StateStack[m_StateStack.Count - 1]; } }
    //消耗品数据库
    public ConsumableDatabase m_consumableDatabase;
    //游戏状态栈
    protected List<AState> m_StateStack = new List<AState>();
    //游戏状态字典
    protected Dictionary<string, AState> m_StateDict = new Dictionary<string, AState>();
    //函数执行顺序：awake onenable start
    protected void OnEnable()
    {
        PlayerData.Create();
        s_Instance = this;

        m_consumableDatabase.Load();
        m_StateDict.Clear();

        if (states.Length == 0)
        {
            return;
        }
        //初始化游戏状态字典
        for (int i = 0; i < states.Length; ++i)
        {
            states[i].manager = this;
            m_StateDict.Add(states[i].GetName(), states[i]);
        }

        m_StateStack.Clear();
        PushState(states[0].GetName());
    }

    protected void Update()
    {
        if (m_StateStack.Count > 0)
        {
            m_StateStack[m_StateStack.Count - 1].Tick();
        }
    }

    #region 游戏状态管理
    /// <summary>
    /// 游戏状态间的切换
    /// </summary>
    /// <param name="newState"></param>
    public void SwitchState(string newState)
    {
        AState state = FindState(newState);
        if(state == null)
        {
            Debug.LogError("Can't find the state named " + newState);
            return;
        }
        
        m_StateStack[m_StateStack.Count - 1].Exit(state);
        state.Enter(m_StateStack[m_StateStack.Count - 1]);
        m_StateStack.RemoveAt(m_StateStack.Count - 1);
        m_StateStack.Add(state);
    }
    /// <summary>
    /// 返回上一步游戏状态，并移除栈顶游戏状态
    /// </summary>
    public void PopState()
    {
        if (m_StateStack.Count < 2)
        {
            Debug.LogError("Can't pop states, only one in stack.");
            return;
        }

        m_StateStack[m_StateStack.Count - 1].Exit(m_StateStack[m_StateStack.Count - 2]);
        m_StateStack[m_StateStack.Count - 2].Enter(m_StateStack[m_StateStack.Count - 2]);
        m_StateStack.RemoveAt(m_StateStack.Count - 1);

    }
    /// <summary>
    /// 进入新的游戏状态，并将其入栈
    /// </summary>
    /// <param name="name"></param>
    public void PushState(string name)
    {
        AState state;
        if(!m_StateDict.TryGetValue(name,out state))
        {
            Debug.LogError("Can't find the state named " + name);
            return;
        }

        if (m_StateStack.Count > 0)
        {
            m_StateStack[m_StateStack.Count - 1].Exit(state);
            state.Enter(m_StateStack[m_StateStack.Count - 1]);
        }
        else
        {
            state.Enter(null);
        }
        m_StateStack.Add(state);
    }

    /// <summary>
    /// 在字典中查找游戏状态
    /// </summary>
    /// <param name="statename"></param>
    /// <returns></returns>
    public AState FindState(string statename)
    {
        AState state;
        if (!m_StateDict.TryGetValue(statename, out state)){
            return null;
        }
        else
        {
            return state;
        }
    }
    #endregion
}
/// <summary>
/// 游戏状态，用栈结构来存放
/// </summary>
public abstract class AState:MonoBehaviour
{
    [HideInInspector]
    public GameManager manager;
    /// <summary>
    /// 进入目标游戏状态前要执行的操作
    /// </summary>
    /// <param name="from">从哪个状态进入</param>
    public abstract void Enter(AState from);
    /// <summary>
    /// 离开目标游戏状态后要执行的操作
    /// </summary>
    /// <param name="to"></param>
    public abstract void Exit(AState to);
    /// <summary>
    /// 在目标游戏状态中要执行的操作
    /// </summary>
    public abstract void Tick();
    /// <summary>
    /// 获取游戏状态名（返回字符串）
    /// </summary>
    /// <returns></returns>
    public abstract string GetName();
}
