using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个脚本是实现障碍的基类。派生类应该负责生成障碍所需的任何对象。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public abstract class Obstacle : MonoBehaviour {

    public AudioClip impactedSound;

    public abstract void Spawn(TrackSegment segment, float t);

    public virtual void Impacted()
    {
        Animation anim = GetComponentInChildren<Animation>();
        AudioSource audioSource = GetComponent<AudioSource>();

        if(anim != null)
        {
            anim.Play();
        }

        if(audioSource != null && impactedSound != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = impactedSound;
            audioSource.Play();
        }
    }
}
