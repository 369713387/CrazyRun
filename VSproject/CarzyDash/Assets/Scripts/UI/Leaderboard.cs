using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour {

    public RectTransform entriesRoot;
    public int entriesCount;

    public HighscoreUI playerEntry;
    public bool forcePlayerDisplay;
    public bool displayPlayer = true;

    public void Open()
    {
        gameObject.SetActive(true);

        Populate();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Populate()
    {
        //先让所有的条目都启用，然后再让玩家进入。
        playerEntry.transform.SetAsLastSibling();
        for(int i = 0; i < entriesCount; ++i)
        {
            entriesRoot.GetChild(i).gameObject.SetActive(true);
        }

        //查找本地页面空间中的所有索引。
        int localStart = 0;
        int place = -1;
        int localPlace = -1;

        if (displayPlayer)
        {
            place = PlayerData.instance.GetScorePlace(int.Parse(playerEntry.score.text));
            localPlace = place - localStart;
        }

        if(localPlace >= 0 && localPlace < entriesCount && displayPlayer)
        {
            playerEntry.gameObject.SetActive(true);
            playerEntry.transform.SetSiblingIndex(localPlace);
        }

        if (!forcePlayerDisplay || PlayerData.instance.highscores.Count < entriesCount)
            entriesRoot.GetChild(entriesRoot.transform.childCount - 1).gameObject.SetActive(false);

        int currentHighScore = localStart;

        for(int i = 0; i < entriesCount; ++i)
        {
            HighscoreUI hs = entriesRoot.GetChild(i).GetComponent<HighscoreUI>();

            if(hs == playerEntry || hs == null)
            {
                //跳过玩家条目
                continue;
            }

            if(PlayerData.instance.highscores.Count > currentHighScore)
            {
                hs.gameObject.SetActive(true);
                hs.playerName.text = PlayerData.instance.highscores[currentHighScore].name;
                hs.number.text = (localStart + i + 1).ToString();
                hs.score.text = PlayerData.instance.highscores[currentHighScore].score.ToString();

                currentHighScore++;
            }
            else
            {
                hs.gameObject.SetActive(false);
            }

            //如果我们迫使玩家被展示，即使它在其他地方被禁用，我们也可以启用它
            if (forcePlayerDisplay)
                playerEntry.gameObject.SetActive(true);

            playerEntry.number.text = (place + 1).ToString();
        }

    }
}
