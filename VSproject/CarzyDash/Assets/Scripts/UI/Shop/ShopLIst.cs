using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 商店模块基类
/// </summary>
public abstract class ShopLIst : MonoBehaviour {

    public GameObject prefabItem;
    public RectTransform listRoot;

    public delegate void RefreshCallback();

    protected RefreshCallback m_RefreshCallback;

    public void Open()
    {
        Populate();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        m_RefreshCallback = null;
    }

    public void Refresh()
    {
        m_RefreshCallback();
    }

    public abstract void Populate();
}
