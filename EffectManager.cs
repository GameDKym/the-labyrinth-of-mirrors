using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    public void ExecuteSpecialEffect(Item item, PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        switch (item.name)
        {
            case "무차별 난사":
                ApplyAreaDamage(item.damage, item.targetType, currentPlayer, allPlayers);
                break;
            case "결투":
                StartDuel(currentPlayer, allPlayers);
                break;
            case "주점":
                HealAllPlayers(item.heal, allPlayers);
                break;
            case "강탈":
                StealCard(currentPlayer, allPlayers);
                break;
            default:
                Debug.LogWarning($"알 수 없는 특수 효과: {item.name}");
                break;
        }
    }

    private void ApplyAreaDamage(int damage, TargetType targetType, PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        Debug.Log($"범위 데미지: {damage} (타겟 타입: {targetType})");

        // 타겟 타입에 따른 처리
        switch (targetType)
        {
            case TargetType.All:
                foreach (var player in allPlayers)
                {
                    player.health -= damage;
                    Debug.Log($"{player.playerName}에게 {damage}의 데미지를 입혔습니다. 현재 체력: {player.health}");
                }
                break;

            case TargetType.AllExceptSelf:
                foreach (var player in allPlayers)
                {
                    if (player != currentPlayer)
                    {
                        player.health -= damage;
                        Debug.Log($"{player.playerName}에게 {damage}의 데미지를 입혔습니다. 현재 체력: {player.health}");
                    }
                }
                break;

            default:
                Debug.LogWarning("타겟 타입이 올바르지 않습니다.");
                break;
        }
    }

    private void StartDuel(PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        Debug.Log("결투 시작!");
        // 두 플레이어 간 결투 로직
        PlayerData target = SelectTarget(allPlayers);
        Debug.Log($"{currentPlayer.playerName}와(과) {target.playerName}이(가) 결투를 시작합니다!");
        // 결투 로직 구현 필요
    }

    private void HealAllPlayers(int healAmount, List<PlayerData> allPlayers)
    {
        Debug.Log($"모든 플레이어가 체력을 {healAmount}만큼 회복합니다.");
        foreach (var player in allPlayers)
        {
            player.health += healAmount;
            Debug.Log($"{player.playerName}의 체력이 {healAmount}만큼 회복되었습니다. 현재 체력: {player.health}");
        }
    }

    private void StealCard(PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        PlayerData target = SelectTarget(allPlayers);
        Debug.Log($"{currentPlayer.playerName}가(이) {target.playerName}의 카드를 강탈합니다.");
        // 타겟 플레이어의 카드 강탈 로직
        if (target.weaponCard != null)
        {
            currentPlayer.weaponCard = target.weaponCard;
            target.weaponCard = null;
            Debug.Log($"{currentPlayer.playerName}가 {target.playerName}의 무기 카드를 강탈했습니다.");
        }
    }

    private PlayerData SelectTarget(List<PlayerData> allPlayers)
    {
        // 간단한 타겟 선택 로직 (첫 번째 플레이어 선택)
        foreach (var player in allPlayers)
        {
            if (player != null)
                return player;
        }
        return null;
    }
}
