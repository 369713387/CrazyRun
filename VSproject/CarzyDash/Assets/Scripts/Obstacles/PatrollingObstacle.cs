using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingObstacle : Obstacle
{
    static int s_SpeedRatioHash = Animator.StringToHash("SpeedRatio");
    static int s_DeadHash = Animator.StringToHash("Dead");

    [Tooltip("Minimum time to cross all lanes.")]
    public float minTime = 2f;
    [Tooltip("Maximum time to cross all lanes.")]
    public float maxTime = 5f;
    [Tooltip("Leave empty if no animation")]
    public Animator animator;

    public AudioClip[] patorllingSound;

    protected TrackSegment m_Segment;

    protected Vector3 m_OriginalPosition = Vector3.zero;
    protected float m_MaxSpeed;
    protected float m_CurrentPos;

    protected AudioSource m_Audio;
    protected bool m_Moving = true;

    protected const float k_LaneOffsetToFullWidth = 2f;

    public override void Spawn(TrackSegment segment, float t)
    {
        Vector3 position;
        Quaternion rotation;
        segment.GetPointAt(t, out position, out rotation);
        GameObject obj = Instantiate(gameObject, position, rotation);
        obj.transform.SetParent(segment.objectRoot, true);

        obj.GetComponent<PatrollingObstacle>().m_Segment = segment;
    }

    private void Start()
    {
        m_Audio = GetComponent<AudioSource>();
        if(m_Audio != null && patorllingSound != null && patorllingSound.Length > 0)
        {
            m_Audio.loop = true;
            m_Audio.clip = patorllingSound[Random.Range(0, patorllingSound.Length)];
            m_Audio.Play();
        }

        m_OriginalPosition = transform.localPosition + transform.right * m_Segment.manager.laneOffset;

        transform.localPosition = m_OriginalPosition;

        float actualTime = Random.Range(minTime, maxTime);
        //时间2，因为动画是前后移动的，所以我们需要在给定的时间内完成4车道的偏移速度

        m_MaxSpeed = (m_Segment.manager.laneOffset * k_LaneOffsetToFullWidth * 2) / actualTime;

        if(animator != null)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            animator.SetFloat(s_SpeedRatioHash, clip.length / actualTime);
        }
    }

    public override void Impacted()
    {
        m_Moving = false;
        base.Impacted();

        if(animator != null)
        {
            animator.SetTrigger(s_DeadHash);
        }
    }

    private void Update()
    {
        if (!m_Moving)
            return;

        m_CurrentPos += Time.deltaTime * m_MaxSpeed;

        transform.localPosition = m_OriginalPosition - transform.right * Mathf.PingPong(m_CurrentPos, m_Segment.manager.laneOffset * k_LaneOffsetToFullWidth);
    }
}
