using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Player Management")]
    [SerializeField] PlayerSO playerSO;  // 전체 플레이어 리스트를 관리하는 PlayerSO
    [SerializeField] PlayerData currentPlayer;     // 현재 플레이어 데이터

    [Header("Card Management")]
    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
    [SerializeField] List<Card> otherCards;
    [SerializeField] List<Card> otherCards1;
    [SerializeField] List<Card> otherCards2;
    [SerializeField] List<Card> otherCards3;

    [Header("Card Interaction")]
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform otherCardSpawnPoint;
    [SerializeField] Transform otherCardSpawnPoint1;
    [SerializeField] Transform otherCardSpawnPoint2;
    [SerializeField] Transform otherCardSpawnPoint3;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;
    
    List<Item> itemBuffer;
    Card selectCard;  // 현재 선택된 카드
    bool isMyCardDrag;  // 카드 드래그 상태

    bool onMyCardArea;
    
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }
    public int myPutCount; // 플레이어가 사용한 카드 수

    public Item PopItem()
    {
        if (itemBuffer == null || itemBuffer.Count == 0)
        {
            Debug.LogWarning("덱이 비어 있습니다. 새로운 덱을 생성합니다.");
            SetupItemBuffer(); // 덱 리필

            if (itemBuffer == null || itemBuffer.Count == 0)
            {
                Debug.LogError("덱 생성에 실패했습니다. 게임을 계속할 수 없습니다.");
                return null; // 더 이상 카드를 생성할 수 없을 경우 null 반환
            }
        }

        Item item = itemBuffer[itemBuffer.Count - 1];
        itemBuffer.RemoveAt(itemBuffer.Count - 1);
        return item;
    }
    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>(77);
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.quantity; j++)
                itemBuffer.Add(item);
        }

        // 덱 섞기
        ShuffleDeck(itemBuffer);
    }

    void ShuffleDeck(List<Item> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            Item temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
    }

    void Start()
    {
        // 아이템 버퍼 설정
        SetupItemBuffer();
        // 이벤트 구독 등록
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;

        // 디버그 로그 추가
        Debug.Log("이벤트가 성공적으로 등록되었습니다: OnAddCard 및 OnTurnStarted");
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;

        // 디버그 로그 추가
        Debug.Log("이벤트가 성공적으로 해제되었습니다: OnAddCard 및 OnTurnStarted");
    }

    void OnTurnStarted(int currentPlayerIndex)
    {
        // 턴 시작 시 필요한 로직 수행
        PlayerData player = playerSO.players[currentPlayerIndex];
        if (player.isDead)
        {
            return; // 죽은 플레이어에게 카드를 추가하지 않고 턴 종료
        }

        // 내 턴인지 확인하고 필요한 작업을 수행
        if (currentPlayerIndex == 0)
        {
            // 내 턴이라면 카드 추가
            AddCard(true, currentPlayerIndex);
            player.attackCount = 0;
        }
        else
        {
            // 상대방의 턴일 경우 상대 플레이어에게 카드 추가
            AddCard(false, currentPlayerIndex);
            player.attackCount = 0;
        }
    }

    void DistributeCards(bool isMine, int cardCount, int targetPlayer)
    {
        for (int i = 0; i < cardCount; i++)
        {
            AddCard(isMine, targetPlayer);
        }
    }

	void Update()
	{
        if (isMyCardDrag)
            CardDrag();

        DetectCardArea();
        SetECardState();
    }
    void AddCard(bool isMine, int targetPlayer)
    {
        if (isMine)
        {
            var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
            var card = cardObject.GetComponent<Card>();
            card.Setup(PopItem(), true);
            myCards.Add(card);
            SetOriginOrder(myCards);
            CardAlignment(myCards);
        }
        else
        {
            List<Card> targetCardList = GetOtherCardList(targetPlayer);
            Transform spawnPoint = GetOtherCardSpawnPoint(targetPlayer);

            // 유효한 targetPlayer인지 확인
            if (targetCardList != null && spawnPoint != null)
            {
                // 상대 카드 생성
                var cardObject = Instantiate(cardPrefab, spawnPoint.position, Utils.QI);

                // GetComponent<Card>()를 통해 Card 컴포넌트를 가져옴
                var card = cardObject.GetComponent<Card>();

                // 카드 정보 설정
                card.Setup(PopItem(), false);  // 상대 카드의 앞면이 아닌 뒷면을 설정할 경우

                // 상대 카드 위치 조정 (targetPlayer에 따라 위치 변경)
                Vector3 cardPosition = Vector3.zero;

                // targetPlayer에 맞춰 카드 위치 조정
                switch (targetPlayer)
                {
                    case 1:
                        cardPosition = new Vector3(20.38f, -0.04f, 0); // 텍스트 위치 조정
                        break;
                    case 2:
                        cardPosition = new Vector3(11.11f, 6.04f, 0); // 텍스트 위치 조정
                        break;
                    case 3:
                        cardPosition = new Vector3(-8.82f, 6.04f, 0); // 텍스트 위치 조정
                        break;
                    case 4:
                        cardPosition = new Vector3(-19.88f, -1.75f, 0); // 텍스트 위치 조정
                        break;
                }

                // 생성된 카드의 위치를 설정
                cardObject.transform.localPosition = cardPosition;

                // 상대 카드 리스트에 카드 추가
                targetCardList.Add(card);

                // 상대 카드 개수 업데이트
                UpdateOtherPlayerCardDisplay(targetPlayer, targetCardList.Count);
            }
            else
            {
                Debug.LogError($"Invalid targetPlayer {targetPlayer} or spawn point is null");
            }
        }
    }

    List<Card> GetOtherCardList(int targetPlayer)
    {
        switch (targetPlayer)
        {
            case 1: return otherCards;  // OtherPlayer
            case 2: return otherCards1; // OtherPlayer1
            case 3: return otherCards2; // OtherPlayer2
            case 4: return otherCards3; // OtherPlayer3
            default:
                Debug.LogError($"Invalid targetPlayer: {targetPlayer}");
                return null;
        }
    }

    Transform GetOtherCardSpawnPoint(int targetPlayer)
    {
        switch (targetPlayer)
        {
            case 1: return otherCardSpawnPoint;  // OtherPlayer
            case 2: return otherCardSpawnPoint1; // OtherPlayer1
            case 3: return otherCardSpawnPoint2; // OtherPlayer2
            case 4: return otherCardSpawnPoint3; // OtherPlayer3
            default:
                Debug.LogError($"Invalid targetPlayer: {targetPlayer}");
                return null;
        }
    }
    void UpdateOtherPlayerCardDisplay(int targetPlayer, int cardCount)
    {
        // OtherPlayer의 카드 표시 위치
        Transform spawnPoint = GetOtherCardSpawnPoint(targetPlayer);
        // 카드 개수를 표시하는 텍스트 생성 또는 갱신
        var cardCountText = spawnPoint.GetComponentInChildren<TextMesh>();
        if (cardCountText == null)
        {
            var textObject = new GameObject($"OtherPlayer{targetPlayer}_CardCount");
            textObject.transform.SetParent(spawnPoint);
            switch(targetPlayer)
            {
                case 1:
                    textObject.transform.localPosition = new Vector3(-1.82f, 0.16f, 0); // 텍스트 위치 조정
                    break;
                case 2:
                    textObject.transform.localPosition = new Vector3(-3f, -5f, 0); // 텍스트 위치 조정
                    break;
                case 3:
                    textObject.transform.localPosition = new Vector3(+3f, -5f, 0); // 텍스트 위치 조정
                    break;
                case 4:
                    textObject.transform.localPosition = new Vector3(1.92f, -0.5f, 0); // 텍스트 위치 조정
                    break;
            }
            cardCountText = textObject.AddComponent<TextMesh>();
            cardCountText.fontSize = 20;
            cardCountText.color = Color.white;
        }
        cardCountText.text = $"x{cardCount}";
    }
    void SetOriginOrder(List<Card> cardList)
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i]?.GetComponent<Order>().SetOriginOrder(i);
        }
    }
    void CardAlignment(List<Card> cardList)
    {
        if (cardList == myCards) // MyPlayer만 정렬
        {
            List<PRS> originCardPRSs = RoundAlignment(myCardLeft, myCardRight, cardList.Count, -0.5f, Vector3.one * 1.9f);

            for (int i = 0; i < cardList.Count; i++)
            {
                var targetCard = cardList[i];
                targetCard.originPRS = originCardPRSs[i];
                targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
            }
        }
    }
    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                    objLerps[i] = interval * i;
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Utils.QI;
            if (objCount >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
                curve = height >= 0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }
    public void extortion(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        List<Card> targetCardList;
        // 타겟 플레이어를 랜덤으로 선택 (자기 자신 제외)
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager에전달된 PlayerData 정보: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");
        if (targetPlayer != null)
        {
            switch (targetPlayer.id)
            {
                case 1:
                    // 상대 플레이어의 카드 목록 가져오기
                    targetCardList = GetOtherCardList(0);

                    // 상대 카드가 있다면 강탈
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // 랜덤 카드 선택

                        // 카드의 앞면으로 설정 (강탈된 카드가 뒷면으로 설정되지 않도록)
                        stolenCard.Setup(stolenCard.item, true); // true는 카드의 앞면을 의미

                        // 카드를 내 카드 목록에 추가
                        switch (currentPlayer.id)
                        {
                            case 0:
                                myCards.Add(stolenCard);
                                break;
                            case 1:
                                otherCards.Add(stolenCard);
                                break;
                            case 2:
                                otherCards1.Add(stolenCard);
                                break;
                            case 3:
                                otherCards2.Add(stolenCard);
                                break;
                            case 4:
                                otherCards3.Add(stolenCard);
                                break;
                        }

                        // 상대 카드 목록에서 해당 카드 제거
                        targetCardList.RemoveAt(randomIndex);

                        // 카드 정렬 및 업데이트
                        CardAlignment(myCards);  // 내 카드 목록을 정렬
                        SetOriginOrder(myCards); // 정렬 순서 설정
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // 상대 카드 수 업데이트
                        Debug.Log($"{currentPlayer.playerName}가 {targetPlayer.playerName}의 카드를 강탈했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}는 카드를 가지고 있지 않습니다.");
                    }
                    break;
                case 2:
                    // 상대 플레이어의 카드 목록 가져오기
                    targetCardList = GetOtherCardList(1);
                    // 상대 카드가 있다면 강탈
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // 랜덤 카드 선택

                        // 카드의 앞면으로 설정 (강탈된 카드가 뒷면으로 설정되지 않도록)
                        stolenCard.Setup(stolenCard.item, true); // true는 카드의 앞면을 의미

                        // 카드를 내 카드 목록에 추가
                        myCards.Add(stolenCard);

                        // 상대 카드 목록에서 해당 카드 제거
                        targetCardList.RemoveAt(randomIndex);

                        // 카드 정렬 및 업데이트
                        CardAlignment(myCards);  // 내 카드 목록을 정렬
                        SetOriginOrder(myCards); // 정렬 순서 설정
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // 상대 카드 수 업데이트
                        Debug.Log($"{currentPlayer.playerName}가 {targetPlayer.playerName}의 카드를 강탈했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}는 카드를 가지고 있지 않습니다.");
                    }
                    break;
                case 3:
                    // 상대 플레이어의 카드 목록 가져오기
                    targetCardList = GetOtherCardList(2);
                    // 상대 카드가 있다면 강탈
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // 랜덤 카드 선택

                        // 카드의 앞면으로 설정 (강탈된 카드가 뒷면으로 설정되지 않도록)
                        stolenCard.Setup(stolenCard.item, true); // true는 카드의 앞면을 의미

                        // 카드를 내 카드 목록에 추가
                        myCards.Add(stolenCard);

                        // 상대 카드 목록에서 해당 카드 제거
                        targetCardList.RemoveAt(randomIndex);

                        // 카드 정렬 및 업데이트
                        CardAlignment(myCards);  // 내 카드 목록을 정렬
                        SetOriginOrder(myCards); // 정렬 순서 설정
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // 상대 카드 수 업데이트
                        Debug.Log($"{currentPlayer.playerName}가 {targetPlayer.playerName}의 카드를 강탈했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}는 카드를 가지고 있지 않습니다.");
                    }
                    break;
                case 4:
                    // 상대 플레이어의 카드 목록 가져오기
                    targetCardList = GetOtherCardList(3);
                    // 상대 카드가 있다면 강탈
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // 랜덤 카드 선택

                        // 카드의 앞면으로 설정 (강탈된 카드가 뒷면으로 설정되지 않도록)
                        stolenCard.Setup(stolenCard.item, true); // true는 카드의 앞면을 의미

                        // 카드를 내 카드 목록에 추가
                        myCards.Add(stolenCard);

                        // 상대 카드 목록에서 해당 카드 제거
                        targetCardList.RemoveAt(randomIndex);

                        // 카드 정렬 및 업데이트
                        CardAlignment(myCards);  // 내 카드 목록을 정렬
                        SetOriginOrder(myCards); // 정렬 순서 설정
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // 상대 카드 수 업데이트
                        Debug.Log($"{currentPlayer.playerName}가 {targetPlayer.playerName}의 카드를 강탈했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}는 카드를 가지고 있지 않습니다.");
                    }
                    break;
                default:
                    Debug.LogError($"Invalid targetPlayer: {targetPlayer}");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("강탈할 상대를 선택할 수 없습니다.");
        }
    }
    public void curse(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        List<Card> targetCardList;
        // 타겟 플레이어를 랜덤으로 선택 (자기 자신 제외)
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, TargetType.Single, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager에전달된 PlayerData 정보: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");
        if (targetPlayer != null)
        {
            // targetPlayer.id를 이용해 올바른 상대 카드 목록을 가져옵니다.
            targetCardList = GetOtherCardList(targetPlayer.id - 1); // targetPlayer.id에 맞춰 인덱스를 조정

            // 상대 카드가 있다면 강탈
            if (targetCardList != null && targetCardList.Count > 0)
            {
                int randomIndex = Random.Range(0, targetCardList.Count);
                Card stolenCard = targetCardList[randomIndex];  // 랜덤 카드 선택

                // 상대 카드 목록에서 해당 카드 제거
                targetCardList.RemoveAt(randomIndex);

                // 카드 정렬 및 업데이트
                UpdateOtherPlayerCardDisplay(targetPlayer.id, targetCardList.Count); // 올바른 플레이어 ID로 카드 수 업데이트
                Debug.Log($"{currentPlayer.playerName}가 {targetPlayer.playerName}의 카드를 강탈했습니다.");
            }
            else
            {
                Debug.LogWarning($"{targetPlayer.playerName}는 카드를 가지고 있지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("타겟 플레이어가 유효하지 않습니다.");
        }
    }

    public void AWanderingMerchant(PlayerData currentPlayer, TargetType targetType)
    {
        List<PlayerData> allPlayers = playerSO.players;
        // 자신을 선택
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        if (targetPlayer != null)
        {
            List<Item> equipCards = new List<Item>();
            // itemSO.items에서 장착 카드 복사
            foreach (var item in itemSO.items)
            {
                if (item.type == "장착 카드" && item.quantity > 0) // 장착 카드이고 수량이 1 이상인 카드만
                {
                    equipCards.Add(new Item
                    {
                        name = item.name,
                        type = item.type,
                        effect = item.effect,
                        quantity = item.quantity,
                        damage = item.damage,
                        heal = item.heal,
                        targetType = item.targetType,
                        sprite = item.sprite
                    });
                }
            }

            if (equipCards.Count > 0)
            {
                int randomIndex = Random.Range(0, equipCards.Count);
                Item selectedItem = equipCards[randomIndex]; // 장착 카드 선택

                // 선택된 장착 카드로 새로운 Card 객체 생성
                GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
                Card selectedCard = cardObject.GetComponent<Card>();
                selectedCard.Setup(selectedItem, true);
                // 선택된 카드의 앞면을 설정
                // 내 카드 목록에 추가
                switch (currentPlayer.id)
                {
                    case 0:
                        // 카드 앞면 설정
                        myCards.Add(selectedCard);
                        break;
                    case 1:
                        cardObject.SetActive(false);
                        otherCards.Add(selectedCard);
                        break;
                    case 2:
                        cardObject.SetActive(false);
                        otherCards1.Add(selectedCard);
                        break;
                    case 3:
                        cardObject.SetActive(false);
                        otherCards2.Add(selectedCard);
                        break;
                    case 4:
                        cardObject.SetActive(false);
                        otherCards3.Add(selectedCard);
                        break;
                }

                // 아이템 수량 감소
                itemBuffer.RemoveAll(item => item.name == selectedItem.name && item.quantity == selectedItem.quantity);

                CardAlignment(myCards);  // 내 카드 목록을 정렬
                SetOriginOrder(myCards); // 정렬 순서 설정
            }
        }
        else
        {
            Debug.LogWarning("강탈할 상대를 선택할 수 없습니다.");
        }
    }
    public void GiftFairy(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        // 자신을 선택
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager에전달된 PlayerData 정보: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");

        if (targetPlayer != null)
        {
            List<Item> drawnItems = new List<Item>(); // 드로우된 카드들을 저장할 리스트

            // 카드 2장을 뽑기
            for (int i = 0; i < 2; i++)
            {
                if (itemBuffer.Count == 0 || itemBuffer.Count == 1)
                {
                    SetupItemBuffer(); // 덱이 비었을 경우 리필
                }

                if (itemBuffer.Count > 0)
                {
                    // 첫 번째 카드부터 순차적으로 가져오기
                    Item selectedItem = itemBuffer[0]; // 첫 번째 카드를 가져옵니다.

                    // 드로우된 카드 목록에 추가
                    drawnItems.Add(selectedItem);

                    // 선택된 카드를 내 카드 목록에 추가
                    GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
                    Card selectedCard = cardObject.GetComponent<Card>();
                    selectedCard.Setup(selectedItem, true);  // 카드 앞면 설정

                    myCards.Add(selectedCard); // 내 카드 목록에 추가

                    // itemBuffer에서 첫 번째 아이템 제거
                    itemBuffer.RemoveAt(0); // 첫 번째 카드만 제거
                }
                else
                {
                    Debug.LogWarning("덱에 카드가 없습니다.");
                    break;  // 카드가 없으면 반복 종료
                }
            }

            // 카드 정렬 및 위치 설정
            CardAlignment(myCards);  // 내 카드 목록을 정렬
            SetOriginOrder(myCards); // 정렬 순서 설정
        }
        else
        {
            Debug.LogWarning("강탈할 상대를 선택할 수 없습니다.");
        }
    }
    public void GoldenGoblin(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        // 자신을 선택
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager에전달된 PlayerData 정보: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");

        if (targetPlayer != null)
        {
            List<Item> drawnItems = new List<Item>(); // 드로우된 카드들을 저장할 리스트

            // 카드 2장을 뽑기
            for (int i = 0; i < 3; i++)
            {
                if (itemBuffer.Count == 0 || itemBuffer.Count == 1 || itemBuffer.Count == 2)
                {
                    SetupItemBuffer(); // 덱이 비었을 경우 리필
                }

                if (itemBuffer.Count > 0)
                {
                    // 첫 번째 카드부터 순차적으로 가져오기
                    Item selectedItem = itemBuffer[0]; // 첫 번째 카드를 가져옵니다.

                    // 드로우된 카드 목록에 추가
                    drawnItems.Add(selectedItem);

                    // 선택된 카드를 내 카드 목록에 추가
                    GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
                    Card selectedCard = cardObject.GetComponent<Card>();
                    selectedCard.Setup(selectedItem, true);  // 카드 앞면 설정

                    myCards.Add(selectedCard); // 내 카드 목록에 추가

                    // itemBuffer에서 첫 번째 아이템 제거
                    itemBuffer.RemoveAt(0); // 첫 번째 카드만 제거
                }
                else
                {
                    Debug.LogWarning("덱에 카드가 없습니다.");
                    break;  // 카드가 없으면 반복 종료
                }
            }

            // 카드 정렬 및 위치 설정
            CardAlignment(myCards);  // 내 카드 목록을 정렬
            SetOriginOrder(myCards); // 정렬 순서 설정
        }
        else
        {
            Debug.LogWarning("강탈할 상대를 선택할 수 없습니다.");
        }
    }
    public bool TryPutCard(bool isMine, int targetPlayer, PlayerData currentPlayer)
    {
        // 타겟 카드 리스트 가져오기
        List<Card> targetCards = isMine ? myCards : GetOtherCardList(targetPlayer);

        // 타겟 카드 리스트가 null이거나 비어 있을 경우 로그 출력 후 함수 종료
        if (targetCards == null || targetCards.Count == 0)
        {
            Debug.LogError($"No cards available for target player {targetPlayer}. Cannot proceed.");
            return false;
        }

        // 카드 선택 (내 카드일 경우 selectCard, 아닐 경우 랜덤으로 선택)
        Card card = isMine ? selectCard : targetCards[Random.Range(0, targetCards.Count)];

        // 선택된 카드가 null인 경우 로그 출력 후 함수 종료
        if (card == null)
        {
            Debug.LogError("Selected card is null. Cannot proceed.");
            return false;
        }

        List<PlayerData> allPlayers = playerSO.players;

        // 카드가 null이 아닌 경우 카드 사용 로직 수행
        try
        {
            card.UseCard(currentPlayer, allPlayers); // 카드 효과 발동
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error using card: {ex.Message}");
        }
        finally
        {
            // 카드 제거 처리
            if (isMine)
            {
                myCards.Remove(card); // 내 카드에서 제거
            }
            else
            {
                targetCards.Remove(card); // 상대 카드에서 제거
            }

            // 카드 오브젝트 제거
            Destroy(card.gameObject);

            // 내 카드 정렬
            if (isMine)
            {
                CardAlignment(myCards);
            }
        }

        return true;
    }

    #region MyCard
    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Nothing)
            return;

        selectCard = card;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        EnlargeCard(false, card);
    }

    public void CardMouseDown() 
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        isMyCardDrag = true;
    }

    public void CardMouseUp()
    {
        isMyCardDrag = false;

        if (eCardState != ECardState.CanMouseDrag)
            return;
   
        // 카드가 내 카드 영역 안에 있을 경우
        if (onMyCardArea)
        {
            Debug.Log("CardManager-CardMouseUP : 카드를 다시 손패로 되돌립니다.");
            // 손패로 카드 복귀 (아무 작업도 하지 않음)
        }
        else
        {
            // 카드를 사용하려는 시도
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

            bool cardUsed = TryPutCard(true, 0, currentPlayer);
            if (cardUsed && selectCard != null)
                try
                {
                    Debug.Log($"CardManager -> CardMouseUP : Card {selectCard.item.name} effect executed!");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"카드 사용 중 오류 발생: {ex.Message}");
                }
                finally
                {
                    myCards.Remove(selectCard);
                    Destroy(selectCard.gameObject); // 카드 제거
                    selectCard = null;
                }
        }
    }

    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        if (!onMyCardArea) //카드 사용 조건
        {
            selectCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectCard.originPRS.scale), false);
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (TurnManager.Inst.isGameOver)
        {
            return;
        }
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4.8f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 3.5f), false);
        }
        else
            card.MoveTransform(card.originPRS, false);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    void SetECardState()
    {
        if (TurnManager.Inst.isGameOver)
        {
            eCardState = ECardState.Nothing;
        }

        if (TurnManager.Inst.isLoading)
            eCardState = ECardState.Nothing;

        else if (!TurnManager.Inst.myTurn)
        {
            eCardState = ECardState.CanMouseOver;
        }


        else if (TurnManager.Inst.myTurn)
        {
            if (selectCard != null && selectCard.item.type == "공격 카드" && currentPlayer.attackCount >= 1)
            {
                // 내 턴이지만 이미 공격 카드를 사용한 경우, 해당 공격 카드는 마우스 오버만 가능
                eCardState = ECardState.CanMouseOver;
            }
            else
            {
                // 다른 카드이거나 공격 카드를 처음 사용하는 경우, 드래그 가능
                eCardState = ECardState.CanMouseDrag;
            }
        }

    }

    #endregion

}