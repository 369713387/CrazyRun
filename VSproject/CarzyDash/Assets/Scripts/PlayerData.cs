using System.Collections.Generic;
using System.IO;
#if UNITY_ANALYTICS
using UnityEngine.Analytics.Experimental;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//游戏分数记录排名
public struct HighscoreEntry : System.IComparable<HighscoreEntry>
{
    public string name;
    public int score;

    public int CompareTo(HighscoreEntry other)
    {
        //我们想要从最高到最低，所以反比。
        return other.score.CompareTo(score);
    }
}

public class PlayerData
{
    //没有继承mono的单例模式
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";

    public int coins;
    public int diamonds;
    //拥有消耗品（道具）的类型和对应的数量的字典
    public Dictionary<Consumable.ConsumableType, int> consumables = new Dictionary<Consumable.ConsumableType, int>();
    //拥有的角色
    public List<string> characters = new List<string>();
    //拥有的主题
    public List<string> themes = new List<string>();
    //拥有的任务
    public List<MissionBase> missions = new List<MissionBase>();
    //当前使用的主题
    public int usedTheme;
    //当前使用的角色
    public int usedCharater;
    //当前使用的角色的装饰
    public int usedAccessory = -1;
    //所有配件清单，在表单“charName:accessoryName”中。
    public List<string> characterAccessories = new List<string>();

    public List<HighscoreEntry> highscores = new List<HighscoreEntry>();
    //任务列表清单
    //public List<>
    public string previousName = "Trash Cat";

    public bool licenceAccepted;

    public float masterVolume = float.MinValue, musicVolume = float.MinValue, masterSFXVolume = float.MinValue;

    //这个变量是用来跟踪玩家第一次做的事情。每次用户执行其中一个步骤时，它都会增加
    //当他们点击开始时，它将增加到1，在第一次run时增加到2，在run了至少300米时，增加到3
    public int ftueLevel = 0;
    //Player win a rank ever 300m (e.g. a player having reached 1200m at least once will be rank 4)
    public int rank = 0;

    static int s_Version = 11;

    /// <summary>
    /// 使用消耗品（道具）
    /// </summary>
    /// <param name="type"></param>
    public void Consume(Consumable.ConsumableType type)
    {
        if (!consumables.ContainsKey(type))
            return;

        consumables[type] -= 1;
        if (consumables[type] == 0)
        {
            consumables.Remove(type);
        }

        Save();
    }
    /// <summary>
    /// 购买消耗品（道具）
    /// </summary>
    /// <param name="type"></param>
    public void Add(Consumable.ConsumableType type)
    {
        if (!consumables.ContainsKey(type))
        {
            consumables[type] = 0;
        }

        consumables[type] += 1;

        Save();
    }
    /// <summary>
    /// 购买角色
    /// </summary>
    /// <param name="name"></param>
    public void AddCharacter(string name)
    {
        characters.Add(name);
    }
    /// <summary>
    /// 购买装饰
    /// </summary>
    /// <param name="name"></param>
    public void AddAccessory(string name)
    {
        characterAccessories.Add(name);
    }
    /// <summary>
    /// 购买主题
    /// </summary>
    /// <param name="theme"></param>
    public void AddTheme(string theme)
    {
        themes.Add(theme);
    }
    #region 任务管理
    /// <summary>
    /// 检查任务数量
    /// </summary>
    public void CheckMissionsCount()
    {
        //设定最多接受的任务数
        while (missions.Count < 2)
        {
            AddMission();
        }
    }
    /// <summary>
    /// 添加任务
    /// </summary>
    public void AddMission()
    {
        int val = Random.Range(0, (int)MissionBase.MissionType.MAX);

        MissionBase newMission = MissionBase.GetNewMissionFromType((MissionBase.MissionType)val);
        newMission.Created();

        missions.Add(newMission);
    }
    /// <summary>
    /// 开始执行任务
    /// </summary>
    public void StartRunMissions(TrackManager manager)
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            missions[i].RunStart(manager);
        }
    }
    /// <summary>
    /// 更新任务
    /// </summary>
    public void UpdateMissions(TrackManager manager)
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            missions[i].Update(manager);
        }
    }
    /// <summary>
    /// 完成任务
    /// </summary>
    /// <returns></returns>
    public bool AnyMissionsComplete()
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            if (missions[i].isComplete) return true;
        }
        return false;
    }
    /// <summary>
    /// 任务要求
    /// </summary>
    public void ClaimMission(MissionBase mission)
    {
        diamonds += mission.reward;
#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Diamonds, // Currency type
            "mission",               // Context
            mission.reward,          // Amount
            "anchovies",             // Item ID
            Diamonds,                 // Item balance
            "consumable",            // Item type
            rank.ToString()          // Level
        );
