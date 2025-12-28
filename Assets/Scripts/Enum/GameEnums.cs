using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 游戏状态枚举
public enum GameState
{
    None,           // 无状态
    PlayerTurn,     // 玩家回合
    EnemyTurn,      // 敌方回合
    BattlePhase,    // 战斗阶段
    GameOver        // 游戏结束
}

// 卡牌位置枚举
public enum CardLocation
{
    Deck,           // 卡组中
    Hand,           // 手牌中
    Battlefield,    // 战场上
    Graveyard       // 墓地中
}