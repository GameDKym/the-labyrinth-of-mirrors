using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }
    [SerializeField] private PlayerSO playerSO; // PlayerSO를 Inspector에서 연결
    private void Start()
    {
        InitializePlayers();
    }
    private void InitializePlayers()
    {
        for (int i = 0; i < playerSO.players.Count; i++)
        {
            playerSO.players[i].id = i; // ID 부여
        }
    }
    private void Awake()
    {
        Inst = this;

        if (playerSO == null || playerSO.players == null || playerSO.players.Count == 0)
        {
            Debug.LogError("PlayerSO가 설정되지 않았거나 플레이어 데이터가 없습니다!");
            return;
        }
    }
    // 모든 플레이어 가져오기
    public List<PlayerData> GetAllPlayers()
    {
        return playerSO.players;
    }
    // 데미지 적용
    public void ApplyDamageToTarget(int damage, TargetType targetType, PlayerData currentPlayer)
        {
            List<PlayerData> allPlayers = GetAllPlayers();
            int repeatCount = 0;
            switch (targetType)
            {
                case TargetType.Self:
                    ApplyHealthChange(currentPlayer, -damage);
                    break;

                case TargetType.Single:
                    PlayerData singleTarget = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
                    Debug.Log($"[ApplyHealthChange] PlayerManager에전달된 PlayerData 정보: " +
                    $"ID: {singleTarget.id}, Name: {singleTarget.playerName}, " +
                    $"Health: {singleTarget.health}, BaseHealth: {singleTarget.characterData.baseHealth}");
                    if (singleTarget != null)
                    {
                    damage += currentPlayer.attack;
                    ApplyHealthChange(singleTarget, -damage);
                    }
                    else
                    {
                    Debug.LogWarning("타겟이 선택되지 않았습니다!");
                    }
                    if (TurnManager.Inst.myTurn)  // 내 턴일 경우만
                    {
                        Debug.Log($"카드 사용! 현재 myPutCount: {CardManager.Inst.myPutCount}");
                    }

                break;

                case TargetType.All:
                    foreach (var player in allPlayers)
                    {
                        ApplyHealthChange(player, -damage);
                    }
                    break;
           
                case TargetType.AllExceptSelf:
                    damage += currentPlayer.attack;
                    foreach (var player in allPlayers)
                    {
                        if (repeatCount >= 3) break;
                        PlayerData AllExceptSelfTarget = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
                        if (AllExceptSelfTarget != null)
                        {
                            ApplyHealthChange(AllExceptSelfTarget, -damage);
                        }
                        else
                        {
                            Debug.LogWarning("타겟이 선택되지 않았습니다!");
                        }

                        repeatCount++;
                    }
                    break;

                default:
                    Debug.LogWarning("올바르지 않은 타겟 타입입니다.");
                    break;
            }
        }
    // 체력 변경 (데미지/회복)
    private void ApplyHealthChange(PlayerData currentPlayer, int amount)
    {
        currentPlayer.health += amount;
        Debug.Log($"{currentPlayer.playerName}의 체력이 {amount}만큼 변경되었습니다. 현재 체력: {currentPlayer.health}");
        UpdatePlayerHealth(currentPlayer);
    }
    // UI 업데이트
    public void UpdatePlayerHealth(PlayerData player)
    {
        // FindObjectsOfType로 PlayerUI를 찾고 이름으로 정렬
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        playerUIs = playerUIs.OrderBy(ui => ui.gameObject.name).ToArray();
        for (int i = 0; i < playerUIs.Length; i++)
        {
            Debug.Log($"[{i}] PlayerUI - GameObject Name: {playerUIs[i].gameObject.name}");
        }

        // ID를 기준으로 PlayerUI를 매핑
        int playerIndex = playerSO.players.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < playerSO.players.Count)
        {
            PlayerUI playerUI = playerUIs[playerIndex];
            playerUI.UpdateHealth(player.health);
        }
        else
        {
            Debug.LogWarning($"UI를 업데이트할 수 없습니다. {player.playerName}의 UI를 찾을 수 없습니다.");
        }

        player.CheckDeath();
    }
    // 플레이어 체력 회복
    public void HealPlayer(PlayerData currentPlayer, TargetType targetType, int heal)
    {
        List<PlayerData> allPlayers = GetAllPlayers();
        switch (targetType)
        {
            case TargetType.Self:
                Debug.Log($"PlayerManager -> HealPlayer -> Self");
                PlayerData selfTarget = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
                ApplyHealthChange(selfTarget, heal);
                break;
            case TargetType.All:
                Debug.Log($"PlayerManager -> HealPlayer -> All");
                foreach (var player in allPlayers)
                {
                    PlayerData allTarget = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
                    ApplyHealthChange(allTarget, +heal);
                }
                break;

            default:
                Debug.LogWarning("올바르지 않은 타겟 타입입니다.");
                break;
        }
    }
    // 방어 활성화
    public void ActivateDefense(PlayerData player)
    {
        Debug.Log($"{player.playerName} 방어 활성화");
        player.isStealthed = true; // 방어 또는 은신 상태 적용
    }
    // 아이템 장착
}
