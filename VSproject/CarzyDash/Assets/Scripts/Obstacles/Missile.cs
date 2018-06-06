using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 当player离得足够近的时候，障碍就会开始向前移动。
/// </summary>
public class Missile : Obstacle {
    static int s_DeathHash = Animator.StringToHash("Death");
    static int s_RunHash = Animator.StringToHash("Run");

    public Animator animator;
    public AudioClip[] movingSound;

    protected TrackSegment m_OwnSegement;

    protected bool m_IsMoving;
    protected AudioSource m_Audio;

    protected const int k_LeftMostLaneIndex = -1;
    protected const int k_RightMostLaneIndex = 1;
    protected const float k_Speed = 5f;

    public void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
    }

    public override void Spawn(TrackSegment segment, float t)
    {
        int lane = Random.Range(k_LeftMostLaneIndex, k_RightMostLaneIndex + 1);

        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);

        GameObject obj = Instantiate(gameObject, position, rotation);
        obj.transform.SetParent(segment.objectRoot, true);
        obj.transform.position += obj.transform.right * lane * segment.manager.laneOffset;

        obj.transform.forward = -obj.transform.forward;

        obj.GetComponent<Missile>().m_OwnSegement = segment;
    }

    public override void Impacted()
    {
        base.Impacted();

        if(animator != null)
        {
            animator.SetTrigger(s_DeathHash);
        }
    }

    public void Update()
    {
        if (m_IsMoving)
            transform.position += transform.forward * k_Speed * Time.deltaTime;
        else
        {
            if (TrackManager.instance.segments[1] == m_OwnSegement)
            {
                if(animator != null)
                {
                    animator.SetTrigger(s_RunHash);
                }

                if(m_Audio !=null && movingSound != null && movingSound.Length > 0)
                {
                    m_Audio.clip = movingSound[Random.Range(0, movingSound.Length)];
                    m_Audio.Play();
                    m_Audio.loop = true;
                }

                m_IsMoving = true;
            }
        }
    }
}
