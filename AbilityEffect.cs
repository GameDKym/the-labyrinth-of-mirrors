using UnityEngine;

public enum AbilityEffect
{
    None,                     // 능력 없음
    AddAttackAndDefense,      // 드루이드: 공격력과 방어력 +1
    AttackSuccessOnDraw,      // 레인저: 드로우로 공격 성공 판단
    StealthAttackBonus,       // 로그: 은신 상태에서 피해량 +2
    EvadeAttack,              // 뭉크: 50% 확률로 회피
    GiveCardToAlly,           // 바드: 아군에게 카드 전달
    DoubleDamageOnFirstAttack, // 바바리안: 첫 공격 2배 피해
    DrawExtraCardOnTurnStart, // 아르카나: 턴 시작 시 1장 추가 드로우
    MagicAttack,              // 워락: 공격 시 피해량 +2
    MultiAttack,              // 위저드: 2회 공격 가능
    HealSelfOrAlly,           // 성직자: 턴마다 체력 회복
    AddDefenseEveryTurn,      // 파이터: 매턴 방어력 +1
    HealOnAttack              // 팔라딘: 적 공격 시 체력 회복
}




