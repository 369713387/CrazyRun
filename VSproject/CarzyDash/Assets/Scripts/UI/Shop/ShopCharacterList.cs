using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
using UnityEngine.Analytics.Experimental;
#endif

public class ShopCharacterList : ShopLIst {
    //填充显示的内容,人物角色列表
    public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        foreach (KeyValuePair<string,Character> pair in CharacterDatabase.dictionary)
        {
            Character ch = pair.Value;
            if(ch != null)
            {
                GameObject newEntry = Instantiate(prefabItem);
                newEntry.transform.SetParent(listRoot, false);

                ShopItemListItem item = newEntry.GetComponent<ShopItemListItem>();

                item.icon.sprite = ch.icon;
                item.nameText.text = ch.characterName;
                item.pricetext.text = ch.cost.ToString();

                item.buyButton.image.sprite = item.buyButtonSprite;

                if(ch.diamondCost > 0)
                {
                    item.diamondText.transform.parent.gameObject.SetActive(true);
                    item.diamondText.text = ch.diamondCost.ToString();
                }
                else
                {
                    item.diamondText.transform.parent.gameObject.SetActive(false);
                }

                item.buyButton.onClick.AddListener(delegate () { Buy(ch); });

                m_RefreshCallback += delegate () { RefreshButton(item, ch); };
                RefreshButton(item, ch);
            }
        }
    }

    protected void RefreshButton(ShopItemListItem item, Character ch)
    {
        if (ch.cost > PlayerData.instance.coins)
        {
            item.buyButton.interactable = false;
            item.pricetext.color = Color.red;
        }
        else
        {
            item.pricetext.color = Color.black;
        }

        if (ch.diamondCost > PlayerData.instance.diamonds)
        {
            item.buyButton.interactable = false;
            item.diamondText.color = Color.red;
        }
        else
        {
            item.diamondText.color = Color.black;
        }

        if (PlayerData.instance.characters.Contains(ch.characterName))
        {
            item.buyButton.interactable = false;
            item.buyButton.image.sprite = item.disabledButtonSprite;
            item.buyButton.transform.GetChild(0).GetComponent<Text>().text = "Owend";
        }
    }

    public void Buy(Character ch)
    {
        PlayerData.instance.coins -= ch.cost;
        PlayerData.instance.diamonds -= ch.diamondCost;
        PlayerData.instance.AddCharacter(ch.characterName);
        PlayerData.instance.Save();
#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        var transactionId = System.Guid.NewGuid().ToString();
        var transactionContext = "store";
        var level = PlayerData.instance.rank.ToString();
        var itemId = c.GetConsumableName();
        var itemType = "consumable";
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
        
        if (c.GetPrice() > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Soft, // Currency type
                transactionContext,
                c.GetPrice(),
                itemId,
                PlayerData.instance.coins, // Balance
                itemType,
                level,
                transactionId
            );
        }

        if (c.GetPremiumCost() > 0)
        {
            AnalyticsEvent.ItemSpent(
                AcquisitionType.Premium, // Currency type
                transactionContext,
                c.GetPremiumCost(),
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
