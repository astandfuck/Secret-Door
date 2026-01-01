using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("卡牌数据")]
    private CardData cardData;                    // 卡牌数据
    private int currentHealth;                    // 当前生命值
    private bool canAttackThisTurn = true;        // 本轮是否可攻击

    [Header("UI组件引用")]
    public Text cardNameText;                     // 卡牌名称
    public Text healthText;                       // 生命值文本
    public Text attackText;                       // 攻击力文本
    public Text manaText;                         // 法力值文本（预留）
    public Text descriptionText;                  // 描述文本
    public Image cardBackground;                  // 卡牌背景
    public Image cardFrame;                       // 卡牌边框
    public Image cardArt;                         // 卡牌图案

    [Header("状态")]
    public bool isPlayerCard = true;              // 是否为玩家卡牌
    public CardLocation currentLocation = CardLocation.Deck; // 当前位置
    public bool isDraggable = true;               // 是否可拖拽
    public bool isInHand = false;                 // 是否在手牌中

    [Header("视觉效果")]
    public Color playerCardColor = new Color(0.6f, 0.8f, 1f, 1f);     // 玩家卡牌颜色
    public Color enemyCardColor = new Color(1f, 0.6f, 0.6f, 1f);      // 敌方卡牌颜色
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);        // 高亮颜色
    public Color damagedColor = Color.red;                            // 受伤颜色

    [Header("拖拽设置")]
    public float dragScale = 1.2f;                // 拖拽时缩放
    public float handCardScale = 0.7f;            // 手牌中的缩放
    public float battlefieldCardScale = 1f;       // 战场上的缩放

    // 私有变量
    private Vector3 startDragPosition;            // 拖拽起始位置
    private Transform startDragParent;            // 拖拽前父对象
    private CanvasGroup canvasGroup;              // 用于控制射线检测
    private RectTransform rectTransform;          // 矩形变换
    public bool isBeingDragged = false;          // 是否正在拖拽

    // 属性
    public string CardName => cardData?.cardName ?? "未知卡牌";
    public int Health => currentHealth;
    public int MaxHealth => cardData?.health ?? 0;
    public int Attack => cardData?.attack ?? 0;
    public bool CanAttack => canAttackThisTurn && currentLocation == CardLocation.Battlefield;

    // ==================== Unity生命周期 ====================

    void Awake()
    {
        // 获取组件引用
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 自动查找UI组件
        FindUIComponents();
    }

    void Start()
    {
        // 根据位置更新状态
        UpdateCardAppearance();
    }

    // ==================== 初始化 ====================

    // 使用CardData初始化
    public void Initialize(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("无法用空数据初始化卡牌");
            return;
        }

        cardData = new CardData(data); // 创建副本
        currentHealth = cardData.health;
        isPlayerCard = data.isPlayerCard;

        UpdateCardUI();
        UpdateCardAppearance();

        Debug.Log($"卡牌初始化: {CardName} (攻{Attack}/血{Health}) - {(isPlayerCard ? "玩家" : "敌方")}");
    }

    // 简单初始化（兼容旧代码）
    public void Initialize(string name, int health, int attack, bool isPlayer)
    {
        cardData = new CardData("temp", name, health, attack);
        cardData.isPlayerCard = isPlayer;
        currentHealth = health;

        UpdateCardUI();
        UpdateCardAppearance();
    }

    // 自动查找UI组件
    void FindUIComponents()
    {
        if (cardNameText == null)
            cardNameText = transform.Find("CardNameText")?.GetComponent<Text>();

        if (healthText == null)
            healthText = transform.Find("HealthPanel/HealthText")?.GetComponent<Text>();

        if (attackText == null)
            attackText = transform.Find("AttackPanel/AttackText")?.GetComponent<Text>();

        if (manaText == null)
            manaText = transform.Find("ManaText")?.GetComponent<Text>();

        if (descriptionText == null)
            descriptionText = transform.Find("DescriptionText")?.GetComponent<Text>();

        if (cardBackground == null)
            cardBackground = GetComponent<Image>();

        if (cardFrame == null)
            cardFrame = transform.Find("CardFrame")?.GetComponent<Image>();

        if (cardArt == null)
            cardArt = transform.Find("CardArt")?.GetComponent<Image>();
    }

    // ==================== UI更新 ====================

    // 更新所有UI元素
    void UpdateCardUI()
    {
        if (cardData == null) return;

        // 更新文本
        if (cardNameText != null) cardNameText.text = cardData.cardName;
        if (healthText != null) healthText.text = currentHealth.ToString();
        if (attackText != null) attackText.text = cardData.attack.ToString();
        if (manaText != null) manaText.text = cardData.manaCost.ToString();
        if (descriptionText != null) descriptionText.text = cardData.description;

        // 更新卡牌图案
        if (cardArt != null && cardData.cardArt != null)
            cardArt.sprite = cardData.cardArt;
    }

    // 更新卡牌外观（颜色、大小等）
    void UpdateCardAppearance()
    {
        // 根据阵营设置颜色
        if (cardBackground != null)
        {
            cardBackground.color = isPlayerCard ? playerCardColor : enemyCardColor;
        }

        // 根据位置调整大小
        switch (currentLocation)
        {
            case CardLocation.Hand:
                transform.localScale = Vector3.one * handCardScale;
                isDraggable = isPlayerCard;
                break;

            case CardLocation.Battlefield:
                transform.localScale = Vector3.one * battlefieldCardScale;
                isDraggable = false; // 战场上的卡牌不可拖拽
                break;

            case CardLocation.Deck:
            case CardLocation.Graveyard:
                transform.localScale = Vector3.one * 0.5f;
                isDraggable = false;
                break;
        }

        // 更新边框颜色（根据血量）
        if (cardFrame != null)
        {
            if (currentHealth <= 0)
                cardFrame.color = Color.black; // 死亡
            else if (currentHealth < MaxHealth)
                cardFrame.color = damagedColor; // 受伤
            else
                cardFrame.color = Color.white; // 健康
        }

        // 更新文本颜色
        if (healthText != null)
            healthText.color = (currentHealth < MaxHealth) ? damagedColor : Color.white;
    }

    // ==================== 游戏逻辑 ====================

    // 受到伤害
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int previousHealth = currentHealth;
        currentHealth -= damage;

        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{CardName} 受到 {damage} 点伤害 ({previousHealth} -> {currentHealth})");

        UpdateCardUI();
        UpdateCardAppearance();

        // 播放伤害效果
        StartCoroutine(DamageEffect());

        // 检查是否死亡
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    // 治疗
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int previousHealth = currentHealth;
        currentHealth += amount;

        if (currentHealth > MaxHealth) currentHealth = MaxHealth;

        Debug.Log($"{CardName} 恢复 {amount} 点生命 ({previousHealth} -> {currentHealth})");

        UpdateCardUI();
        UpdateCardAppearance();

        // 播放治疗效果
        StartCoroutine(HealEffect());
    }

    // 攻击其他卡牌
    public void AttackTarget(Card targetCard)
    {
        if (!CanAttack || targetCard == null) return;

        Debug.Log($"{CardName} 攻击 {targetCard.CardName}");

        // 互相造成伤害
        targetCard.TakeDamage(Attack);
        TakeDamage(targetCard.Attack);

        // 标记为已攻击
        canAttackThisTurn = false;

        // 更新外观
        UpdateCardAppearance();
    }

    // 直接攻击玩家
    public void AttackPlayer(Player targetPlayer)
    {
        if (!CanAttack || targetPlayer == null) return;

        Debug.Log($"{CardName} 直接攻击 {targetPlayer.playerName}");
        targetPlayer.TakeDamage(Attack);

        // 标记为已攻击
        canAttackThisTurn = false;

        // 更新外观
        UpdateCardAppearance();
    }

    // 重置攻击状态（每回合开始调用）
    public void ResetAttackStatus()
    {
        canAttackThisTurn = true;
        UpdateCardAppearance();
    }

    // 死亡处理
    void OnDeath()
    {
        Debug.Log($"{CardName} 已死亡");

        // 触发死亡事件
        GameManager.Instance?.OnCardDied(this);

        // 移动到墓地
        MoveToGraveyard();
    }

    // 移动到墓地
    public void MoveToGraveyard()
    {
        currentLocation = CardLocation.Graveyard;

        // 找到对应的墓地
        Transform graveyard = isPlayerCard ?
            GameManager.Instance?.playerGraveyard :
            GameManager.Instance?.enemyGraveyard;

        if (graveyard != null)
        {
            transform.SetParent(graveyard);
            transform.localPosition = Vector3.zero;
        }

        // 禁用交互
        isDraggable = false;
        gameObject.SetActive(false);

        UpdateCardAppearance();
    }

    // ==================== 视觉效果 ====================

    // 伤害效果
    IEnumerator DamageEffect()
    {
        if (cardFrame == null) yield break;

        Color originalColor = cardFrame.color;
        cardFrame.color = damagedColor;

        // 闪烁效果
        for (int i = 0; i < 3; i++)
        {
            cardFrame.enabled = false;
            yield return new WaitForSeconds(0.1f);
            cardFrame.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        cardFrame.color = originalColor;
    }

    // 治疗效果
    IEnumerator HealEffect()
    {
        if (healthText == null) yield break;

        Color originalColor = healthText.color;
        healthText.color = Color.green;

        yield return new WaitForSeconds(0.5f);

        healthText.color = originalColor;
    }

    // 高亮效果
    public void SetHighlight(bool highlight)
    {
        if (cardFrame == null) return;

        if (highlight)
        {
            cardFrame.color = highlightColor;
            cardFrame.enabled = true;
        }
        else
        {
            UpdateCardAppearance();
        }
    }

    // ==================== 位置管理 ====================

    // 设置卡牌位置
    public void SetCardLocation(CardLocation location)
    {
        currentLocation = location;
        UpdateCardAppearance();
    }

    // 移动到指定位置
    public void MoveToLocation(Transform targetParent, Vector3 localPosition)
    {
        if (targetParent == null) return;

        transform.SetParent(targetParent);
        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;

        UpdateCardAppearance();
    }

    // ==================== 拖拽系统 ====================

    // 开始拖拽
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || !isPlayerCard) return;
        if (currentLocation != CardLocation.Hand) return;

        isBeingDragged = true;
        startDragPosition = transform.position;
        startDragParent = transform.parent;

        // 禁用射线检测，防止遮挡
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        // 放大卡牌
        transform.localScale = Vector3.one * dragScale;

        // 置顶显示
        transform.SetAsLastSibling();

        Debug.Log($"开始拖拽: {CardName}");
    }

    // 拖拽中
    public void OnDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;

        // 跟随鼠标位置
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint
        );

        transform.position = worldPoint;
    }

    // 结束拖拽
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;

        isBeingDragged = false;

        // 恢复射线检测
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // 恢复大小
        UpdateCardAppearance();

        // 检查是否放置到卡槽
        bool placedSuccessfully = CheckDropTarget(eventData);

        if (!placedSuccessfully)
        {
            // 返回原位置
            ReturnToStartPosition();
        }

        Debug.Log($"结束拖拽: {CardName}, 放置{(placedSuccessfully ? "成功" : "失败")}");
    }

    // 检查放置目标
    bool CheckDropTarget(PointerEventData eventData)
    {
        // 获取鼠标下方的对象
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            Debug.Log("放置失败：下方没有对象");
            return false;
        }

        // 检查是否是卡槽
        CardSlot slot = eventData.pointerCurrentRaycast.gameObject.GetComponent<CardSlot>();
        if (slot != null && slot.CanAcceptCard(this))
        {
            // 尝试放置到卡槽
            if (slot.TryPlaceCard(this))
            {
                // 放置成功，从手牌移除
                RemoveFromHand();
                return true;
            }
        }

        Debug.Log("放置失败：目标不是有效卡槽或卡槽已满");
        return false;
    }

    // 返回起始位置
    void ReturnToStartPosition()
    {
        transform.position = startDragPosition;
        transform.SetParent(startDragParent);
        UpdateCardAppearance();
    }

    // 从手牌移除
    void RemoveFromHand()
    {
        if (isPlayerCard)
        {
            GameManager.Instance?.player?.RemoveCardFromHand(gameObject);
        }
        else
        {
            GameManager.Instance?.enemy?.RemoveCardFromHand(gameObject);
        }
    }

    // 鼠标点击（双击放置）
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && currentLocation == CardLocation.Hand && isPlayerCard)
        {
            OnDoubleClick();
        }
    }

    // 双击事件
    void OnDoubleClick()
    {
        Debug.Log($"双击卡牌: {CardName}");

        // 尝试自动放置到空卡槽
        CardSlot emptySlot = GameManager.Instance?.GetEmptyPlayerSlot();
        if (emptySlot != null && emptySlot.TryPlaceCard(this))
        {
            RemoveFromHand();
            UIManager.Instance?.ShowMessage($"{CardName} 已放置到战场", false);
        }
        else
        {
            UIManager.Instance?.ShowMessage("没有可用的卡槽位置", true);
        }
    }

    // ==================== 工具方法 ====================

    // 重置卡牌状态
    public void ResetCard()
    {
        currentHealth = MaxHealth;
        canAttackThisTurn = true;
        isBeingDragged = false;
        currentLocation = CardLocation.Deck;

        UpdateCardUI();
        UpdateCardAppearance();

        // 重置变换
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    // 检查卡牌是否存活
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    // 获取卡牌信息字符串
    public string GetCardInfo()
    {
        return $"{CardName} [攻{Attack}/血{currentHealth}/{MaxHealth}]";
    }
}
