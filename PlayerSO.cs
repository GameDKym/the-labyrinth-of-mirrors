using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Game/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public List<PlayerData> players = new List<PlayerData>(); // �÷��̾� ������ ����Ʈ

    public void InitializePlayers()
    {
        foreach (var player in players)
        {
            player.Initialize();  // �⺻ ü�� �� ���ݷ� �ʱ�ȭ
            player.isRoleRevealed = false;  // ���� ���� ���� �ʱ�ȭ
            player.isDead = false;  // ���� ���� �ʱ�ȭ
            player.weaponCard = null;  // ���� ī�� �ʱ�ȭ
            player.potionCard = null;  // ���� ī�� �ʱ�ȭ
            player.debufferCard = null;  // ����� ī�� �ʱ�ȭ
            player.isStealthed = false;  // ���� ���� �ʱ�ȭ
            player.isFirstAttack = false;  // ù ���� ���� �ʱ�ȭ
            player.attackCount = 0;  // ���� Ƚ�� �ʱ�ȭ
        }

        Debug.Log("PlayerSO �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }

}

[System.Serializable]
public class PlayerData
{
    public int id; // ���� ID
    public string playerName;           // �÷��̾� �̸�
    public RoleCardSO roleCard;         // ���� ī��
    public CharacterSO characterData;  // ĳ���� ������
    public bool isRoleRevealed;         // ���� ���� ����

    public Item weaponCard;             // ���� ī�� (ItemSO�� ȣȯ)
    public Item potionCard;             // ���� ī�� (ItemSO�� ȣȯ)
    public Item debufferCard;           // ����� ī�� (ItemSO�� ȣȯ)

    [Range(1, 20)] public int health;   // �÷��̾� ü�� (Inspector���� ���� ����)
    [Range(0, 10)] public int attack;   // �÷��̾� ���ݷ�
    [Range(0, 10)] public int defense;  // �÷��̾� ����

    public bool isStealthed;            // ���� ����
    public bool isFirstAttack;          // ù ���� ����
    public int attackCount;

    public bool isDead = false; // ���� ���� (ü���� 0�� �Ǹ� �ڵ����� true)

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
