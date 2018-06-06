using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
using UnityEngine.Analytics.Experimental;
#endif

public class ShopUI : MonoBehaviour {

    public ConsumableDatabase consumableDatabase;

    public ShopItemList itemList;
    public ShopCharacterList characterList;
    public ShopAccesoriesList accessoriesList;

    [Header("UI")]
    public Text coinCounter;
    public Text premiumCounter;

    protected ShopLIst m_OpenList;//当前打开的界面
    void Start () {
        PlayerData.Create();

        consumableDatabase.Load();
        AssetBundlesDatabaseHandler.Load();

#if UNITY_ANALYTICS
        AnalyticsEvent.StoreOpened(StoreType.Soft);
#endif
        m_OpenList = itemList;
        itemList.Open();
    }
	void Update () {
        //从数据库更新数据
        coinCounter.text = PlayerData.instance.coins.ToString();
        premiumCounter.text = PlayerData.instance.diamonds.ToString();
	}

    public void OpenItemList()
    {
        m_OpenList.Close();
        itemList.Open();
        m_OpenList = itemList;
    }

    public void OpenCharacterList()
    {
        m_OpenList.Close();
        characterList.Open();
        m_OpenList = characterList;
    }

    public void OpenAccessoriesList()
    {
        m_OpenList.Close();
        accessoriesList.Open();
        m_OpenList = accessoriesList;
    }

    public void LoadScene(string scenename)
    {
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }

    public void CloseScene()
    {
        SceneManager.UnloadSceneAsync("shop");
        LoadoutState loadoutState = GameManager.instance.topState as LoadoutState;
        if(loadoutState != null)
        {
            loadoutState.Refresh();
        }
    }
}
