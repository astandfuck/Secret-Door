using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 卡牌数据类（用于存储卡牌信息）
[System.Serializable]
public class CardData
{
    [Header("基础信息")]
    public string cardID;            // 卡牌唯一ID
    public string cardName;          // 卡牌名称
    [TextArea(2, 3)]
    public string description;       // 卡牌描述

    [Header("战斗属性")]
    public int health;               // 生命值
    public int attack;               // 攻击力
    public int manaCost;             // 法力消耗（预留）

    [Header("视觉效果")]
    public Sprite cardArt;           // 卡牌图案
    public Color cardColor = Color.white; // 卡牌颜色

    [Header("游戏逻辑")]
    public bool isPlayerCard = true; // 是否为玩家卡牌
    public CardLocation startLocation = CardLocation.Deck; // 起始位置

    // 构造函数
    public CardData(string id, string name, int hp, int atk)
    {
        cardID = id;
        cardName = name;
        health = hp;
        attack = atk;
        description = $"{name} - 攻击:{atk} 生命:{hp}";
    }

    // 复制构造函数
    public CardData(CardData other)
    {
        cardID = other.cardID;
        cardName = other.cardName;
        description = other.description;
        health = other.health;
        attack = other.attack;
        manaCost = other.manaCost;
        cardArt = other.cardArt;
        cardColor = other.cardColor;
        isPlayerCard = other.isPlayerCard;
        startLocation = other.startLocation;
    }
}