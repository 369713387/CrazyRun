using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当玩家死亡时，状态被推到游戏管理器的顶部
/// </summary>
public class GameOverState : AState
{
    public TrackManager trackManager;
    public Canvas canvas;
    public MissionUI missionPopup;

    public AudioClip gameOverTheme;
    //排行榜
    public Leaderboard miniLeaderboard;
    public Leaderboard fullLeaderboard;

    public GameObject addButton;

    protected bool m_CoinCredited = false;

    #region 实现父类的抽象函数
    public override void Enter(AState from)
    {
        canvas.gameObject.SetActive(true);

        miniLeaderboard.playerEntry.inputName.text = PlayerData.instance.previousName;

        miniLeaderboard.playerEntry.score.text = trackManager.score.ToString();
        miniLeaderboard.Populate();

        if (PlayerData.instance.AnyMissionsComplete())
        {
            missionPopup.Open();
        }
        else
        {
            missionPopup.gameObject.SetActive(false);
        }

        m_CoinCredited = false;

        CreditCoins();

        if(MusicPlayer.instance.GetStem(0) != gameOverTheme)
        {
            MusicPlayer.instance.SetStem(0, gameOverTheme);
            StartCoroutine(MusicPlayer.instance.RestartAllStems());
        }
    }

    public override void Exit(AState to)
    {
        canvas.gameObject.SetActive(false);
        FinishRun();
    }

    public override string GetName()
    {
        return "GameOver";
    }

    public override void Tick()
    {

    }
    #endregion

    #region 按钮事件
    /// <summary>
    /// 打开排行榜
    /// </summary>
    public void OpenLeaderboard()
    {
        fullLeaderboard.forcePlayerDisplay = false;
        fullLeaderboard.displayPlayer = true;
        fullLeaderboard.playerEntry.playerName.text = miniLeaderboard.playerEntry.inputName.text;
        fullLeaderboard.playerEntry.score.text = trackManager.score.ToString();

        fullLeaderboard.Open();
    }
    /// <summary>
    /// 打开商店界面
    /// </summary>
    public void GoToStore()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("shop", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
    /// <summary>
    /// 返回游戏人物选择的主菜单界面
    /// </summary>
    public void GoToLoadout()
    {
        trackManager.isRerun = false;
        manager.SwitchState("Loadout");
    }
    /// <summary>
    /// 重新进行游戏
    /// </summary>
    public void RunAgain()
    {
        trackManager.isRerun = false;
        manager.SwitchState("Game");
    }
    /// <summary>
    /// 货币结算
    /// </summary>
    protected void CreditCoins()
    {
        if (m_CoinCredited)
            return;

        //收集硬币
        PlayerData.instance.coins += trackManager.characterController.coins;
        PlayerData.instance.diamonds += trackManager.characterController.diamonds;

        PlayerData.instance.Save();

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "gameplay";
        var level = PlayerData.instance.rank.ToString();
        var itemType = "consumable";
        
        if (trackManager.characterController.coins > 0)
        {
            AnalyticsEvent.ItemAcquired(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                trackManager.characterController.coins,
                "fishbone",
                PlayerData.instance.coins,
                itemType,
                level,
                transactionId
            );
        }

        if (trackManager.characterController.premium > 0)
        {
            AnalyticsEvent.ItemAcquired(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                trackManager.characterController.premium,
                "anchovies",
                PlayerData.instance.premium,
                itemType,
                level,
                transactionId
            );
        }
#endif

        m_CoinCredited = true;
    }
    /// <summary>
    /// 停止奔跑，登记排行
    /// </summary>
    protected void FinishRun()
    {
        if (miniLeaderboard.playerEntry.inputName.text == "")
            miniLeaderboard.playerEntry.inputName.text = "Trash Cat";
        else
            PlayerData.instance.previousName = miniLeaderboard.playerEntry.inputName.text;

        PlayerData.instance.InsertScore(trackManager.score, miniLeaderboard.playerEntry.inputName.text);

        CharacterCollider.DeathEvent de = trackManager.characterController.characterCollider.deathData;
        //将数据注册到analysis中
#if UNITY_ANALYTICS
        AnalyticsEvent.GameOver(null, new Dictionary<string, object> {
            { "coins", de.coins },
            { "premium", de.premium },
            { "score", de.score },
            { "distance", de.worldDistance },
            { "obstacle",  de.obstacleType },
            { "theme", de.themeUsed },
            { "character", de.character },
        });
#endif

        PlayerData.instance.Save();

        trackManager.End();
    }
    #endregion
}
