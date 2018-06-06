using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputController : MonoBehaviour
{

    static int s_DeadHash = Animator.StringToHash("Dead");
    static int s_RunStartHash = Animator.StringToHash("runStart");
    static int s_MovingHash = Animator.StringToHash("Moving");
    static int s_JumpingHash = Animator.StringToHash("Jumping");
    static int s_JumpingSpeedHash = Animator.StringToHash("JumpSpeed");
    static int s_SlidingHash = Animator.StringToHash("Sliding");

    public TrackManager trackManager;
    public Character character;
    public CharacterCollider characterCollider;
    public GameObject blobShadow;
    public float laneChangeSpeed = 1.0f;

    public int maxLife = 3;

    public Consumable inventory;

    public int coins { get { return m_Coins; } set { m_Coins = value; } }
    public int diamonds { get { return m_Diamonds; } set { m_Diamonds = value; } }
    public int currentLife { get { return m_CurrentLife; } set { m_CurrentLife = value; } }
    public List<Consumable> consumables { get { return m_ActiveConsumables; } }
    public bool isJumping { get { return m_Jumping; } }
    public bool isSliding { get { return m_Sliding; } }

    [Header("Controls")]
    public float jumpLength = 2.0f;     // Distance jumped
    public float jumpHeight = 1.2f;

    public float slideLength = 2.0f;

    [Header("Sounds")]
    public AudioClip slideSound;
    public AudioClip powerUpUseSound;
    public AudioSource powerupSource;

    protected int m_Coins;
    protected int m_Diamonds;
    protected int m_CurrentLife;

    protected List<Consumable> m_ActiveConsumables = new List<Consumable>();

    protected int m_ObstacleLayer;

    protected bool m_IsInvincible;

    protected float m_JumpStart;
    protected bool m_Jumping;

    protected bool m_Sliding;
    protected float m_SlideStart;

    protected AudioSource m_Audio;

    protected int m_CurrentLane = k_StartingLane;
    protected Vector3 m_TargetPosition = Vector3.zero;

    protected readonly Vector3 k_StartingPosition = Vector3.forward * 2f;

    protected const int k_StartingLane = 1;
    protected const float k_GroundingSpeed = 80f;
    protected const float k_ShadowRaycastDistance = 100f;
    protected const float k_ShadowGroundOffset = 0.01f;
    protected const float k_TrackSpeedToJumpAnimSpeedRatio = 0.6f;
    protected const float k_TrackSpeedToSlideAnimSpeedRatio = 0.9f;

    protected void Awake()
    {
        m_Diamonds = 0;
        m_CurrentLife = 0;
        m_Sliding = false;
        m_SlideStart = 0.0f;
    }

#if !UNITY_STANDALONE
    protected Vector2 m_StartingTouch;
    protected bool m_IsSwiping = false;
#endif

    /// <summary>
    /// 用于测试
    /// </summary>
    /// <param name="invincible"></param>
    public void CheatInvincible(bool invincible)
    {
        m_IsInvincible = invincible;
    }

    public bool IsCheatInvincible()
    {
        return m_IsInvincible;
    }
    /// <summary>
    /// 初始化参数
    /// </summary>
    public void Init()
    {
        transform.position = k_StartingPosition;
        m_TargetPosition = Vector3.zero;

        m_CurrentLane = k_StartingLane;
        characterCollider.transform.localPosition = Vector3.zero;

        currentLife = maxLife;

        m_Audio = GetComponent<AudioSource>();

        m_ObstacleLayer = 1 << LayerMask.NameToLayer("Obstacle");
    }

    /// <summary>
    /// 在开始奔跑或者复活的时候调用
    /// </summary>
    public void Begin()
    {
        character.animator.SetBool(s_DeadHash, false);

        characterCollider.Init();

        m_ActiveConsumables.Clear();
    }
    /// <summary>
    /// 消耗品使用结束（时间到了）
    /// </summary>
    public void End()
    {
        CleanConsumable();
    }

    public void CleanConsumable()
    {
        for (int i = 0; i < m_ActiveConsumables.Count; ++i)
        {
            m_ActiveConsumables[i].Ended(this);
            Destroy(m_ActiveConsumables[i].gameObject);
        }

        m_ActiveConsumables.Clear();
    }

    public void StartRunning()
    {
        if (character.animator)
        {
            character.animator.Play(s_RunStartHash);
            character.animator.SetBool(s_MovingHash, true);
        }
    }

    public void StopMoving()
    {
        trackManager.StopMove();
        if (character.animator)
        {
            character.animator.SetBool(s_MovingHash, false);
        }
    }

    protected void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        //在电脑上调试时的控制操作
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!m_Sliding)
                Slide();
        }

#else
        //在手机上面的控制操作
        if(Input.touchCount == 1)
        {
            //滑动过程中
            if (m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;
                //在屏幕比例上有差异，但是只使用宽度，所以在两个轴上的比例都是一样的(否则我们就必须垂直滑动…)
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.height);

                if(diff.magnitude > 0.01f)
                {
                    //滑动距离达到屏幕的宽度的1%，即可触发操作效果
                    if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (diff.y < 0)
                            Slide();
                        else
                            Jump();
                    }
                    else
                    {
                        if (diff.x < 0)
                            ChangeLane(-1);
                        else
                            ChangeLane(1);
                    }

                    m_IsSwiping = false;
                }
            }
            //开始滑动
            if(Input.GetTouch(0).phase == TouchPhase.Began)
            {
                m_StartingTouch = Input.GetTouch(0).position;
                m_IsSwiping = true;
            }
            else if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                //滑动结束
                m_IsSwiping = false;
            }
        }
