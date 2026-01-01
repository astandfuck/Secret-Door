using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("卡槽设置")]
    public int slotIndex = 0;                      // 卡槽索引（0-9）
    public bool isPlayerSlot = true;               // 是否为玩家卡槽
    public Card currentCard = null;                // 当前卡牌

    [Header("UI组件")]
    public Image slotBackground;                   // 背景图片
    public Image slotBorder;                       // 边框图片
    public Text slotIndexText;                     // 索引文本（调试用）
    public GameObject occupiedIndicator;           // 占用指示器

    [Header("视觉效果")]
    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);      // 空槽颜色
    public Color occupiedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);   // 占用颜色
    public Color highlightColor = new Color(0.5f, 0.5f, 0.2f, 0.7f);  // 高亮颜色
    public Color validDropColor = Color.green;                        // 有效放置颜色
    public Color invalidDropColor = Color.red;                        // 无效放置颜色

    private Color originalColor;                   // 原始颜色
    private bool isHighlighted = false;            // 是否高亮

    // ==================== Unity生命周期 ====================

    void Start()
    {
        // 保存原始颜色
        if (slotBackground != null)
            originalColor = slotBackground.color;

        // 显示索引（调试用）
        if (slotIndexText != null)
            slotIndexText.text = slotIndex.ToString();

        // 初始化外观
        UpdateAppearance();
    }

    // ==================== 卡牌管理 ====================

    // 尝试放置卡牌
    public bool TryPlaceCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("尝试放置空卡牌");
            return false;
        }

        if (!CanAcceptCard(card))
        {
            Debug.Log($"卡槽 {slotIndex} 无法接受卡牌 {card.CardName}");
            return false;
        }

        // 放置卡牌
        PlaceCard(card);
        return true;
    }

    // 放置卡牌
    public void PlaceCard(Card card)
    {
        if (currentCard != null)
        {
            Debug.LogWarning($"卡槽 {slotIndex} 已被占用，无法放置 {card.CardName}");
            return;
        }

        currentCard = card;

        // 设置卡牌位置和父对象
        card.transform.SetParent(transform, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;

        // 设置卡牌状态
        card.SetCardLocation(CardLocation.Battlefield);
        card.ResetAttackStatus(); // 新放置的卡牌可以攻击

        // 更新外观
        UpdateAppearance();

        // 播放放置效果
        StartCoroutine(PlacementEffect());

        Debug.Log($"卡牌 {card.CardName} 已放置到卡槽 {slotIndex}");

        // 显示消息
        UIManager.Instance?.ShowMessage($"{card.CardName} 进入战场", false);
    }

    // 移除卡牌
    public Card RemoveCard()
    {
        if (currentCard == null)
        {
            Debug.LogWarning($"卡槽 {slotIndex} 没有卡牌可移除");
            return null;
        }

        Card removedCard = currentCard;
        currentCard = null;

        // 更新外观
        UpdateAppearance();

        Debug.Log($"卡牌 {removedCard.CardName} 已从卡槽 {slotIndex} 移除");
        return removedCard;
    }

    // 清空卡槽
    public void ClearSlot()
    {
        if (currentCard != null)
        {
            // 销毁卡牌（实际应该回收）
            Destroy(currentCard.gameObject);
            currentCard = null;
        }

        UpdateAppearance();
    }

    // ==================== 检查方法 ====================

    // 检查是否可以接受卡牌
    public bool CanAcceptCard(Card card)
    {
        if (card == null)
        {
            Debug.Log("卡牌为空");
            return false;
        }

        if (currentCard != null)
        {
            Debug.Log($"卡槽 {slotIndex} 已被占用");
            return false;
        }

        if (isPlayerSlot != card.isPlayerCard)
        {
            Debug.Log($"阵营不匹配: 卡槽({isPlayerSlot}) vs 卡牌({card.isPlayerCard})");
            return false;
        }

        if (card.currentLocation != CardLocation.Hand)
        {
            Debug.Log($"卡牌不在手牌中: {card.currentLocation}");
            return false;
        }

        // 检查游戏状态
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.currentState != GameState.PlayerTurn || gm.isBattlePhase)
        {
            Debug.Log($"当前游戏状态不允许放置卡牌: {gm?.currentState}");
            return false;
        }

        return true;
    }

    // 检查卡槽是否为空
    public bool IsEmpty()
    {
        return currentCard == null;
    }

    // ==================== 外观更新 ====================

    // 更新外观
    void UpdateAppearance()
    {
        // 更新背景颜色
        if (slotBackground != null)
        {
            Color targetColor = currentCard != null ? occupiedColor : emptyColor;

            // 如果正在高亮，覆盖颜色
            if (isHighlighted)
                targetColor = highlightColor;

            slotBackground.color = targetColor;
        }

        // 显示/隐藏占用指示器
        if (occupiedIndicator != null)
            occupiedIndicator.SetActive(currentCard != null);

        // 更新边框
        if (slotBorder != null)
        {
            slotBorder.enabled = currentCard == null; // 空槽显示边框
            slotBorder.color = currentCard == null ? Color.white : Color.clear;
        }
    }

    // 播放放置效果
    IEnumerator PlacementEffect()
    {
        if (slotBackground == null) yield break;

        Color original = slotBackground.color;
        slotBackground.color = highlightColor;

        yield return new WaitForSeconds(0.3f);

        slotBackground.color = original;
        UpdateAppearance();
    }

    // 播放无效放置效果
    IEnumerator InvalidDropEffect()
    {
        if (slotBackground == null) yield break;

        Color original = slotBackground.color;
        slotBackground.color = invalidDropColor;

        yield return new WaitForSeconds(0.5f);

        slotBackground.color = original;
        UpdateAppearance();
    }

    // ==================== 事件处理 ====================

    // 拖拽放入
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
        {
            Debug.Log("拖拽对象为空");
            return;
        }

        Card draggedCard = eventData.pointerDrag.GetComponent<Card>();
        if (draggedCard == null)
        {
            Debug.Log("拖拽对象不是卡牌");
            return;
        }

        // 验证放置条件
        if (CanAcceptCard(draggedCard))
        {
            TryPlaceCard(draggedCard);
        }
        else
        {
            // 播放无效放置效果
            StartCoroutine(InvalidDropEffect());

            // 显示提示消息
            string message = "无法放置卡牌: ";
            if (currentCard != null) message += "卡槽已被占用";
            else if (isPlayerSlot != draggedCard.isPlayerCard) message += "阵营不匹配";
            else message += "当前状态不允许";

            UIManager.Instance?.ShowMessage(message, true);
        }
    }

    // 鼠标进入
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlighted = true;
        UpdateAppearance();

        // 显示提示信息
        if (currentCard != null)
        {
            UIManager.Instance?.ShowTooltip(
                currentCard.CardName,
                $"攻击: {currentCard.Attack} 生命: {currentCard.Health}/{currentCard.MaxHealth}",
                transform.position
            );
        }
        else if (CanAcceptCard(GetDraggedCard()))
        {
            UIManager.Instance?.ShowTooltip(
                "空卡槽",
                $"位置: {slotIndex}\n可放置卡牌",
                transform.position
            );
        }
    }

    // 鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlighted = false;
        UpdateAppearance();

        UIManager.Instance?.HideTooltip();
    }

    // 获取当前拖拽的卡牌
    Card GetDraggedCard()
    {
        // 简化版本：不使用Tag
        foreach (Card card in FindObjectsOfType<Card>())
        {
            if (card.isBeingDragged)
                return card;
        }
        return null;
    }

    // ==================== 工具方法 ====================

    // 获取卡槽位置信息
    public string GetSlotInfo()
    {
        return $"卡槽[{slotIndex}] - {(isPlayerSlot ? "玩家" : "敌方")} - {(IsEmpty() ? "空" : $"卡牌: {currentCard.CardName}")}";
    }

    // 调试信息
    public void DebugInfo()
    {
        Debug.Log(GetSlotInfo());
    }
}