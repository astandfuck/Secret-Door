using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 单例实例
    public static UIManager Instance { get; private set; }

    [Header("游戏状态显示")]
    public Text gameStateText;
    public Text turnInfoText;

    [Header("玩家信息")]
    public Text playerHealthText;
    public Text playerCardCountText;
    public Text enemyHealthText;
    public Text enemyCardCountText;

    [Header("消息提示")]
    public GameObject messagePanel;
    public Text messageText;
    public float messageDuration = 2f;

    [Header("控制按钮")]
    public Button endTurnButton;

    [Header("调试按钮")]
    public Button debugWinButton;
    public Button debugLoseButton;

    [Header("游戏结束")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Button restartButton;

    [Header("工具提示")]
    public GameObject tooltipPanel;
    public Text tooltipTitleText;
    public Text tooltipDescriptionText;
    public float tooltipOffsetX = 10f;
    public float tooltipOffsetY = -10f;

    // 私有变量
    private Coroutine messageCoroutine;

    // ==================== Unity生命周期 ====================

    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("UIManager初始化完成");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 初始化UI
        InitializeUI();

        // 按钮事件监听
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
            endTurnButton.interactable = false; // 初始不可用
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        // 调试按钮事件
        if (debugWinButton != null)
        {
            debugWinButton.onClick.AddListener(OnDebugWinClicked);
            debugWinButton.gameObject.SetActive(GameManager.Instance.debugMode);
        }

        if (debugLoseButton != null)
        {
            debugLoseButton.onClick.AddListener(OnDebugLoseClicked);
            debugLoseButton.gameObject.SetActive(GameManager.Instance.debugMode);
        }

        // 隐藏工具提示
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    void InitializeUI()
    {
        // 隐藏消息面板和游戏结束面板
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 初始化文本
        UpdateGameState("游戏初始化中...");
        UpdateTurnInfo("准备开始");

        // 初始化玩家信息
        UpdatePlayerHealth(30, 30);
        UpdateEnemyHealth(30, 30);
        UpdatePlayerCardCount(0, 0, 0);
        UpdateEnemyCardCount(0, 0, 0);
    }

    // ==================== UI更新方法 ====================

    // 更新游戏状态
    public void UpdateGameState(string state)
    {
        if (gameStateText != null)
        {
            gameStateText.text = $"游戏状态: {state}";
            Debug.Log($"UI更新 - 游戏状态: {state}");
        }
    }

    // 更新回合信息
    public void UpdateTurnInfo(string info)
    {
        if (turnInfoText != null)
        {
            turnInfoText.text = info;
        }
    }

    // 更新玩家生命值
    public void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"玩家生命: {current}/{max}";
        }
    }

    // 更新敌方生命值
    public void UpdateEnemyHealth(int current, int max)
    {
        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"敌方生命: {current}/{max}";
        }
    }

    // 更新玩家卡牌数量
    public void UpdatePlayerCardCount(int hand, int deck, int graveyard)
    {
        if (playerCardCountText != null)
        {
            playerCardCountText.text = $"手牌:{hand} 卡组:{deck} 墓地:{graveyard}";
        }
    }

    // 更新敌方卡牌数量
    public void UpdateEnemyCardCount(int hand, int deck, int graveyard)
    {
        if (enemyCardCountText != null)
        {
            enemyCardCountText.text = $"手牌:{hand} 卡组:{deck} 墓地:{graveyard}";
        }
    }

    // ==================== 消息系统 ====================

    // 显示消息
    public void ShowMessage(string message, bool isWarning = false)
    {
        if (messagePanel == null || messageText == null) return;

        // 停止之前的消息协程
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        // 设置消息
        messageText.text = message;
        messageText.color = isWarning ? Color.red : Color.white;

        // 显示面板
        messagePanel.SetActive(true);

        // 开始隐藏协程
        messageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
            messageCoroutine = null;
        }
    }

    // ==================== 按钮控制 ====================

    // 设置结束回合按钮状态
    public void SetEndTurnButton(bool interactable, string text = "结束回合")
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable;
            endTurnButton.GetComponentInChildren<Text>().text = text;
            Debug.Log($"结束回合按钮: {(interactable ? "启用" : "禁用")}");
        }
    }

    // 结束回合按钮点击事件
    void OnEndTurnButtonClicked()
    {
        Debug.Log("UI: 结束回合按钮被点击");
        GameManager.Instance.EndPlayerTurn();
    }

    // ==================== 游戏结束界面 ====================

    // 显示游戏结束
    public void ShowGameOver(bool playerWon)
    {
        if (gameOverPanel == null) return;

        string result = playerWon ? "胜利！🎉" : "失败...💀";
        string message = playerWon ? "恭喜你击败了对手！" : "再接再厉！";

        if (gameOverText != null)
        {
            gameOverText.text = $"游戏结束\n{result}\n{message}";
        }

        // 禁用其他UI
        SetEndTurnButton(false);

        // 显示游戏结束面板
        gameOverPanel.SetActive(true);
    }

    // 隐藏游戏结束
    public void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 重新开始按钮
    void OnRestartButtonClicked()
    {
        Debug.Log("重新开始游戏");

        // 隐藏游戏结束面板
        HideGameOver();

        // 重置游戏
        // 注意：这里需要根据实际游戏逻辑实现
        ShowMessage("重新开始功能将在第5天实现", true);
    }

    // ==================== 调试方法 ====================

    // 调试用：模拟更新
    public void DebugUpdateUI()
    {
        UpdateGameState(GameManager.Instance.currentState.ToString());
        UpdateTurnInfo($"第1回合 - {GameManager.Instance.GetGameStateInfo()}");
        UpdatePlayerHealth(25, 30);
        UpdateEnemyHealth(30, 30);
        UpdatePlayerCardCount(3, 17, 0);
        UpdateEnemyCardCount(3, 17, 0);
    }

    // 调试：直接胜利
    void OnDebugWinClicked()
    {
        GameManager.Instance.EndGame(true);
        ShowMessage("调试：直接胜利", false);
    }

    // 调试：直接失败
    void OnDebugLoseClicked()
    {
        GameManager.Instance.EndGame(false);
        ShowMessage("调试：直接失败", true);
    }

    // ==================== 工具提示系统 ====================

    // 显示工具提示
    public void ShowTooltip(string title, string description, Vector3 position)
    {
        if (tooltipPanel == null || tooltipTitleText == null || tooltipDescriptionText == null)
            return;

        // 设置文本
        tooltipTitleText.text = title;
        tooltipDescriptionText.text = description;

        // 设置位置（跟随鼠标）
        RectTransform rectTransform = tooltipPanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, position);
            rectTransform.anchoredPosition = new Vector2(
                screenPoint.x + tooltipOffsetX,
                screenPoint.y + tooltipOffsetY
            );
        }

        // 显示面板
        tooltipPanel.SetActive(true);
    }

    // 隐藏工具提示
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }


}