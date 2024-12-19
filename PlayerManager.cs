using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Inst { get; private set; }
    [SerializeField] private PlayerSO playerSO; // PlayerSO�� Inspector���� ����
    private void Start()
    {
        InitializePlayers();
    }
    private void InitializePlayers()
    {
        for (int i = 0; i < playerSO.players.Count; i++)
        {
            playerSO.players[i].id = i; // ID �ο�
        }
    }
    private void Awake()
    {
        Inst = this;

        if (playerSO == null || playerSO.players == null || playerSO.players.Count == 0)
        {
            Debug.LogError("PlayerSO�� �������� �ʾҰų� �÷��̾� �����Ͱ� �����ϴ�!");
            return;
        }
    }
    // ��� �÷��̾� ��������
    public List<PlayerData> GetAllPlayers()
    {
        return playerSO.players;
    }
    // ������ ����
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
                    Debug.Log($"[ApplyHealthChange] PlayerManager�����޵� PlayerData ����: " +
                    $"ID: {singleTarget.id}, Name: {singleTarget.playerName}, " +
                    $"Health: {singleTarget.health}, BaseHealth: {singleTarget.characterData.baseHealth}");
                    if (singleTarget != null)
                    {
                    damage += currentPlayer.attack;
                    ApplyHealthChange(singleTarget, -damage);
                    }
                    else
                    {
                    Debug.LogWarning("Ÿ���� ���õ��� �ʾҽ��ϴ�!");
                    }
                    if (TurnManager.Inst.myTurn)  // �� ���� ��츸
                    {
                        Debug.Log($"ī�� ���! ���� myPutCount: {CardManager.Inst.myPutCount}");
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
                            Debug.LogWarning("Ÿ���� ���õ��� �ʾҽ��ϴ�!");
                        }

                        repeatCount++;
                    }
                    break;

                default:
                    Debug.LogWarning("�ùٸ��� ���� Ÿ�� Ÿ���Դϴ�.");
                    break;
            }
        }
    // ü�� ���� (������/ȸ��)
    private void ApplyHealthChange(PlayerData currentPlayer, int amount)
    {
        currentPlayer.health += amount;
        Debug.Log($"{currentPlayer.playerName}�� ü���� {amount}��ŭ ����Ǿ����ϴ�. ���� ü��: {currentPlayer.health}");
        UpdatePlayerHealth(currentPlayer);
    }
    // UI ������Ʈ
    public void UpdatePlayerHealth(PlayerData player)
    {
        // FindObjectsOfType�� PlayerUI�� ã�� �̸����� ����
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        playerUIs = playerUIs.OrderBy(ui => ui.gameObject.name).ToArray();
        for (int i = 0; i < playerUIs.Length; i++)
        {
            Debug.Log($"[{i}] PlayerUI - GameObject Name: {playerUIs[i].gameObject.name}");
        }

        // ID�� �������� PlayerUI�� ����
        int playerIndex = playerSO.players.IndexOf(player);
        if (playerIndex >= 0 && playerIndex < playerSO.players.Count)
        {
            PlayerUI playerUI = playerUIs[playerIndex];
            playerUI.UpdateHealth(player.health);
        }
        else
        {
            Debug.LogWarning($"UI�� ������Ʈ�� �� �����ϴ�. {player.playerName}�� UI�� ã�� �� �����ϴ�.");
        }

        player.CheckDeath();
    }
    // �÷��̾� ü�� ȸ��
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
                Debug.LogWarning("�ùٸ��� ���� Ÿ�� Ÿ���Դϴ�.");
                break;
        }
    }
    // ��� Ȱ��ȭ
    public void ActivateDefense(PlayerData player)
    {
        Debug.Log($"{player.playerName} ��� Ȱ��ȭ");
        player.isStealthed = true; // ��� �Ǵ� ���� ���� ����
    }
    // ������ ����
}
