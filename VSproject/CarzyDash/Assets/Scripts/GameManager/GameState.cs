using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : AState
{

    protected static int s_DeadHash = Animator.StringToHash("Dead");

    public Canvas canvas;
    public TrackManager trackManager;
    public AudioClip gameTheme;

    [Header("UI")]
    public Text coinText;
    public Text diamondText;
    public Text scoreText;
    public Text distanceText;
    public Text multiplierText;
    public Text countdownText;
    public RectTransform powerupZone;
    public RectTransform lifeRectTransform;

    public RectTransform pauseMenu;
    public RectTransform wholeUI;
    public Button pauseButton;

    public Image inventoryIcon;

    public GameObject gameOverPopup;
    public Button premiumForLifeButton;
    public GameObject adsForLifeButton;
    public Text premiumCurrencyOwned;

    [Header("Prefabs")]
    public GameObject PowerupIconPrefab;

    public Modifier currentModifier = new Modifier();

    public string adsPlacementId = "rewardedVideo";

    public bool adsRewarded = true;

    protected bool m_Finished;
    protected float m_TimeSinceStart;
    protected List<PowerupIcon> m_PowerupIcons = new List<PowerupIcon>();
    protected Image[] m_LifeHearts;

    protected RectTransform m_CountdownRectTransform;
    protected bool m_WasMoving;

    protected bool m_AdsInitialised = false;
    protected bool m_GameoverSelectionDone = false;

    protected int k_MaxLives = 3;
    #region 父类抽象函数的实现
    public override void Enter(AState from)
    {
        m_CountdownRectTransform = countdownText.GetComponent<RectTransform>();

        m_LifeHearts = new Image[k_MaxLives];

        for (int i = 0; i < k_MaxLives; ++i)
        {
            m_LifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
        }

        //背景音乐管理器 TODO
        if (MusicPlayer.instance.GetStem(0) != gameTheme)
        {
            MusicPlayer.instance.SetStem(0, gameTheme);
            CoroutineHandler.StartStaticCoroutine(MusicPlayer.instance.RestartAllStems());
        }

        m_AdsInitialised = false;
        m_GameoverSelectionDone = false;

        StartGame();
    }

    public override void Exit(AState to)
    {
        canvas.gameObject.SetActive(false);

        ClearPowerup();
    }

    public override string GetName()
    {
        //正在游戏中
        return "Game";
    }

    public override void Tick()
    {
        if (m_Finished)
        {
            //游戏结束，检查广告是否播放完毕，如果广告还没播放完毕，允许显示关闭广告按钮
#if UNITY_ADS
            if (!m_AdsInitialised && Advertisement.IsReady(adsPlacementId))
            {
                adsForLifeButton.SetActive(true);
                m_AdsInitialised = true;
#if UNITY_ANALYTICS
                AnalyticsEvent.AdOffer(adsRewarded, adsNetwork, adsPlacementId, new Dictionary<string, object>
            {
                { "level_index", PlayerData.instance.rank },
                { "distance", TrackManager.instance == null ? 0 : TrackManager.instance.worldDistance },
            });
#endif
            }
            else if(!m_AdsInitialised)
                adsForLifeButton.SetActive(false);
#else
            adsForLifeButton.SetActive(false); //Ads is disabled
#endif

            return;
        }

        CharacterInputController chrCtrl = trackManager.characterController;

        m_TimeSinceStart += Time.deltaTime;

        if (chrCtrl.currentLife <= 0)
        {
            pauseButton.gameObject.SetActive(false);
            chrCtrl.CleanConsumable();
            chrCtrl.character.animator.SetBool(s_DeadHash, true);
            chrCtrl.characterCollider.koParticle.gameObject.SetActive(true);
            StartCoroutine(WaitForGameOver());
        }

        // 消耗品计时和生命管理
        List<Consumable> toRemove = new List<Consumable>();
        List<PowerupIcon> toRemoveIcon = new List<PowerupIcon>();

        for (int i = 0; i < chrCtrl.consumables.Count; ++i)
        {
            PowerupIcon icon = null;
            for (int j = 0; j < m_PowerupIcons.Count; ++j)
            {
                if (m_PowerupIcons[j].linkedConsumable == chrCtrl.consumables[i])
                {
                    icon = m_PowerupIcons[j];
                    break;
                }
            }

            chrCtrl.consumables[i].Tick(chrCtrl);
            if (!chrCtrl.consumables[i].active)
            {
                toRemove.Add(chrCtrl.consumables[i]);
                toRemoveIcon.Add(icon);
            }
            else if (icon == null)
            {
                // 如果没有可以用的消耗品prefabs则创建它
                GameObject o = Instantiate(PowerupIconPrefab);
                icon = o.GetComponent<PowerupIcon>();

                icon.linkedConsumable = chrCtrl.consumables[i];
                icon.transform.SetParent(powerupZone, false);

                m_PowerupIcons.Add(icon);
            }
        }

        for (int i = 0; i < toRemove.Count; ++i)
        {
            toRemove[i].Ended(trackManager.characterController);

            Destroy(toRemove[i].gameObject);
            if (toRemoveIcon[i] != null)
                Destroy(toRemoveIcon[i].gameObject);

            chrCtrl.consumables.Remove(toRemove[i]);
            m_PowerupIcons.Remove(toRemoveIcon[i]);
        }

        UpdateUI();

        currentModifier.OnRunTick(this);
    }

    #endregion

    #region 按钮事件
    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        //界面管理
        canvas.gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);
        wholeUI.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(true);
        gameOverPopup.SetActive(false);

        //再次进入游戏
        if (!trackManager.isRerun)
        {
            m_TimeSinceStart = 0;
            //trackManager.characterController.currentLifr = trackManager.characterController.maxLife;
        }

        currentModifier.OnRunStart(this);
        trackManager.Begin();

        m_Finished = false;
        m_PowerupIcons.Clear();
    }

    void OnApplicationPause(bool paseStatus)
    {
        if (paseStatus) Pause();
    }
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void Pause()
    {
        //检查当前状态是否可以暂停
        if (m_Finished || AudioListener.pause == true)
            return;

        AudioListener.pause = true;
        Time.timeScale = 0;

        pauseButton.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
        wholeUI.gameObject.SetActive(false);
        m_WasMoving = trackManager.isMoving;
        trackManager.StopMove();

    }
    /// <summary>
    /// 返回游戏
    /// </summary>
    public void Resume()
    {
        Time.timeScale = 1.0f;
        pauseButton.gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);
        wholeUI.gameObject.SetActive(true);
        if (m_WasMoving)
            trackManager.StartMove(false);

        AudioListener.pause = false;
    }
    /// <summary>
    /// 在暂停菜单界面可以通过点击按钮立刻返回到选择人物的主菜单界面（加载界面）
    /// </summary>
    public void QuitToLoadout()
    {
        //在pause菜单中使用，立即返回loadout，取消所有内容
        Time.timeScale = 1.0f;
        AudioListener.pause = false;
        trackManager.End();
        trackManager.isRerun = false;
        manager.SwitchState("Loadout");

    }
    /// <summary>
    /// 在游戏过程中，更新UI的显示
    /// </summary>
    protected void UpdateUI()
    {
        coinText.text = trackManager.characterController.coins.ToString();
        diamondText.text = trackManager.characterController.diamonds.ToString();

        for(int i = 0; i < 3; ++i)
        {
            if (trackManager.characterController.currentLife > i)
                m_LifeHearts[i].color = Color.white;
            else
                m_LifeHearts[i].color = Color.black;
        }

        scoreText.text = trackManager.score.ToString();
        multiplierText.text = "x" + trackManager.multiplier;
        distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString()+"m";

        if(trackManager.timeToStart >= 0)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
            m_CountdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.timeToStart - Mathf.Floor(trackManager.timeToStart)));
        }
        else
        {
            m_CountdownRectTransform.localScale = Vector3.zero;
        }

        //消耗品
        if (trackManager.characterController.inventory != null)
        {
            inventoryIcon.transform.parent.gameObject.SetActive(true);
            inventoryIcon.sprite = trackManager.characterController.inventory.icon;
        }
        else
            inventoryIcon.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator WaitForGameOver()
    {
        m_Finished = true;
        trackManager.StopMove();

        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);
        yield return new WaitForSeconds(2.0f);

        if (currentModifier.OnRunEnd(this))
        {
            if (trackManager.isRerun)
                manager.SwitchState("GameOver");
            else
                OpenGameOverPopup();
        }
    }

    /// <summary>
    /// 清空游戏道具
    /// </summary>
    protected void ClearPowerup()
    {
        for (int i = 0; i < m_PowerupIcons.Count; ++i)
        {
            if (m_PowerupIcons[i] != null)
            {
                GameObject.Destroy(m_PowerupIcons[i].gameObject);
            }
        }

        trackManager.characterController.powerupSource.Stop();

        m_PowerupIcons.Clear();
    }
    /// <summary>
    /// 死亡后，是否选择复活
    /// </summary>
    public void OpenGameOverPopup()
    {
        premiumForLifeButton.interactable = PlayerData.instance.diamonds>=3;

        premiumCurrencyOwned.text = PlayerData.instance.diamonds.ToString();

        ClearPowerup();

        gameOverPopup.SetActive(true);
    }
    /// <summary>
    /// 游戏结束事件
    /// </summary>
    public void GameOver()
    {
        manager.SwitchState("GameOver");
    }
    /// <summary>
    /// 使用钻石复活
    /// </summary>
    public void DiamondForLife()
    {
        if (m_GameoverSelectionDone)
            return;

        m_GameoverSelectionDone = true;

        PlayerData.instance.diamonds -= 3;
        SecondWind();
    }
    /// <summary>
    /// 复活后继续游戏
    /// </summary>
    public void SecondWind()
    {
        trackManager.characterController.currentLife = 1;
        trackManager.isRerun = true;
        StartGame();
    }
    /// <summary>
    /// 显示奖励广告
    /// </summary>
    public void ShowRewardedAd()
    {
        if (m_GameoverSelectionDone)
            return;

        m_GameoverSelectionDone = true;
#if UNITY_ADS
        if (Advertisement.IsReady(adsPlacementId))
        {
#if UNITY_ANALYTICS
            AnalyticsEvent.AdStart(adsRewarded, adsNetwork, adsPlacementId, new Dictionary<string, object>
            {
                { "level_index", PlayerData.instance.rank },
                { "distance", TrackManager.instance == null ? 0 : TrackManager.instance.worldDistance },
            });
#endif
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show(adsPlacementId, options);
        }
        else
        {
#if UNITY_ANALYTICS
            AnalyticsEvent.AdSkip(adsRewarded, adsNetwork, adsPlacementId, new Dictionary<string, object> {
                { "error", Advertisement.GetPlacementState(adsPlacementId).ToString() }
            });
#endif
        }
#else
        GameOver();
#endif
    }
    #endregion

    //广告
#if UNITY_ADS

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
#if UNITY_ANALYTICS
                AnalyticsEvent.AdComplete(adsRewarded, adsNetwork, adsPlacementId);
#endif
                SecondWind();
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
#if UNITY_ANALYTICS
                AnalyticsEvent.AdSkip(adsRewarded, adsNetwork, adsPlacementId);
#endif
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
#if UNITY_ANALYTICS
                AnalyticsEvent.AdSkip(adsRewarded, adsNetwork, adsPlacementId, new Dictionary<string, object> {
                    { "error", "failed" }
                });
#endif
                break;
        }
    }
#endif
}