#endif
        //任务完成后从list中删除
        missions.Remove(mission);

        CheckMissionsCount();

        Save();
    }

    #endregion

    #region 分数排行榜管理
    /// <summary>
    /// 得到一个分数的排名
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public int GetScorePlace(int score)
    {
        HighscoreEntry entry = new HighscoreEntry();
        entry.score = score;
        entry.name = "";
        //找到指定条目的索引
        int index = highscores.BinarySearch(entry);

        return index < 0 ? (~index) : index;
    }
    /// <summary>
    /// 插入一个排名
    /// </summary>
    /// <param name="score"></param>
    /// <param name="name"></param>
    public void InsertScore(int score, string name)
    {
        HighscoreEntry entry = new HighscoreEntry();
        entry.score = score;
        entry.name = name;
        highscores.Insert(GetScorePlace(score), entry);
        //只能显示3条记录
        while (highscores.Count > 3)
        {
            highscores.RemoveAt(highscores.Count - 1);
        }
    }
    #endregion

    #region 文件管理
    public static void Create()
    {
        if (m_Instance == null)
        {
            m_Instance = new PlayerData();

            AssetBundlesDatabaseHandler.Load();
        }

        m_Instance.saveFile = Application.persistentDataPath + "/save.bin";

        if (File.Exists(m_Instance.saveFile))
        {
            //如果我们有保存的数据，则进行加载
            m_Instance.Read();
        }
        else
        {
            NewSave();
        }

        m_Instance.CheckMissionsCount();
    }
    /// <summary>
    /// 清空记录，新建存档
    /// </summary>
    public static void NewSave()
    {
        m_Instance.characters.Clear();
        m_Instance.themes.Clear();
        m_Instance.missions.Clear();
        m_Instance.characterAccessories.Clear();
        m_Instance.consumables.Clear();

        m_Instance.usedCharater = 0;
        m_Instance.usedTheme = 0;
        m_Instance.usedAccessory = -1;

        m_Instance.coins = 0;
        m_Instance.diamonds = 0;

        m_Instance.characters.Add("Trash Cat");
        m_Instance.themes.Add("Day");

        m_Instance.ftueLevel = 0;
        m_Instance.rank = 0;

        m_Instance.CheckMissionsCount();

        m_Instance.Save();
    }
    /// <summary>
    /// 从文件中读数据（读写顺序要一致！）
    /// </summary>
    public void Read()
    {
        BinaryReader br = new BinaryReader(new FileStream((saveFile), FileMode.Open));
        //读取版本号
        int verson = br.ReadInt32();

        if (verson < 6)
        {
            br.Close();

            NewSave();
            br = new BinaryReader(new FileStream(saveFile, FileMode.Open));
            verson = br.ReadInt32();
        }
        //读取金币
        coins = br.ReadInt32();
        //读取消耗品
        consumables.Clear();
        int consumableCount = br.ReadInt32();
        for (int i = 0; i < consumableCount; ++i)
        {
            consumables.Add((Consumable.ConsumableType)br.ReadInt32(), br.ReadInt32());
        }
        //读取角色
        characters.Clear();
        int charCount = br.ReadInt32();
        for (int i = 0; i < charCount; ++i)
        {
            string charName = br.ReadString();

            if (charName.Contains("Raccooon") && verson < 11)
            {
                charName = charName.Replace("Racoon", "Raccoon");
            }

            characters.Add(charName);
        }
        usedCharater = br.ReadInt32();

        //读取角色装扮数据
        characterAccessories.Clear();
        int accCount = br.ReadInt32();
        for (int i = 0; i < accCount; ++i)
        {
            characterAccessories.Add(br.ReadString());
        }
        //读取主题数据
        themes.Clear();
        int themeCount = br.ReadInt32();
        for (int i = 0; i < themeCount; ++i)
        {
            themes.Add(br.ReadString());
        }

        usedTheme = br.ReadInt32();

        if (verson >= 2)
        {
            diamonds = br.ReadInt32();
        }

        //读取排名记录
        if (verson >= 3)
        {
            highscores.Clear();
            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                HighscoreEntry entry = new HighscoreEntry();
                entry.name = br.ReadString();
                entry.score = br.ReadInt32();

                highscores.Add(entry);
            }
        }

        // 读取任务记录.
        if (verson >= 4)
        {
            missions.Clear();

            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                MissionBase.MissionType type = (MissionBase.MissionType)br.ReadInt32();
                MissionBase tempMission = MissionBase.GetNewMissionFromType(type);

                tempMission.Deserialize(br);

                if (tempMission != null)
                {
                    missions.Add(tempMission);
                }
            }
        }

        //读取排名列表中用过的名字的记录
        if (verson >= 7)
        {
            previousName = br.ReadString();
        }

        if (verson >= 8)
        {
            licenceAccepted = br.ReadBoolean();
        }
        //读取音量设置的数据
        if (verson >= 9)
        {
            masterVolume = br.ReadSingle();
            musicVolume = br.ReadSingle();
            masterSFXVolume = br.ReadSingle();
        }

        if (verson >= 10)
        {
            ftueLevel = br.ReadInt32();
            rank = br.ReadInt32();
        }

        br.Close();
    }
    #endregion

    /// <summary>
    /// 保存数据
    /// </summary>
    public void Save()
    {
        BinaryWriter w = new BinaryWriter(new FileStream(saveFile, FileMode.OpenOrCreate));

        w.Write(s_Version);
        w.Write(coins);

        w.Write(consumables.Count);
        foreach (KeyValuePair<Consumable.ConsumableType, int> p in consumables)
        {
            w.Write((int)p.Key);
            w.Write(p.Value);
        }

        // 角色信息存档.
        w.Write(characters.Count);
        foreach (string c in characters)
        {
            w.Write(c);
        }

        w.Write(usedCharater);
        //装饰信息存档
        w.Write(characterAccessories.Count);
        foreach (string a in characterAccessories)
        {
            w.Write(a);
        }

        // 主题信息存档.
        w.Write(themes.Count);
        foreach (string t in themes)
        {
            w.Write(t);
        }

        w.Write(usedTheme);
        w.Write(diamonds);

        // 存档排名信息.
        w.Write(highscores.Count);
        for (int i = 0; i < highscores.Count; ++i)
        {
            w.Write(highscores[i].name);
            w.Write(highscores[i].score);
        }

        //存档任务记录.
        w.Write(missions.Count);
        for (int i = 0; i < missions.Count; ++i)
        {
            w.Write((int)missions[i].GetMissionType());
            missions[i].Serialize(w);
        }

        // 排行榜名字信息存档.
        w.Write(previousName);

        w.Write(licenceAccepted);

        w.Write(masterVolume);
        w.Write(musicVolume);
        w.Write(masterSFXVolume);

        w.Write(ftueLevel);
        w.Write(rank);

        w.Close();
    }
}

// 测试时可以快速添加数据
#if UNITY_EDITOR
public class PlayerDataEditor : Editor
{
    [MenuItem("Trash Dash Debug/Clear Save")]
    static public void ClearSave()
    {
        File.Delete(Application.persistentDataPath + "/save.bin");
    }

    [MenuItem("Trash Dash Debug/Give 1000000 fishbones and 1000 diamonds")]
    static public void GiveCoins()
    {
        PlayerData.instance.coins += 1000000;
        PlayerData.instance.diamonds += 1000;
        PlayerData.instance.Save();
    }

    [MenuItem("Trash Dash Debug/Give 10 Consumables of each types")]
    static public void AddConsumables()
    {

        for (int i = 0; i < ShopItemList.s_ConsumablesTypes.Length; ++i)
        {
            Consumable c = ConsumableDatabase.GetConsumable(ShopItemList.s_ConsumablesTypes[i]);
            if (c != null)
            {
                PlayerData.instance.consumables[c.GetConsumableType()] = 10;
            }
        }

        PlayerData.instance.Save();
    }

}
#endif
