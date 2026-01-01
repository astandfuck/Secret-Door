using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    [Header("卡牌预制体")]
    public GameObject cardPrefab;          // 卡牌预制体
    public GameObject test;

    [Header("默认卡牌数据")]
    public List<CardData> defaultCardLibrary = new List<CardData>();

    [Header("卡牌池")]
    private List<CardData> cardLibrary = new List<CardData>();

    [Header("卡牌对象池")]
    private Queue<GameObject> cardPool = new Queue<GameObject>();
    public int poolSize = 50;              // 对象池大小

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("CardManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"cardPrefab:{cardPrefab}");//!!!!!!

        // 初始化卡牌库
        InitializeCardLibrary();

        // 初始化对象池
        InitializeCardPool();
    }

    // 初始化卡牌库
    void InitializeCardLibrary()
    {
        Debug.Log("初始化卡牌库...");

        // 清空现有卡牌库
        cardLibrary.Clear();

        // 添加默认卡牌（如果未在Inspector中设置）
        if (defaultCardLibrary.Count == 0)
        {
            CreateDefaultCards();
        }
        else
        {
            cardLibrary.AddRange(defaultCardLibrary);
        }

        Debug.Log($"卡牌库初始化完成，共有 {cardLibrary.Count} 张卡牌");
    }

    // 创建默认卡牌数据
    void CreateDefaultCards()
    {
        // 战士类卡牌
        cardLibrary.Add(new CardData("001", "初级战士", 5, 3));
        cardLibrary.Add(new CardData("002", "中级战士", 7, 4));
        cardLibrary.Add(new CardData("003", "高级战士", 10, 6));

        // 法师类卡牌
        cardLibrary.Add(new CardData("004", "初级法师", 3, 5));
        cardLibrary.Add(new CardData("005", "中级法师", 4, 7));

        // 坦克类卡牌
        cardLibrary.Add(new CardData("006", "初级守卫", 8, 2));
        cardLibrary.Add(new CardData("007", "中级守卫", 12, 3));

        // 远程类卡牌
        cardLibrary.Add(new CardData("008", "弓箭手", 4, 4));
        cardLibrary.Add(new CardData("009", "弩手", 5, 5));

        // 特殊卡牌
        cardLibrary.Add(new CardData("010", "治疗师", 6, 2));

        Debug.Log("创建了10张默认卡牌");
    }

    // 初始化卡牌对象池
    void InitializeCardPool()
    {
        Debug.Log("初始化卡牌对象池...");

        if (cardPrefab == null)
        {
            Debug.LogError("卡牌预制体未设置！");
            return;
        }

        // 创建对象池
        for (int i = 0; i < poolSize; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, transform);
            Card cardScript = cardObj.GetComponent<Card>();

            if (cardScript == null)
            {
                Debug.LogError("卡牌预制体缺少Card组件！");
                Destroy(cardObj);
                continue;
            }

            cardObj.name = $"Card_{i:000}";
            cardObj.SetActive(false);
            cardPool.Enqueue(cardObj);
        }

        Debug.Log($"卡牌对象池初始化完成，大小: {cardPool.Count}");
    }

    // 从对象池获取卡牌
    public GameObject GetCardFromPool()
    {
        if (cardPool.Count == 0)
        {
            Debug.LogWarning("对象池为空，创建新卡牌");
            return CreateNewCard();
        }

        GameObject cardObj = cardPool.Dequeue();
        cardObj.SetActive(true);

        return cardObj;
    }

    // 回收卡牌到对象池
    public void ReturnCardToPool(GameObject cardObj)
    {
        if (cardObj == null) return;

        // 重置卡牌状态
        cardObj.transform.SetParent(transform);
        cardObj.SetActive(false);

        // 清理组件
        Card cardScript = cardObj.GetComponent<Card>();
        if (cardScript != null)
        {
            cardScript.ResetCard();
        }

        cardPool.Enqueue(cardObj);
    }

    // 创建新卡牌（对象池为空时使用）
    GameObject CreateNewCard()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("无法创建卡牌：预制体未设置");
            return null;
        }

        GameObject cardObj = Instantiate(cardPrefab, transform);
        cardObj.name = $"Card_New_{cardPool.Count}";

        return cardObj;
    }

    // 获取随机卡牌数据
    public CardData GetRandomCardData()
    {
        if (cardLibrary.Count == 0)
        {
            Debug.LogError("卡牌库为空！");
            return null;
        }

        int randomIndex = Random.Range(0, cardLibrary.Count);
        return new CardData(cardLibrary[randomIndex]); // 返回副本
    }

    // 根据ID获取卡牌数据
    public CardData GetCardDataByID(string cardID)
    {
        foreach (CardData data in cardLibrary)
        {
            if (data.cardID == cardID)
            {
                return new CardData(data); // 返回副本
            }
        }

        Debug.LogWarning($"未找到卡牌ID: {cardID}");
        return GetRandomCardData();
    }

    // 创建特定卡牌
    public GameObject CreateCard(string cardID, bool isPlayerCard, Transform parent = null)
    {
        CardData cardData = GetCardDataByID(cardID);
        if (cardData == null)
        {
            Debug.LogError($"无法创建卡牌，无效的ID: {cardID}");
            return null;
        }

        cardData.isPlayerCard = isPlayerCard;
        return CreateCard(cardData, parent);
    }

    // 创建卡牌（使用卡牌数据）
    public GameObject CreateCard(CardData cardData, Transform parent = null)
    {
        GameObject cardObj = GetCardFromPool();
        if (cardObj == null) return null;

        // 设置父对象
        if (parent != null)
        {
            cardObj.transform.SetParent(parent, false);
        }

        // 获取Card组件并初始化
        Card cardScript = cardObj.GetComponent<Card>();
        if (cardScript != null)
        {
            cardScript.Initialize(cardData);
        }
        else
        {
            Debug.LogError("卡牌对象缺少Card组件！");
        }

        cardObj.name = $"Card_{cardData.cardName}_{(cardData.isPlayerCard ? "P" : "E")}";

        return cardObj;
    }

    // 创建随机卡牌
    public GameObject CreateRandomCard(bool isPlayerCard, Transform parent = null)
    {
        CardData randomData = GetRandomCardData();
        if (randomData == null) return null;

        randomData.isPlayerCard = isPlayerCard;
        return CreateCard(randomData, parent);
    }

    // 工具方法：打印卡牌库信息
    public void PrintCardLibraryInfo()
    {
        Debug.Log("=== 卡牌库信息 ===");
        Debug.Log($"总卡牌数: {cardLibrary.Count}");

        foreach (CardData card in cardLibrary)
        {
            Debug.Log($"[{card.cardID}] {card.cardName} - 攻{card.attack}/血{card.health}");
        }
    }

    // 工具方法：打印对象池信息
    public void PrintPoolInfo()
    {
        Debug.Log($"对象池状态: {cardPool.Count}张卡牌可用");
    }

    // 获取对象池信息
    public string GetPoolInfo()
    {
        return $"对象池: {cardPool.Count}/{poolSize}";
    }

    // 获取卡组数量信息（用于UI显示）
    public int GetDeckCount(bool isPlayerCard)
    {
        // 这里应该根据实际卡组逻辑返回
        // 暂时返回固定值
        return 20; // 假设卡组有20张卡
    }

    // 获取卡组剩余卡牌数量
    public int GetDeckCardCount(bool isPlayerCard)
    {
        // 这里需要根据实际卡组管理逻辑实现
        // 暂时返回固定值
        return 20; // 假设每个卡组初始有20张卡
    }

    // 获取对象池信息（字符串，用于调试）
    public string GetPoolInfoString()
    {
        return $"对象池: {cardPool.Count}/{poolSize}";
    }
}