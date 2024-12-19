using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Game/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public List<PlayerData> players = new List<PlayerData>(); // 플레이어 데이터 리스트

    public void InitializePlayers()
    {
        foreach (var player in players)
        {
            player.Initialize();  // 기본 체력 및 공격력 초기화
            player.isRoleRevealed = false;  // 역할 공개 여부 초기화
            player.isDead = false;  // 죽음 여부 초기화
            player.weaponCard = null;  // 무기 카드 초기화
            player.potionCard = null;  // 포션 카드 초기화
            player.debufferCard = null;  // 디버프 카드 초기화
            player.isStealthed = false;  // 은신 여부 초기화
            player.isFirstAttack = false;  // 첫 공격 여부 초기화
            player.attackCount = 0;  // 공격 횟수 초기화
        }

        Debug.Log("PlayerSO 데이터가 초기화되었습니다.");
    }

}

[System.Serializable]
public class PlayerData
{
    public int id; // 고유 ID
    public string playerName;           // 플레이어 이름
    public RoleCardSO roleCard;         // 역할 카드
    public CharacterSO characterData;  // 캐릭터 데이터
    public bool isRoleRevealed;         // 역할 공개 여부

    public Item weaponCard;             // 무기 카드 (ItemSO와 호환)
    public Item potionCard;             // 포션 카드 (ItemSO와 호환)
    public Item debufferCard;           // 디버프 카드 (ItemSO와 호환)

    [Range(1, 20)] public int health;   // 플레이어 체력 (Inspector에서 조정 가능)
    [Range(0, 10)] public int attack;   // 플레이어 공격력
    [Range(0, 10)] public int defense;  // 플레이어 방어력

    public bool isStealthed;            // 은신 여부
    public bool isFirstAttack;          // 첫 공격 여부
    public int attackCount;

    public bool isDead = false; // 죽음 여부 (체력이 0이 되면 자동으로 true)

    public void Initialize()
    {
        playerName = characterData.characterName;
        health = characterData.baseHealth;
        attack = characterData.baseAttack;
        isDead = false;
    }
    public void CheckDeath()
    {
        if (health <= 0)
        {
            isDead = true;
        }
    }
}
