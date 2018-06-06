using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Consumable : MonoBehaviour {
    //持续时间
    public float duration;

    public enum ConsumableType
    {
        NONE,
        COIN_MAG,
        SCORE_MULTIPLAYER,
        INVINCIBILITY,
        EXTRALIFE,
        MAX_COUNT
    }

    public Sprite icon;
    public AudioClip activatedSound;
    public ParticleSystem activatedParticle;
    public bool canBeSpawned = true;

    public bool active { get { return m_Active; } }
    public float timeActive { get { return m_SinceStart; } }

    protected bool m_Active = true;
    protected float m_SinceStart;
    protected ParticleSystem m_ParticleSpawned;

    public abstract ConsumableType GetConsumableType();
    public abstract string GetConsumableName();
    public abstract int GetPrice();
    public abstract int GetPremiumCost();
    /// <summary>
    /// 重置开始时间
    /// </summary>
    public void ResetTime()
    {
        m_SinceStart = 0;
    }

    public virtual bool CanBeUsed(CharacterInputController c)
    {
        return true;
    }
    /// <summary>
    /// 消耗品开始使用前进行的操作
    /// </summary>
    /// <param name="c"></param>
    public virtual void Started(CharacterInputController c)
    {
        m_SinceStart = 0;

        if (activatedSound != null)
        {
            c.powerupSource.clip = activatedSound;
            c.powerupSource.Play();
        }

        if(activatedParticle != null)
        {
            m_ParticleSpawned = Instantiate(activatedParticle);
            if (!m_ParticleSpawned.main.loop)
            {
                Destroy(m_ParticleSpawned.gameObject, m_ParticleSpawned.main.duration);
            }

            m_ParticleSpawned.transform.SetParent(c.characterCollider.transform);
            m_ParticleSpawned.transform.localPosition = activatedParticle.transform.position;
        }
    }
    /// <summary>
    /// 消耗品使用过程中进行的操作
    /// </summary>
    /// <param name="c"></param>
    public virtual void Tick(CharacterInputController c)
    {
        m_SinceStart += Time.deltaTime;
        if (m_SinceStart >= duration)
        {
            m_Active = false;
            return;
        }
    }
    /// <summary>
    /// 消耗品使用结束后进行的操作
    /// </summary>
    /// <param name="c"></param>
    public virtual void Ended(CharacterInputController c)
    {
        if(m_ParticleSpawned != null)
        {
            if (activatedParticle.main.loop)
            {
                Destroy(m_ParticleSpawned.gameObject);
            }
        }
        //处理特效播放的声音
        if(activatedSound != null &&c.powerupSource.clip == activatedSound){
            c.powerupSource.Stop();//如果输入的特效声音是正在播放的那一个特效声音则停止播放
        }
        //
        for(int i = 0; i < c.consumables.Count; ++i)
        {
            //如果一个特效声音在播放着，输入另一个特效声音，则还是会播放原来的特效声音
            if(c.consumables[i].active && c.consumables[i].activatedSound != null)
            {
                c.powerupSource.clip = c.consumables[i].activatedSound;
                c.powerupSource.Play();
            }
        }
    }
}
