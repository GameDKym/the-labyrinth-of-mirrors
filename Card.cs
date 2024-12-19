using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class Card : MonoBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] SpriteRenderer cardSpriteRenderer; // ī�� ���
    [SerializeField] SpriteRenderer characterSpriteRenderer; // ī�� ���
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text effectTMP;

    [Header("Card Sprites")]
    [SerializeField] Sprite cardFrontSprite;
    [SerializeField] Sprite cardBackSprite;

    [Header("Card Data")]
    public PRS originPRS; // ī���� ���� ��ġ�� ȸ��, ũ�� ����
    public Item item;     // ī���� ������
    private bool isFront; // ī�尡 �ո����� ����
    public string SelectType;

    public void Setup(Item item, bool isFront)
    {
        this.item = item;
        this.isFront = isFront;

        if (this.isFront)
        {
            cardSpriteRenderer.sprite = cardFrontSprite;
            characterSpriteRenderer.sprite = this.item.sprite; // ĳ���� ��������Ʈ ����
            nameTMP.text = this.item.name;
            effectTMP.text = this.item.effect;
        }
        else
        {
            // ī�� �޸� ������ ��ǥ��
            cardSpriteRenderer.sprite = cardBackSprite;
            nameTMP.text = "";
            effectTMP.text = "";
        }
    }
    public void UseCard(PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        if (currentPlayer.id == 0)
        {
            PlayerData targetPlayer = allPlayers.FirstOrDefault(player => player.id == 0);
            if (targetPlayer == null)
            {
                Debug.LogError("ID�� 0�� �÷��̾ ã�� �� �����ϴ�!");
                return;
            }

            // ã�� �÷��̾��� ������ currentPlayer ��� ���
            currentPlayer = targetPlayer;
            Debug.Log($"{currentPlayer.playerName}");
        }

        SelectType = this.item.type;


        switch (item.type)
        {
            case "���� ī��":
                ApplyDamage(currentPlayer);
                currentPlayer.attackCount++;
                Debug.Log($"{SelectType} �Դϴ�.");
                break;

            case "ȸ�� ī��":
                HealPlayer(currentPlayer);
                break;

            case "��� ī��":
                ActivateDefense(currentPlayer);
                break;

            case "Ư�� ī��":
                ApplySpecialEffect(currentPlayer);
                break;

            case "���� ī��":
                EquipItem(currentPlayer, allPlayers);
                break;

            default:
                Debug.LogWarning($"�� �� ���� ī�� ����: {item.type}");
                break;
        }

        // ī�� ��� �� ����
        Destroy(gameObject);
    }
    private void ApplyDamage(PlayerData currentPlayer)
    {
        // PlayerSO���� ������ ��������
        List<PlayerData> allPlayers = PlayerManager.Inst.GetAllPlayers();
        
        if (currentPlayer.id == 0)
        {
            PlayerData targetPlayer = allPlayers.FirstOrDefault(player => player.id == 0);
            if (targetPlayer == null)
            {
                Debug.LogError("ID�� 0�� �÷��̾ ã�� �� �����ϴ�!");
                return;
            }

            // ã�� �÷��̾��� ������ currentPlayer ��� ���
            currentPlayer = targetPlayer;
        }

        // TargetType�� ���� ó��
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("�������� �ڽſ��� ����˴ϴ�.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.Self, currentPlayer);
                break;

            case TargetType.Single:
                Debug.Log("�������� Ư�� ��󿡰� ����˴ϴ�.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.Single, currentPlayer);
                break;

            case TargetType.All:
                Debug.Log("�������� ��� �÷��̾�� ����˴ϴ�.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.All, currentPlayer);
                break;

            case TargetType.AllExceptSelf:
                Debug.Log("�������� �ڽ��� ������ ��� �÷��̾�� ����˴ϴ�.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.AllExceptSelf, currentPlayer);
                break;

            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void HealPlayer(PlayerData currentPlayer)
    {
        Debug.Log($"HealPlayerī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("ü���� ȸ���˴ϴ�.");
                PlayerManager.Inst.HealPlayer(currentPlayer, TargetType.Self, item.heal);
                break;

            case TargetType.All:
                Debug.Log("��� �÷��̾��� ü���� ȸ���˴ϴ�.");
                PlayerManager.Inst.HealPlayer(currentPlayer, TargetType.All, item.heal);
                break;

            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void ActivateDefense(PlayerData currentPlayer)
    {
        Debug.Log($"ī�� {item.name} ���: {item.effect}");
        PlayerManager.Inst.ActivateDefense(currentPlayer);
    }
    private void ApplySpecialEffect(PlayerData currentPlayer)
    {
        Debug.Log($"Card -> ApplySpecialEffect : {item.name}");
        switch (item.name)
        {
            case "������ ����":
                ApplyDamage(currentPlayer);
                break;

            case "����":
                HealPlayer(currentPlayer);
                break;

            case "��Ż":
                extortion(currentPlayer);
                break;

            case "����":
                Curse(currentPlayer);
                break;

            case "������ ����":
                AWanderingMerchant(currentPlayer);
                break;

            case "������ ����":
                GiftFairy(currentPlayer);
                break;

            case "Ȳ�� ���":
                GoldenGoblin(currentPlayer);
                break;

            default:
                Debug.LogWarning("�� �� ���� ī�� �Դϴ�");
                break;
        }
    }
    private void extortion(PlayerData currentPlayer) //��Ż
    {
        Debug.Log($"extortion ī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Single:
                Debug.Log("��� ī�带 ���Ŀɴϴ�.");
                // ��Ż ȿ�� ����
                CardManager.Inst.extortion(currentPlayer, TargetType.Single);
                break;

            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void EquipItem(PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        PlayerUI targetPlayerUI = null;

        // ��� PlayerUI �߿��� ���� �÷��̾�� ����� UI�� ã��
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        foreach (var playerUI in playerUIs)
        {
            if (playerUI.currentPlayerData == currentPlayer) // PlayerData�� PlayerUI ���� Ȯ��
            {
                targetPlayerUI = playerUI;
                break;
            }
        }

        // ���� ī�� ȿ�� ����
        if (currentPlayer.weaponCard != null)
        { 
            Debug.Log($"���� ���� ī�� ����: {currentPlayer.weaponCard.name}");
            RemoveCardEffect(currentPlayer, currentPlayer.weaponCard);
        }

        // ���ο� ī��� ��ü
        currentPlayer.weaponCard = item;

        // ���ο� ī�� ȿ�� ����
        switch (item.name)
        {
            case "��æƮ - 1":
                currentPlayer.attack += 1; // ���ݷ� 1 ����
                break;

            case "��æƮ - 2":
                currentPlayer.attack += 2; // ���ݷ� 2 ����
                break;

            case "��æƮ - 3":
                currentPlayer.attack += 3; // ���ݷ� 3 ����
                break;

            case "��æƮ - 4":
                currentPlayer.attack += 4; // ���ݷ� 4 ����
                break;

            case "��æƮ - 1+":
                currentPlayer.attackCount -= 1; // ���ݷ� 3 ����
                break;

            case "��æƮ - 2+":
                currentPlayer.attackCount -= 2; // ���ݷ� 4 ����
                break;

            default:
                Debug.LogWarning($"�� �� ���� ���� ī��: {item.name}");
                break;
        }

        // UI ������Ʈ
        if (targetPlayerUI != null)
        {
            Debug.Log($"UI ������Ʈ: {item.name}");
            targetPlayerUI.UpdateItemImages(item, currentPlayer);
        }
        else
        {
            Debug.LogWarning("targetPlayerUI�� ã�� �� �����ϴ�!");
        }
    }
    private void Curse(PlayerData currentPlayer)
    {
        Debug.Log($"Curse ī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Single:
                Debug.Log("��� ī�带 ������ �մϴ�.");
                // ��Ż ȿ�� ����
                CardManager.Inst.curse(currentPlayer, TargetType.Single);
                break;

            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void AWanderingMerchant(PlayerData currentPlayer)
    {
        Debug.Log($"A Wandering Merchant ī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("���� ī�� �� ���� ����ϴ�.");
                CardManager.Inst.AWanderingMerchant(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void GiftFairy(PlayerData currentPlayer)
    {
        Debug.Log($"Gift Fairy ī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("ī�� ���̿��� ī�� �� ���� �����´�.");
                CardManager.Inst.GiftFairy(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void GoldenGoblin(PlayerData currentPlayer)
    {
        Debug.Log($"Golden Goblin ī�� {item.name} ���: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("ī�� ���̿��� ī�� �� ���� �����´�.");
                CardManager.Inst.GoldenGoblin(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"�� �� ���� TargetType: {item.targetType}");
                break;
        }
    }
    private void RemoveCardEffect(PlayerData currentPlayer, Item previousCard)
    {
        if (previousCard == null)
        {
            Debug.LogWarning("���� ������ ī�尡 �����ϴ�.");
            return;
        }

        // ī�� ���� �����
        Debug.Log($"���� ī�� ����: {previousCard.name}, ���ݷ� ���� ��: {currentPlayer.attack}");

        // ���� ī���� �̸��� ���� ���ݷ� ����
        switch (previousCard.name)
        {
            case "��æƮ - 1":
                currentPlayer.attack -= 1;
                break;
            case "��æƮ - 2":
                currentPlayer.attack -= 2;
                break;
            case "��æƮ - 3":
                currentPlayer.attack -= 3;
                break;
            case "��æƮ - 4":
                currentPlayer.attack -= 4;
                break;
            default:
                Debug.LogWarning($"�� �� ���� ���� ī��: {previousCard.name}");
                break;
        }

        // ���ݷ��� 0 �̸����� �������� �ʵ��� ���
        if (currentPlayer.attack < 0)
        {
            currentPlayer.attack = 0;
            Debug.LogWarning("���ݷ��� 0 �̸����� �����߽��ϴ�. 0���� �ʱ�ȭ�մϴ�.");
        }

        Debug.Log($"���� ���� ī�� ���� �� ���ݷ�: {currentPlayer.attack}");
    }
    void OnMouseOver()
    {
        if (isFront)
            CardManager.Inst.CardMouseOver(this);
    }
    void OnMouseExit()
    {
        if (isFront)
            CardManager.Inst.CardMouseExit(this);
    }
    void OnMouseDown()
    {
        if (isFront)
            CardManager.Inst.CardMouseDown();
    }
    void OnMouseUp()
    {
        if (isFront)
            CardManager.Inst.CardMouseUp();
    }
    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
}
