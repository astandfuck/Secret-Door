using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("玩家属性")]
    public string playerName = "玩家";
    public int health = 30;
    public int maxHealth = 30;
    public bool isHumanPlayer = true; // true=玩家, false=AI

    [Header("卡牌管理")]
    public List<GameObject> deck = new List<GameObject>();      // 卡组
    public List<GameObject> handCards = new List<GameObject>(); // 手牌
    public List<GameObject> graveyard = new List<GameObject>(); // 墓地

    [Header("位置引用")]
    public Transform handArea;      // 手牌区域
    public Transform deckPosition;  // 牌堆位置
    public Transform graveyardArea; // 墓地位置

    // 事件
    public delegate void HealthChanged(int newHealth, int maxHealth);
    public event HealthChanged OnHealthChanged;

    // ==================== 初始化 ====================

    public void Initialize(string name, int startHealth, bool isHuman)
    {
        playerName = name;
        health = startHealth;
        maxHealth = startHealth;
        isHumanPlayer = isHuman;

        Debug.Log($"{playerName} 初始化完成 - 生命值: {health}");
    }

    // ==================== 生命值管理 ====================

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int previousHealth = health;
        health -= damage;

        if (health < 0) health = 0;

        Debug.Log($"{playerName} 受到 {damage} 点伤害 ({previousHealth} -> {health})");

        // 触发事件
        OnHealthChanged?.Invoke(health, maxHealth);

        // 检查是否死亡
        if (health <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int previousHealth = health;
        health += amount;

        if (health > maxHealth) health = maxHealth;

        Debug.Log($"{playerName} 恢复 {amount} 点生命 ({previousHealth} -> {health})");

        // 触发事件
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    void OnDeath()
    {
        Debug.Log($"{playerName} 已死亡！");

        // 通知GameManager
        GameManager.Instance.EndGame(!isHumanPlayer);
    }

    // ==================== 卡牌管理 ====================

    // 添加卡牌到手牌
    public void AddCardToHand(GameObject card)
    {
        if (card == null) return;

        handCards.Add(card);
        Debug.Log($"{playerName} 获得手牌: {card.name}, 当前手牌数: {handCards.Count}");
    }

    // 从手牌移除卡牌
    public void RemoveCardFromHand(GameObject card)
    {
        if (card == null) return;

        if (handCards.Remove(card))
        {
            Debug.Log($"{playerName} 移除手牌: {card.name}, 剩余手牌数: {handCards.Count}");
        }
    }

    // 移动到墓地
    public void MoveToGraveyard(GameObject card)
    {
        if (card == null) return;

        graveyard.Add(card);
        Debug.Log($"{playerName} 卡牌进入墓地: {card.name}, 墓地卡牌数: {graveyard.Count}");
    }

    // ==================== 工具方法 ====================

    public bool IsAlive()
    {
        return health > 0;
    }

    public string GetHealthInfo()
    {
        return $"{health}/{maxHealth}";
    }

    public string GetCardCountInfo()
    {
        return $"手牌: {handCards.Count}, 卡组: {deck.Count}, 墓地: {graveyard.Count}";
    }
}