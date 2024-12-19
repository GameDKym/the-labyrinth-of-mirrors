using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class Card : MonoBehaviour
{
    [Header("Visual Elements")]
    [SerializeField] SpriteRenderer cardSpriteRenderer; // 카드 배경
    [SerializeField] SpriteRenderer characterSpriteRenderer; // 카드 배경
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text effectTMP;

    [Header("Card Sprites")]
    [SerializeField] Sprite cardFrontSprite;
    [SerializeField] Sprite cardBackSprite;

    [Header("Card Data")]
    public PRS originPRS; // 카드의 원래 위치와 회전, 크기 정보
    public Item item;     // 카드의 데이터
    private bool isFront; // 카드가 앞면인지 여부
    public string SelectType;

    public void Setup(Item item, bool isFront)
    {
        this.item = item;
        this.isFront = isFront;

        if (this.isFront)
        {
            cardSpriteRenderer.sprite = cardFrontSprite;
            characterSpriteRenderer.sprite = this.item.sprite; // 캐릭터 스프라이트 설정
            nameTMP.text = this.item.name;
            effectTMP.text = this.item.effect;
        }
        else
        {
            // 카드 뒷면 데이터 비표시
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
                Debug.LogError("ID가 0인 플레이어를 찾을 수 없습니다!");
                return;
            }

            // 찾은 플레이어의 정보를 currentPlayer 대신 사용
            currentPlayer = targetPlayer;
            Debug.Log($"{currentPlayer.playerName}");
        }

        SelectType = this.item.type;


        switch (item.type)
        {
            case "공격 카드":
                ApplyDamage(currentPlayer);
                currentPlayer.attackCount++;
                Debug.Log($"{SelectType} 입니다.");
                break;

            case "회복 카드":
                HealPlayer(currentPlayer);
                break;

            case "방어 카드":
                ActivateDefense(currentPlayer);
                break;

            case "특수 카드":
                ApplySpecialEffect(currentPlayer);
                break;

            case "장착 카드":
                EquipItem(currentPlayer, allPlayers);
                break;

            default:
                Debug.LogWarning($"알 수 없는 카드 유형: {item.type}");
                break;
        }

        // 카드 사용 후 삭제
        Destroy(gameObject);
    }
    private void ApplyDamage(PlayerData currentPlayer)
    {
        // PlayerSO에서 데이터 가져오기
        List<PlayerData> allPlayers = PlayerManager.Inst.GetAllPlayers();
        
        if (currentPlayer.id == 0)
        {
            PlayerData targetPlayer = allPlayers.FirstOrDefault(player => player.id == 0);
            if (targetPlayer == null)
            {
                Debug.LogError("ID가 0인 플레이어를 찾을 수 없습니다!");
                return;
            }

            // 찾은 플레이어의 정보를 currentPlayer 대신 사용
            currentPlayer = targetPlayer;
        }

        // TargetType에 따른 처리
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("데미지가 자신에게 적용됩니다.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.Self, currentPlayer);
                break;

            case TargetType.Single:
                Debug.Log("데미지가 특정 대상에게 적용됩니다.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.Single, currentPlayer);
                break;

            case TargetType.All:
                Debug.Log("데미지가 모든 플레이어에게 적용됩니다.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.All, currentPlayer);
                break;

            case TargetType.AllExceptSelf:
                Debug.Log("데미지가 자신을 제외한 모든 플레이어에게 적용됩니다.");
                PlayerManager.Inst.ApplyDamageToTarget(item.damage, TargetType.AllExceptSelf, currentPlayer);
                break;

            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void HealPlayer(PlayerData currentPlayer)
    {
        Debug.Log($"HealPlayer카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("체력이 회복됩니다.");
                PlayerManager.Inst.HealPlayer(currentPlayer, TargetType.Self, item.heal);
                break;

            case TargetType.All:
                Debug.Log("모든 플레이어의 체력이 회복됩니다.");
                PlayerManager.Inst.HealPlayer(currentPlayer, TargetType.All, item.heal);
                break;

            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void ActivateDefense(PlayerData currentPlayer)
    {
        Debug.Log($"카드 {item.name} 사용: {item.effect}");
        PlayerManager.Inst.ActivateDefense(currentPlayer);
    }
    private void ApplySpecialEffect(PlayerData currentPlayer)
    {
        Debug.Log($"Card -> ApplySpecialEffect : {item.name}");
        switch (item.name)
        {
            case "무차별 난사":
                ApplyDamage(currentPlayer);
                break;

            case "주점":
                HealPlayer(currentPlayer);
                break;

            case "강탈":
                extortion(currentPlayer);
                break;

            case "저주":
                Curse(currentPlayer);
                break;

            case "떠돌이 상인":
                AWanderingMerchant(currentPlayer);
                break;

            case "요정의 선물":
                GiftFairy(currentPlayer);
                break;

            case "황금 고블린":
                GoldenGoblin(currentPlayer);
                break;

            default:
                Debug.LogWarning("알 수 없는 카드 입니다");
                break;
        }
    }
    private void extortion(PlayerData currentPlayer) //강탈
    {
        Debug.Log($"extortion 카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Single:
                Debug.Log("상대 카드를 훔쳐옵니다.");
                // 강탈 효과 실행
                CardManager.Inst.extortion(currentPlayer, TargetType.Single);
                break;

            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void EquipItem(PlayerData currentPlayer, List<PlayerData> allPlayers)
    {
        PlayerUI targetPlayerUI = null;

        // 모든 PlayerUI 중에서 현재 플레이어와 연결된 UI를 찾음
        PlayerUI[] playerUIs = FindObjectsOfType<PlayerUI>();
        foreach (var playerUI in playerUIs)
        {
            if (playerUI.currentPlayerData == currentPlayer) // PlayerData와 PlayerUI 연결 확인
            {
                targetPlayerUI = playerUI;
                break;
            }
        }

        // 기존 카드 효과 제거
        if (currentPlayer.weaponCard != null)
        { 
            Debug.Log($"기존 장착 카드 제거: {currentPlayer.weaponCard.name}");
            RemoveCardEffect(currentPlayer, currentPlayer.weaponCard);
        }

        // 새로운 카드로 교체
        currentPlayer.weaponCard = item;

        // 새로운 카드 효과 적용
        switch (item.name)
        {
            case "인챈트 - 1":
                currentPlayer.attack += 1; // 공격력 1 증가
                break;

            case "인챈트 - 2":
                currentPlayer.attack += 2; // 공격력 2 증가
                break;

            case "인챈트 - 3":
                currentPlayer.attack += 3; // 공격력 3 증가
                break;

            case "인챈트 - 4":
                currentPlayer.attack += 4; // 공격력 4 증가
                break;

            case "인챈트 - 1+":
                currentPlayer.attackCount -= 1; // 공격력 3 증가
                break;

            case "인챈트 - 2+":
                currentPlayer.attackCount -= 2; // 공격력 4 증가
                break;

            default:
                Debug.LogWarning($"알 수 없는 장착 카드: {item.name}");
                break;
        }

        // UI 업데이트
        if (targetPlayerUI != null)
        {
            Debug.Log($"UI 업데이트: {item.name}");
            targetPlayerUI.UpdateItemImages(item, currentPlayer);
        }
        else
        {
            Debug.LogWarning("targetPlayerUI를 찾을 수 없습니다!");
        }
    }
    private void Curse(PlayerData currentPlayer)
    {
        Debug.Log($"Curse 카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Single:
                Debug.Log("상대 카드를 버리게 합니다.");
                // 강탈 효과 실행
                CardManager.Inst.curse(currentPlayer, TargetType.Single);
                break;

            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void AWanderingMerchant(PlayerData currentPlayer)
    {
        Debug.Log($"A Wandering Merchant 카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("장착 카드 한 장을 얻습니다.");
                CardManager.Inst.AWanderingMerchant(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void GiftFairy(PlayerData currentPlayer)
    {
        Debug.Log($"Gift Fairy 카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("카드 더미에서 카드 두 장을 가져온다.");
                CardManager.Inst.GiftFairy(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void GoldenGoblin(PlayerData currentPlayer)
    {
        Debug.Log($"Golden Goblin 카드 {item.name} 사용: {item.effect}");
        switch (item.targetType)
        {
            case TargetType.Self:
                Debug.Log("카드 더미에서 카드 세 장을 가져온다.");
                CardManager.Inst.GoldenGoblin(currentPlayer, TargetType.Self);
                break;
            default:
                Debug.LogWarning($"알 수 없는 TargetType: {item.targetType}");
                break;
        }
    }
    private void RemoveCardEffect(PlayerData currentPlayer, Item previousCard)
    {
        if (previousCard == null)
        {
            Debug.LogWarning("기존 장착된 카드가 없습니다.");
            return;
        }

        // 카드 제거 디버깅
        Debug.Log($"기존 카드 제거: {previousCard.name}, 공격력 이전 값: {currentPlayer.attack}");

        // 이전 카드의 이름에 따라 공격력 감소
        switch (previousCard.name)
        {
            case "인챈트 - 1":
                currentPlayer.attack -= 1;
                break;
            case "인챈트 - 2":
                currentPlayer.attack -= 2;
                break;
            case "인챈트 - 3":
                currentPlayer.attack -= 3;
                break;
            case "인챈트 - 4":
                currentPlayer.attack -= 4;
                break;
            default:
                Debug.LogWarning($"알 수 없는 장착 카드: {previousCard.name}");
                break;
        }

        // 공격력이 0 미만으로 내려가지 않도록 방어
        if (currentPlayer.attack < 0)
        {
            currentPlayer.attack = 0;
            Debug.LogWarning("공격력이 0 미만으로 감소했습니다. 0으로 초기화합니다.");
        }

        Debug.Log($"기존 장착 카드 제거 후 공격력: {currentPlayer.attack}");
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
