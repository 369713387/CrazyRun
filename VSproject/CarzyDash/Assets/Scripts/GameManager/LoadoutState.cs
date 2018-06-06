using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutState : AState
{
    public Canvas inventoryCanvas;

    [Header("Char UI")]
    public Text charNameDisplay;
    public RectTransform charSelect;
    public Transform charPosition;

    [Header("Theme UI")]
    public Text themeNameDisplay;
    public RectTransform themeSelect;
    public Image themeIcon;

    [Header("PowerUp UI")]
    public RectTransform powerupSelect;
    public Image powerupIcon;
    public Text powerupCount;
    public Sprite noItemIcon;

    [Header("Accessory UI")]
    public RectTransform accessoriesSelector;
    public Text accesoryNameDisplay;
    public Image accessoryIconDisplay;

    [Header("Other Data")]
    //排行榜
    public Leaderboard leaderboard;
    //任务列表
    public MissionUI missionPopup;
    public Button runButton;

    public MeshFilter skyMeshFilter;
    public MeshFilter UIGroundFilter;

    public AudioClip menuTheme;


    [Header("Prefabs")]
    public ConsumableIcon consumableIcon;

    Consumable.ConsumableType m_PowerupToUse = Consumable.ConsumableType.NONE;

    protected GameObject m_Character;
    protected List<int> m_OwnedAccesories = new List<int>();
    protected int m_UsedAccessory = -1;
    protected int m_UsedPowerupIndex;
    protected bool m_IsLoadingCharacter;

    protected Modifier m_CurrentModifier = new Modifier();

    protected const float k_CharacterRotationSpeed = 45f;
    protected const string k_ShopSceneName = "shop";
    protected const float k_OwnedAccessoriesCharacterOffset = -0.1f;
    protected int k_UILayer;
    protected readonly Quaternion k_FlippedYAxisRotation = Quaternion.Euler(0f, 180f, 0f);

    #region 实现父类的抽象函数
    public override void Enter(AState from)
    {
        inventoryCanvas.gameObject.SetActive(true);
        missionPopup.gameObject.SetActive(false);

        charNameDisplay.text = "";
        themeNameDisplay.text = "";

        k_UILayer = LayerMask.NameToLayer("UI");

        skyMeshFilter.gameObject.SetActive(true);
        UIGroundFilter.gameObject.SetActive(true);

        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        if (MusicPlayer.instance.GetStem(0) != menuTheme)
        {
            MusicPlayer.instance.SetStem(0, menuTheme);
            StartCoroutine(MusicPlayer.instance.RestartAllStems());
        }

        runButton.interactable = false;
        runButton.GetComponentInChildren<Text>().text = "Loading...";

        if(m_PowerupToUse != Consumable.ConsumableType.NONE)
        {
            if (!PlayerData.instance.consumables.ContainsKey(m_PowerupToUse) || PlayerData.instance.consumables[m_PowerupToUse] == 0)
            {
                m_PowerupToUse = Consumable.ConsumableType.NONE;
            }
        }

        Refresh();
    }

    public override void Exit(AState to)
    {
        missionPopup.gameObject.SetActive(false);
        inventoryCanvas.gameObject.SetActive(false);

        if (m_Character != null) Destroy(m_Character);

        GameState gamestate = to as GameState;

        skyMeshFilter.gameObject.SetActive(false);
        UIGroundFilter.gameObject.SetActive(false);

        if(gamestate != null)
        {
            gamestate.currentModifier = m_CurrentModifier;

            //重置人物选择的装饰为初始状态，为了下一次的游戏开始。
            m_CurrentModifier = new Modifier();
            //给游戏状态加载道具
            if(m_PowerupToUse != Consumable.ConsumableType.NONE)
            {
                PlayerData.instance.Consume(m_PowerupToUse);
                Consumable inventory = Instantiate(ConsumableDatabase.GetConsumable(m_PowerupToUse));
                inventory.gameObject.SetActive(false);
                gamestate.trackManager.characterController.inventory = inventory;
            }
        }
    }

    public override string GetName()
    {
        return "Loadout";
    }

    public override void Tick()
    {
        //数据库加载成功之后，更改文字
        if (!runButton.interactable)
        {
            bool interactable = ThemeDatabase.loaded && CharacterDatabase.loaded;
            if (interactable)
            {
                runButton.interactable = true;
                runButton.GetComponentInChildren<Text>().text = "Run!";
            }
        }
        //控制人物旋转
        if(m_Character != null)
        {
            m_Character.transform.Rotate(0, k_CharacterRotationSpeed * Time.deltaTime, 0, Space.Self);
        }
        //至少拥有一个角色或者一个主题就显示
        charSelect.gameObject.SetActive(PlayerData.instance.characters.Count > 1);
        themeSelect.gameObject.SetActive(PlayerData.instance.themes.Count > 1);
    }
    #endregion

    /// <summary>
    /// 更新主界面的数据
    /// </summary>
    public void Refresh()
    {
        PopulatePowerup();

        StartCoroutine(PopulateCharacters());
        StartCoroutine(PopulateTheme());
    }

    #region 按钮事件和UI显示事件
    public void GoToStore()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(k_ShopSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
    
    public void ChangeCharacter(int dir)
    {
        PlayerData.instance.usedCharater += dir;
        if (PlayerData.instance.usedCharater >= PlayerData.instance.characters.Count)
            PlayerData.instance.usedCharater = 0;
        else if (PlayerData.instance.usedCharater < 0)
            PlayerData.instance.usedCharater = PlayerData.instance.characters.Count - 1;

        StartCoroutine(PopulateCharacters());
    }

    public void ChangeAccessory(int dir)
    {
        m_UsedAccessory += dir;
        if (m_UsedAccessory >= m_OwnedAccesories.Count)
            m_UsedAccessory = -1;
        else if (m_UsedAccessory < -1)
            m_UsedAccessory = m_OwnedAccesories.Count - 1;

        if (m_UsedAccessory != -1)
            PlayerData.instance.usedAccessory = m_OwnedAccesories[m_UsedAccessory];
        else
            PlayerData.instance.usedAccessory = -1;

        SetupAccessory();
    }

    public void ChangeTheme(int dir)
    {
        PlayerData.instance.usedTheme += dir;
        if (PlayerData.instance.usedTheme >= PlayerData.instance.themes.Count)
            PlayerData.instance.usedTheme = 0;
        else if(PlayerData.instance.usedTheme < 0)
        {
            PlayerData.instance.usedTheme = PlayerData.instance.themes.Count - 1;
        }

        StartCoroutine(PopulateTheme());
    }
    /// <summary>
    /// 从数据库查询并加载主题信息
    /// </summary>
    /// <returns></returns>
    public IEnumerator PopulateTheme()
    {
        ThemeData themeData = null;

        while(themeData == null)
        {
            themeData = ThemeDatabase.GetThemeData(PlayerData.instance.themes[PlayerData.instance.usedTheme]);
            yield return null;
        }

        themeNameDisplay.text = themeData.themeName;
        themeIcon.sprite = themeData.themeIcon;

        skyMeshFilter.sharedMesh = themeData.skyMesh;
        UIGroundFilter.sharedMesh = themeData.UIGroundMesh;      
    }
    /// <summary>
    /// 从数据库查询并加载角色信息
    /// </summary>
    /// <returns></returns>
    public IEnumerator PopulateCharacters()
    {
        accessoriesSelector.gameObject.SetActive(false);
        PlayerData.instance.usedAccessory = -1;
        m_UsedAccessory = -1;

        if (!m_IsLoadingCharacter)
        {
            m_IsLoadingCharacter = true;
            GameObject newChar = null;
            while (newChar == null)
            {
                Character c = CharacterDatabase.GetCharacter(PlayerData.instance.characters[PlayerData.instance.usedCharater]);

                if(c != null)
                {
                    m_OwnedAccesories.Clear();
                    for(int i = 0; i < c.accessories.Length; ++i)
                    {
                        //初始化已拥有的装饰
                        string compoundName = c.characterName + ":" + c.accessories[i].accessoryName;
                        if (PlayerData.instance.characterAccessories.Contains(compoundName))
                        {
                            m_OwnedAccesories.Add(i);
                        }
                    }
                    //设置位置
                    Vector3 pos = charPosition.transform.position;
                    if (m_OwnedAccesories.Count > 0)
                    {
                        pos.x = k_OwnedAccessoriesCharacterOffset;
                    }
                    else
                    {
                        pos.x = 0.0f;
                    }
                    charPosition.transform.position = pos;

                    accessoriesSelector.gameObject.SetActive(m_OwnedAccesories.Count > 0);
                    //初始化角色显示
                    newChar = Instantiate(c.gameObject);
                    Helpers.SetRendererLayerRecursive(newChar, k_UILayer);
                    newChar.transform.SetParent(charPosition, false);
                    newChar.transform.rotation = k_FlippedYAxisRotation;
                    
                    if (m_Character != null)
                        Destroy(m_Character);
                    //初始化角色名字
                    m_Character = newChar;
                    charNameDisplay.text = c.characterName;

                    m_Character.transform.localPosition = Vector3.right * 1000;
                    // 动画师需要一个帧来初始化，在此期间角色将处于一个t型。
                    // 我们将字符移出屏幕，等待初始帧，然后将字符移回原位。
                    // 避免出现难看的“t - pose”闪光时间
                    yield return new WaitForEndOfFrame();
                    m_Character.transform.localPosition = Vector3.zero;

                    SetupAccessory();
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                }                
            }
            m_IsLoadingCharacter = false;
        }
    }
    /// <summary>
    /// 设置使用的装饰
    /// </summary>
    void SetupAccessory()
    {
        Character c = m_Character.GetComponent<Character>();
        c.SetupAccessory(PlayerData.instance.usedAccessory);

        if(PlayerData.instance.usedAccessory == -1)
        {
            accesoryNameDisplay.text = "None";
            accessoryIconDisplay.enabled = false;
        }
        else
        {
            accessoryIconDisplay.enabled = true;
            accesoryNameDisplay.text = c.accessories[PlayerData.instance.usedAccessory].accessoryName;
            accessoryIconDisplay.sprite = c.accessories[PlayerData.instance.usedAccessory].accessoryIcon;
        }
    }
    /// <summary>
    /// 从数据库查询并加载道具信息
    /// </summary>
    void PopulatePowerup()
    {
        powerupIcon.gameObject.SetActive(true);

        if (PlayerData.instance.consumables.Count > 0)
        {
            Consumable csm = ConsumableDatabase.GetConsumable(m_PowerupToUse);

            powerupSelect.gameObject.SetActive(true);

            if(csm != null)
            {
                powerupIcon.sprite = csm.icon;
                powerupCount.text = PlayerData.instance.consumables[m_PowerupToUse].ToString();
            }
            else
            {
                powerupIcon.sprite = noItemIcon;
                powerupCount.text = "";
            }
        }
        else
        {
            powerupSelect.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 更改当前使用的道具
    /// </summary>
    public void ChangeConsumable(int dir)
    {
        bool found = false;

        do
        {
            m_UsedPowerupIndex += dir;

            if (m_UsedPowerupIndex >= (int)Consumable.ConsumableType.MAX_COUNT)
            {
                m_UsedPowerupIndex = 0;
            }
            else if (m_UsedPowerupIndex < 0)
            {
                m_UsedPowerupIndex = (int)Consumable.ConsumableType.MAX_COUNT - 1;
            }

            int count = 0;
            if (PlayerData.instance.consumables.TryGetValue((Consumable.ConsumableType)m_UsedPowerupIndex, out count) && count > 0)
            {
                found = true;
            }
        } while (m_UsedPowerupIndex != 0 && !found);

        m_PowerupToUse = (Consumable.ConsumableType)m_UsedPowerupIndex;
        PopulatePowerup();
    }

    public void SetModifier(Modifier modifier)
    {
        m_CurrentModifier = modifier;
    }

    public void StartGame()
    {
        if(PlayerData.instance.ftueLevel == 1)
        {
            PlayerData.instance.ftueLevel = 2;
            PlayerData.instance.Save();
        }

        manager.SwitchState("Game");
    }
    /// <summary>
    /// 打开排行榜
    /// </summary>
    public void Openleaderboard()
    {
        leaderboard.displayPlayer = false;
        leaderboard.forcePlayerDisplay = false;
        leaderboard.Open();
    }
    #endregion
}