#endif

        //输入检查是在swip测试之后，如果是TouchPhase。结束时，在开始阶段之后，一个帧会被注册(否则，m_IsSwiping将被设置为false，并且测试不会发生在那个开始结束的一对中)
        Vector3 verticalTargetPosition = m_TargetPosition;

        if (m_Sliding)
        {
            //滑动时间不是恒定的，但滑动的长度是(即使稍微改变速度，在更快的情况下略微滑动)。这是为了游戏的原因，我们不希望角色在最大速度的时候拖得更远。
            float correctSlideLength = slideLength * (1.0f + trackManager.speedRatio);
            float ratio = (trackManager.worldDistance - m_SlideStart) / correctSlideLength;
            if (ratio >= 1.0f)
            {
                //我们滑到恒定的长度后，回到跑步状态
                StopSliding();
            }
        }

        if (m_Jumping)
        {
            if (trackManager.isMoving)
            {
                //和滑动一样，我们想要一个固定的跳跃长度，而不是固定的跳跃时间。同样，就像滑动一样，我们稍微修改了长度和速度，使它更可玩。
                float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
                float ratio = (trackManager.worldDistance - m_JumpStart) / correctJumpLength;
                if (ratio >= 1.0f)
                {
                    m_Jumping = false;
                    character.animator.SetBool(s_JumpingHash, false);
                }
                else
                {
                    verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
                }
            }
            else if (!AudioListener.pause)
            {
                verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, k_GroundingSpeed * Time.deltaTime);
                if (Mathf.Approximately(verticalTargetPosition.y, 0f))
                {
                    character.animator.SetBool(s_JumpingHash, false);
                    m_Jumping = false;
                }
            }
        }

        characterCollider.transform.localPosition = Vector3.MoveTowards(characterCollider.transform.localPosition, verticalTargetPosition, laneChangeSpeed * Time.deltaTime);

        //将阴影置于角色下方
        RaycastHit hit;
        if (Physics.Raycast(characterCollider.transform.position + Vector3.up, Vector3.down, out hit, k_ShadowRaycastDistance, m_ObstacleLayer))
        {
            blobShadow.transform.position = hit.point + Vector3.up * k_ShadowGroundOffset;
        }
        else
        {
            Vector3 shadowPosition = characterCollider.transform.position;
            shadowPosition.y = k_ShadowGroundOffset;
            blobShadow.transform.position = shadowPosition;
        }

    }

    public void Jump()
    {
        if (!m_Jumping)
        {
            if (m_Sliding)
                StopSliding();

            float correctJumpLength = jumpLength * (1.0f + trackManager.speedRatio);
            m_JumpStart = trackManager.worldDistance;
            float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (trackManager.speed / correctJumpLength);

            character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
            character.animator.SetBool(s_JumpingHash, true);
            m_Audio.PlayOneShot(character.jumpSound);
            m_Jumping = true;
        }
    }

    public void Slide()
    {
        if (!m_Sliding && !m_Jumping)
        {
            float correctSlideLength = slideLength * (1.0f + trackManager.speedRatio);
            m_SlideStart = trackManager.worldDistance;
            float animSpeed = k_TrackSpeedToJumpAnimSpeedRatio * (trackManager.speed / correctSlideLength);

            character.animator.SetFloat(s_JumpingSpeedHash, animSpeed);
            character.animator.SetBool(s_SlidingHash, true);
            m_Audio.PlayOneShot(slideSound);
            m_Sliding = true;

            characterCollider.Slide(true);
        }
    }

    public void StopSliding()
    {
        if (m_Sliding)
        {
            character.animator.SetBool(s_SlidingHash, false);
            m_Sliding = false;

            characterCollider.Slide(false);
        }
    }

    public void ChangeLane(int direction)
    {
        if (!trackManager.isMoving)
            return;

        int targetLane = m_CurrentLane + direction;

        if (targetLane < 0 || targetLane > 2)
        {
            //忽略我们在边界的情况
            return;
        }

        m_CurrentLane = targetLane;
        m_TargetPosition = new Vector3((m_CurrentLane - 1) * trackManager.laneOffset, 0, 0);
    }

    public void UseInventory()
    {
        if (inventory != null && inventory.CanBeUsed(this))
        {
            UseConsumable(inventory);
            inventory = null;
        }
    }

    public void UseConsumable(Consumable c)
    {
        characterCollider.audio.PlayOneShot(powerUpUseSound);

        for (int i = 0; i < m_ActiveConsumables.Count; ++i)
        {
            if (m_ActiveConsumables[i].GetType() == c.GetType())
            {
                //在道具使用期间重复使用道具，只需要重置道具的持续时长即可
                m_ActiveConsumables[i].ResetTime();
                Destroy(c.gameObject);
                return;
            }
        }

        //激活想要使用的道具
        c.transform.SetParent(transform, false);
        c.gameObject.SetActive(false);

        m_ActiveConsumables.Add(c);
        c.Started(this);
    }
}
