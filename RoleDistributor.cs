using System.Collections.Generic;
using UnityEngine;

public class RoleDistributor : MonoBehaviour
{
    public List<RoleCardSO> roleCards;       // 역할 카드 리스트 (ScriptableObject로 관리)
    public List<CharacterSO> characters;    // 캐릭터 리스트 (ScriptableObject로 관리)
    public PlayerSO playerSO;               // 플레이어 데이터

    [Header("Player UI Parent")]
    public Transform playerUIParent; // Player UI가 포함된 부모 Canvas 오브젝트
    private List<PlayerUI> playerUIPanels = new List<PlayerUI>();   // UI 패널 리스트 (Inspector에서 연결)

    void Start()
    {
        InitializePlayerUIPanels(); // 자동으로 UI 패널 초기화
        AssignRolesAndCharacters(); // 역할과 캐릭터를 배정
        DisplayAssignedData();      // 배정된 데이터를 출력
        UpdatePlayerUIs();          // UI 업데이트
    }

    private void InitializePlayerUIPanels()
    {
        playerUIPanels.Clear();

        // Player UI 부모 오브젝트에서 자식들을 검색하여 PlayerUI 컴포넌트 추가
        foreach (Transform child in playerUIParent)
        {
            var playerUI = child.GetComponent<PlayerUI>();
            if (playerUI != null)
            {
                playerUIPanels.Add(playerUI);
            }
        }

        Debug.Log($"Initialized {playerUIPanels.Count} PlayerUI panels.");
    }

    public void AssignRolesAndCharacters()
    {
        List<RoleCardSO> availableRoles = new List<RoleCardSO>(roleCards);
        List<CharacterSO> availableCharacters = new List<CharacterSO>(characters);

        ShuffleList(availableRoles);
        ShuffleList(availableCharacters);

        for (int i = 0; i < playerSO.players.Count; i++)
        {
            var player = playerSO.players[i];
            player.roleCard = availableRoles[i];
            player.characterData = availableCharacters[i];

            player.playerName = player.characterData.characterName;
            player.health = player.characterData.baseHealth;
            player.attack = player.characterData.baseAttack;
            player.defense = player.characterData.baseDefense;

            if (player.roleCard.roleType == Role.TeamLeader)
            {
                player.isRoleRevealed = true; // 공대장 역할만 공개
            }
            else
            {
                player.isRoleRevealed = false; // 나머지 역할은 비공개
            }

            player.isStealthed = false;
            player.isFirstAttack = true;
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void UpdatePlayerUIs()
    {
        for (int i = 0; i < playerSO.players.Count; i++)
        {
            bool isMyUI = (i == 0); // 첫 번째 플레이어가 나 자신
            playerUIPanels[i].Setup(playerSO.players[i], isMyUI);
        }
    }

    public void DisplayAssignedData()
    {
        foreach (var player in playerSO.players)
        {
            Debug.Log($"Player: {player.playerName}, Role: {player.roleCard.roleName}, Revealed: {player.isRoleRevealed}");
        }
    }
}
