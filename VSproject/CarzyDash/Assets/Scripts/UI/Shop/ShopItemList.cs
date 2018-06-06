using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
using UnityEngine.Analytics.Experimental;
#endif

public class ShopItemList : ShopLIst {
    static public Consumable.ConsumableType[] s_ConsumablesTypes = System.Enum.GetValues(typeof(Consumable.ConsumableType)) as Consumable.ConsumableType[];

    //填充显示的内容，道具
    public override void Populate()
    {
        m_RefreshCallback = null;
        foreach (Transform t in listRoot)
        {
            Destroy(t.gameObject);
        }

        for(int i = 0; i < s_ConsumablesTypes.Length; ++i)
        {
            Consumable c = ConsumableDatabase.GetConsumable(s_ConsumablesTypes[i]);
            if(c != null)
            {
                GameObject newEntry = Instantiate(prefabItem);
                newEntry.transform.SetParent(listRoot, false);

                ShopItemListItem item = newEntry.GetComponent<ShopItemListItem>();

                item.buyButton.image.sprite = item.buyButtonSprite;

                item.nameText.text = c.GetConsumableName();
                item.pricetext.text = c.GetPrice().ToString();

                if(c.GetPremiumCost() > 0)
                {
                    item.diamondText.transform.parent.gameObject.SetActive(true);
                    item.diamondText.text = c.GetPremiumCost().ToString();
                }
                else
                {
                    item.diamondText.transform.parent.gameObject.SetActive(false);
                }

                item.icon.sprite = c.icon;

                item.countText.gameObject.SetActive(true);

                item.buyButton.onClick.AddListener(delegate () { Buy(c); });
                m_RefreshCallback += delegate () { RefreshButton(item, c); };
                RefreshButton(item, c);
                
            }
        }
    }

    protected void RefreshButton(ShopItemListItem itemList, Consumable consumable)
    {
        int count = 0;
        PlayerData.instance.consumables.TryGetValue(consumable.GetConsumableType(), out count);
        itemList.countText.text = count.ToString();

        if(consumable.GetPrice() > PlayerData.instance.coins)
        {
            //金币不足
            itemList.buyButton.interactable = false;
            itemList.pricetext.color = Color.red;
        }
        else
        {
            itemList.pricetext.color = Color.black;
        }

        if (consumable.GetPremiumCost() > PlayerData.instance.diamonds)
        {
            //钻石不足
            itemList.buyButton.interactable = false;
            itemList.diamondText.color = Color.red;
        }
        else
        {
            itemList.pricetext.color = Color.black;
        }
    }

    public void Buy(Consumable consumable)
    {
        PlayerData.instance.coins -= consumable.GetPrice();
        PlayerData.instance.diamonds -= consumable.GetPremiumCost();
        PlayerData.instance.Add(consumable.GetConsumableType());
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
