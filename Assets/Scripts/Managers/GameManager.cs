using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance { get; private set; }

    // ========== 新增：玩家引用字段 ==========
    [Header("玩家引用")]
    public Player player;
    public Player enemy;

    // ========== 新增：游戏区域字段 ==========
    [Header("游戏区域（第2天使用）")]
    public Transform playerHandArea;
    public Transform enemyHandArea;
    public Transform playerBattleArea;
    public Transform enemyBattleArea;
    public Transform playerDeckPosition;
    public Transform enemyDeckPosition;
    public Transform playerGraveyard;
    public Transform enemyGraveyard;

    // 游戏状态
    [Header("游戏状态")]
    public GameState currentState = GameState.None;
    public bool isBattlePhase = false;  // 改为public

    // 游戏设置
    [Header("游戏设置")]
    public int startingHealth = 30;
    public int startingHandSize = 3;

    // 调试用
    [Header("调试")]
    public bool debugMode = true;

    // 卡牌管理器引用
    [Header("卡牌管理")]
    public CardManager cardManager;

    [Header("卡牌位置 - 2x5网格")]
    public CardSlot[] playerSlots;  // 玩家10个位置 - 确保这行存在
    public CardSlot[] enemySlots;   // 敌方10个位置

    // 私有变量
    private int currentTurn = 1;

    // ==================== Unity生命周期 ====================
  
    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
            DebugLog("GameManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("已存在GameManager实例，销毁新实例");
            return;
        }

        // 初始化游戏状态
        currentState = GameState.None;
        isBattlePhase = false;
        currentTurn = 1;
    }

    void Start()
    {
        DebugLog("游戏开始初始化...");
        InitializeGame();
    }


    void Update()
    {
        // 调试快捷键
        if (Input.GetKeyDown(KeyCode.F1) && debugMode)
        {
            Debug.Log("=== 游戏状态信息 ===");
            Debug.Log($"当前回合: {currentTurn}");
            Debug.Log($"当前状态: {currentState}");
            Debug.Log($"战斗阶段: {isBattlePhase}");
            if (player != null) Debug.Log($"玩家生命: {player.GetHealthInfo()}");
            if (enemy != null) Debug.Log($"敌方生命: {enemy.GetHealthInfo()}");
        }

        // 快速测试：F2直接胜利，F3直接失败
        if (Input.GetKeyDown(KeyCode.F2) && debugMode)
        {
            DebugLog("调试：直接胜利");
            EndGame(true);
        }
        if (Input.GetKeyDown(KeyCode.F3) && debugMode)
        {
            DebugLog("调试：直接失败");
            EndGame(false);
        }
        if (Input.GetKeyDown(KeyCode.F4) && debugMode)//！！！！！！！！！
        {
            DebugLog("调试：运用DebugUpdateUI()方法");
            UIManager.Instance.DebugUpdateUI();
        }
    }

    // ==================== 卡牌相关方法 ====================

    // 初始化卡牌系统
    void InitializeCardSystem()
    {
        DebugLog("初始化卡牌系统...");

        // 获取或创建CardManager
        cardManager = GetComponent<CardManager>();
        if (cardManager == null)
            cardManager = gameObject.AddComponent<CardManager>();

        // 设置卡牌预制体引用
        if (cardManager.cardPrefab == null)
        {
            // 需要在Inspector中手动设置
            Debug.LogWarning("请手动设置CardManager的cardPrefab字段");
        }
        
        Debug.Log($"CardManager reference: {cardManager}");
        Debug.Log($"Card Prefab: {CardManager.Instance.cardPrefab}");
        Debug.Log($"Card Prefab: {cardManager.test}");
    }

    // 初始抽牌
    void DrawStartingHands()
    {
        DebugLog("开始初始抽牌...");

        if (cardManager == null)
        {
            Debug.LogError("CardManager未初始化");
            return;
        }

        // 玩家抽牌
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawCardForPlayer(player);
            DrawCardForPlayer(enemy);
        }
    }

    // 为玩家抽牌
    void DrawCardForPlayer(Player targetPlayer)
    {
        if (targetPlayer == null || cardManager == null) return;

        // 创建随机卡牌
        GameObject cardObj = cardManager.CreateRandomCard(
            targetPlayer.isHumanPlayer,
            targetPlayer.isHumanPlayer ? playerHandArea : enemyHandArea
        );

        if (cardObj != null)
        {
            // 获取Card组件
            Card card = cardObj.GetComponent<Card>();
            if (card != null)
            {
                card.SetCardLocation(CardLocation.Hand);
                card.isDraggable = targetPlayer.isHumanPlayer;
            }

            // 添加到玩家手牌
            targetPlayer.AddCardToHand(cardObj);

            DebugLog($"{targetPlayer.playerName} 抽到卡牌: {cardObj.name}");
        }
    }

    // 每回合抽牌
    void DrawCardForTurn(Player targetPlayer)
    {
        if (targetPlayer == null || cardManager == null) return;

        // 检查手牌上限
        if (targetPlayer.handCards.Count >= 7) // 假设最大手牌7张
        {
            DebugLog($"{targetPlayer.playerName} 手牌已满");
            return;
        }

        DrawCardForPlayer(targetPlayer);
    }

    // 卡牌死亡事件处理
    public void OnCardDied(Card deadCard)
    {
        if (deadCard == null) return;

        DebugLog($"卡牌死亡: {deadCard.CardName}");

        // 从卡槽中移除
        CardSlot[] slots = deadCard.isPlayerCard ? playerSlots : enemySlots;
        foreach (CardSlot slot in slots)
        {
            if (slot.currentCard == deadCard)
            {
                slot.RemoveCard();
                break;
            }
        }

        // 移动到墓地
        Player owner = deadCard.isPlayerCard ? player : enemy;
        owner.MoveToGraveyard(deadCard.gameObject);

        // 回收卡牌到对象池
        if (cardManager != null)
        {
            cardManager.ReturnCardToPool(deadCard.gameObject);
        }
    }

    // 获取空卡槽
    public CardSlot GetEmptyPlayerSlot()
    {
        if (playerSlots == null) return null;

        foreach (CardSlot slot in playerSlots)
        {
            if (slot.IsEmpty())
                return slot;
        }
        return null;
    }

    public CardSlot GetEmptyEnemySlot()
    {
        if (enemySlots == null) return null;

        foreach (CardSlot slot in enemySlots)
        {
            if (slot.IsEmpty())
                return slot;
        }
        return null;
    }

    // 获取所有卡牌（用于战斗阶段）
    public List<Card> GetAllPlayerCards()
    {
        List<Card> cards = new List<Card>();

        if (playerSlots != null)
        {
            foreach (CardSlot slot in playerSlots)
            {
                if (slot.currentCard != null)
                    cards.Add(slot.currentCard);
            }
        }

        return cards;
    }

    public List<Card> GetAllEnemyCards()
    {
        List<Card> cards = new List<Card>();

        if (enemySlots != null)
        {
            foreach (CardSlot slot in enemySlots)
            {
                if (slot.currentCard != null)
                    cards.Add(slot.currentCard);
            }
        }

        return cards;
    }

    // 重置所有卡牌攻击状态
    void ResetAllCardAttacks()
    {
        ResetCardAttacks(true);  // 玩家卡牌
        ResetCardAttacks(false); // 敌方卡牌
    }

    void ResetCardAttacks(bool isPlayer)
    {
        CardSlot[] slots = isPlayer ? playerSlots : enemySlots;

        foreach (CardSlot slot in slots)
        {
            if (slot.currentCard != null)
            {
                slot.currentCard.ResetAttackStatus();
            }
        }
    }
    // ==================== 游戏初始化 ====================

    // 更新InitializeGame方法
    void InitializeGame()
    {
        DebugLog("开始游戏初始化...");

        // 初始化玩家
        if (player != null)
        {
            player.Initialize("玩家", startingHealth, true);
            player.OnHealthChanged += OnPlayerHealthChanged;
        }

        if (enemy != null)
        {
            enemy.Initialize("敌方", startingHealth, false);
            enemy.OnHealthChanged += OnEnemyHealthChanged;
        }

        // 初始化卡牌系统
        InitializeCardSystem();

        // 更新UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGameState("游戏初始化");
            UIManager.Instance.UpdateTurnInfo($"回合 1");
            UIManager.Instance.UpdatePlayerHealth(player.health, player.maxHealth);
            UIManager.Instance.UpdateEnemyHealth(enemy.health, enemy.maxHealth);
        }

        // 开始游戏
        StartCoroutine(GameStartRoutine());
    }


    IEnumerator GameStartRoutine()
    {
        DebugLog("游戏开始协程启动");

        yield return null;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("游戏开始！", false);
        }

        // 抽起始手牌
        DrawStartingHands();

        yield return new WaitForSeconds(1f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"双方各抽{startingHandSize}张牌", false);
        }

        yield return new WaitForSeconds(1f);

        DebugLog("初始抽牌完成，开始玩家回合");
        StartPlayerTurn();
    }

    // ==================== 回合管理 ====================

    // 开始玩家回合
    // 更新StartPlayerTurn方法
    public void StartPlayerTurn()
    {
        if (currentState == GameState.GameOver)
        {
            DebugLog("游戏已结束，无法开始新回合");
            return;
        }

        currentState = GameState.PlayerTurn;
        isBattlePhase = false;

        DebugLog($"=== 第{currentTurn}回合：玩家回合开始 ===");

        // 抽牌
        DrawCardForTurn(player);

        // 重置卡牌攻击状态
        ResetCardAttacks(true);

        // 更新UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGameState("你的回合");
            UIManager.Instance.UpdateTurnInfo($"回合 {currentTurn} - 玩家行动");
            UIManager.Instance.SetEndTurnButton(true, "结束回合");
            UIManager.Instance.ShowMessage("你的回合，请放置卡牌", false);

            // 更新卡牌计数
            UIManager.Instance.UpdatePlayerCardCount(
                player.handCards.Count,
                cardManager?.GetDeckCardCount(true) ?? 20,  // 改为返回int的方法
                player.graveyard.Count
            );
        }
    }

    // 开始敌方回合
    public void StartEnemyTurn()
    {
        if (currentState == GameState.GameOver)
        {
            DebugLog("游戏已结束，无法开始新回合");
            return;
        }

        currentState = GameState.EnemyTurn;

        DebugLog($"=== 第{currentTurn}回合：敌方回合开始 ===");

        // 更新UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGameState("敌方回合");
            UIManager.Instance.UpdateTurnInfo($"回合 {currentTurn} - 敌方行动");
            UIManager.Instance.SetEndTurnButton(false, "敌方行动中");
        }

        // 简单AI行动
        StartCoroutine(EnemyTurnRoutine());
    }

    // 开始战斗阶段
    public void StartBattlePhase()
    {
        currentState = GameState.BattlePhase;
        isBattlePhase = true;

        DebugLog($"=== 第{currentTurn}回合：战斗阶段开始 ===");

        // 更新UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGameState("战斗阶段");
            UIManager.Instance.UpdateTurnInfo($"回合 {currentTurn} - 战斗中");
            UIManager.Instance.SetEndTurnButton(false, "战斗中");
        }

        // 执行战斗
        StartCoroutine(BattlePhaseRoutine());
    }

    // ==================== AI和战斗协程 ====================

    IEnumerator EnemyTurnRoutine()
    {
        DebugLog("敌方AI思考中...");
        yield return new WaitForSeconds(1f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("敌方正在思考...", false);
        }

        yield return new WaitForSeconds(1f);

        DebugLog("敌方放置卡牌...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("敌方放置了一张卡牌", false);
        }

        yield return new WaitForSeconds(1f);

        DebugLog("敌方回合结束，进入战斗阶段");

        // 进入战斗阶段
        StartBattlePhase();
    }

    IEnumerator BattlePhaseRoutine()
    {
        DebugLog("我方攻击阶段...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("我方攻击阶段开始", false);
        }

        // 模拟战斗
        if (enemy != null && player != null)
        {
            int damage = Random.Range(1, 5);
            enemy.TakeDamage(damage);
        }

        yield return new WaitForSeconds(1.5f);

        DebugLog("敌方攻击阶段...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("敌方攻击阶段开始", false);
        }

        // 模拟战斗
        if (player != null && enemy != null)
        {
            int damage = Random.Range(1, 5);
            player.TakeDamage(damage);
        }

        yield return new WaitForSeconds(1.5f);

        DebugLog("战斗结束，清理战场...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("战斗结束", false);
        }

        yield return new WaitForSeconds(0.5f);

        // 回合数增加
        currentTurn++;

        DebugLog($"战斗阶段结束，开始第{currentTurn}回合");

        // 回到玩家回合
        StartPlayerTurn();
    }

    // ==================== 游戏流程控制 ====================

    // 结束玩家回合（按钮调用）
    public void EndPlayerTurn()
    {
        if (currentState == GameState.PlayerTurn && !isBattlePhase)
        {
            DebugLog("玩家结束回合");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("回合结束", false);
            }

            StartEnemyTurn();
        }
        else
        {
            string message = $"当前无法结束回合 - 状态: {currentState}";
            DebugLog(message);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage(message, true);
            }
        }
    }

    // 结束游戏
    public void EndGame(bool playerWon)
    {
        currentState = GameState.GameOver;
        string result = playerWon ? "胜利" : "失败";
        DebugLog($"游戏结束 - 玩家{result}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(playerWon);
        }
    }

    // ==================== 事件处理 ====================

    // 玩家生命值变化
    void OnPlayerHealthChanged(int newHealth, int maxHealth)
    {
        DebugLog($"玩家生命值变化: {newHealth}/{maxHealth}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerHealth(newHealth, maxHealth);
        }
    }

    // 敌方生命值变化
    void OnEnemyHealthChanged(int newHealth, int maxHealth)
    {
        DebugLog($"敌方生命值变化: {newHealth}/{maxHealth}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateEnemyHealth(newHealth, maxHealth);
        }
    }

    // ==================== 工具方法 ====================

    // 调试日志
    void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }

    // 调试用计时器
    IEnumerator DebugTurnTimer()
    {
        float timer = 0f;
        float maxTime = 10f; // 10秒自动结束回合

        while (timer < maxTime && currentState == GameState.PlayerTurn)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (currentState == GameState.PlayerTurn)
        {
            DebugLog("调试：回合超时，自动结束");
            EndPlayerTurn();
        }
    }

    // 获取游戏状态信息
    public string GetGameStateInfo()
    {
        return $"状态: {currentState}, 战斗: {isBattlePhase}";
    }

    // 获取玩家信息
    public string GetPlayerInfo()
    {
        if (player == null) return "玩家: 未设置";
        return $"玩家: {player.GetHealthInfo()}";
    }

    // 获取敌方信息
    public string GetEnemyInfo()
    {
        if (enemy == null) return "敌方: 未设置";
        return $"敌方: {enemy.GetHealthInfo()}";
    }
}