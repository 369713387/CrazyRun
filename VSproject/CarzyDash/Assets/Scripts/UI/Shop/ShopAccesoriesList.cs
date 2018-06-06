using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopAccesoriesList : ShopLIst {
    //填充显示的内容,人物装饰列表
    public GameObject headerPrefab;
    public override void Populate()
    {
        m_RefreshCallback = null;

        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        foreach (KeyValuePair<string,Character> pair in CharacterDatabase.dictionary)
        {
            Character c = pair.Value;
            if(c != null && c.accessories !=null && c.accessories.Length > 0)
            {
                GameObject header = Instantiate(headerPrefab);
                header.transform.SetParent(listRoot, false);
                ShopItemListItem itemHeader = header.GetComponent<ShopItemListItem>();
                itemHeader.nameText.text = c.characterName;

                for (int i = 0; i < c.accessories.Length; ++i) {
                    CharacterAccessories accessory = c.accessories[i];
                    GameObject newEntry = Instantiate(prefabItem);
                    newEntry.transform.SetParent(listRoot, false);

                    ShopItemListItem item = newEntry.GetComponent<ShopItemListItem>();

                    string compoundName = c.characterName + ":" + accessory.accessoryName;

                    item.nameText.text = accessory.accessoryName;
                    item.pricetext.text = accessory.cost.ToString();
                    item.icon.sprite = accessory.accessoryIcon;
                    item.buyButton.image.sprite = item.buyButtonSprite;

                    if(accessory.diamondCost > 0)
                    {
                        item.diamondText.transform.parent.gameObject.SetActive(true);
                        item.diamondText.text = accessory.diamondCost.ToString();
                    }
                    else
                    {
                        item.diamondText.transform.parent.gameObject.SetActive(false);
                    }

                    item.buyButton.onClick.AddListener(delegate () { Buy(compoundName, accessory.cost, accessory.diamondCost); });

                    m_RefreshCallback += delegate () { RefreshButton(item, accessory, compoundName); };
                    RefreshButton(item, accessory, compoundName);
                }
            }
        }
    }

    protected void RefreshButton(ShopItemListItem item, CharacterAccessories accessory, string compoundName)
    {
        if(accessory.cost > PlayerData.instance.coins)
        {
            item.buyButton.interactable = false;
            item.pricetext.color = Color.red;
        }
        else
        {
            item.pricetext.color = Color.black;
        }

        if (accessory.diamondCost > PlayerData.instance.diamonds)
        {
            item.buyButton.interactable = false;
            item.diamondText.color = Color.red;
        }
        else
        {
            item.diamondText.color = Color.black;
        }

        if (PlayerData.instance.characterAccessories.Contains(compoundName))
        {
            item.buyButton.interactable = false;
            item.buyButton.image.sprite = item.disabledButtonSprite;
            item.buyButton.transform.GetChild(0).GetComponent<Text>().text = "Owned";
        }
    }


    public void Buy(string name,int cost,int diamondsCost)
    {
        PlayerData.instance.coins -= cost;
        PlayerData.instance.diamonds -= diamondsCost;
        PlayerData.instance.AddAccessory(name);
        PlayerData.instance.Save();

#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = name;
        var itemType = "non_consumable";
        var itemQty = 1;

        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Soft,
            transactionContext,
            itemQty,
            itemId,
            itemType,
            level,
            transactionId
        );
        
        if (cost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                cost,
                itemId,
                PlayerData.instance.coins, // Balance
                itemType,
                level,
                transactionId
            );
        }

        if (premiumCost > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                premiumCost,
                itemId,
                PlayerData.instance.premium, // Balance
                itemType,
                level,
                transactionId
            );
        }
#endif

        Refresh();
    }
}
