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
    [SerializeField] PlayerSO playerSO;  // ��ü �÷��̾� ����Ʈ�� �����ϴ� PlayerSO
    [SerializeField] PlayerData currentPlayer;     // ���� �÷��̾� ������

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
    Card selectCard;  // ���� ���õ� ī��
    bool isMyCardDrag;  // ī�� �巡�� ����

    bool onMyCardArea;
    
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }
    public int myPutCount; // �÷��̾ ����� ī�� ��

    public Item PopItem()
    {
        if (itemBuffer == null || itemBuffer.Count == 0)
        {
            Debug.LogWarning("���� ��� �ֽ��ϴ�. ���ο� ���� �����մϴ�.");
            SetupItemBuffer(); // �� ����

            if (itemBuffer == null || itemBuffer.Count == 0)
            {
                Debug.LogError("�� ������ �����߽��ϴ�. ������ ����� �� �����ϴ�.");
                return null; // �� �̻� ī�带 ������ �� ���� ��� null ��ȯ
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

        // �� ����
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
        // ������ ���� ����
        SetupItemBuffer();
        // �̺�Ʈ ���� ���
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;

        // ����� �α� �߰�
        Debug.Log("�̺�Ʈ�� ���������� ��ϵǾ����ϴ�: OnAddCard �� OnTurnStarted");
    }

    void OnDestroy()
    {
        // �̺�Ʈ ���� ���� (�޸� ���� ����)
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;

        // ����� �α� �߰�
        Debug.Log("�̺�Ʈ�� ���������� �����Ǿ����ϴ�: OnAddCard �� OnTurnStarted");
    }

    void OnTurnStarted(int currentPlayerIndex)
    {
        // �� ���� �� �ʿ��� ���� ����
        PlayerData player = playerSO.players[currentPlayerIndex];
        if (player.isDead)
        {
            return; // ���� �÷��̾�� ī�带 �߰����� �ʰ� �� ����
        }

        // �� ������ Ȯ���ϰ� �ʿ��� �۾��� ����
        if (currentPlayerIndex == 0)
        {
            // �� ���̶�� ī�� �߰�
            AddCard(true, currentPlayerIndex);
            player.attackCount = 0;
        }
        else
        {
            // ������ ���� ��� ��� �÷��̾�� ī�� �߰�
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

            // ��ȿ�� targetPlayer���� Ȯ��
            if (targetCardList != null && spawnPoint != null)
            {
                // ��� ī�� ����
                var cardObject = Instantiate(cardPrefab, spawnPoint.position, Utils.QI);

                // GetComponent<Card>()�� ���� Card ������Ʈ�� ������
                var card = cardObject.GetComponent<Card>();

                // ī�� ���� ����
                card.Setup(PopItem(), false);  // ��� ī���� �ո��� �ƴ� �޸��� ������ ���

                // ��� ī�� ��ġ ���� (targetPlayer�� ���� ��ġ ����)
                Vector3 cardPosition = Vector3.zero;

                // targetPlayer�� ���� ī�� ��ġ ����
                switch (targetPlayer)
                {
                    case 1:
                        cardPosition = new Vector3(20.38f, -0.04f, 0); // �ؽ�Ʈ ��ġ ����
                        break;
                    case 2:
                        cardPosition = new Vector3(11.11f, 6.04f, 0); // �ؽ�Ʈ ��ġ ����
                        break;
                    case 3:
                        cardPosition = new Vector3(-8.82f, 6.04f, 0); // �ؽ�Ʈ ��ġ ����
                        break;
                    case 4:
                        cardPosition = new Vector3(-19.88f, -1.75f, 0); // �ؽ�Ʈ ��ġ ����
                        break;
                }

                // ������ ī���� ��ġ�� ����
                cardObject.transform.localPosition = cardPosition;

                // ��� ī�� ����Ʈ�� ī�� �߰�
                targetCardList.Add(card);

                // ��� ī�� ���� ������Ʈ
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
        // OtherPlayer�� ī�� ǥ�� ��ġ
        Transform spawnPoint = GetOtherCardSpawnPoint(targetPlayer);
        // ī�� ������ ǥ���ϴ� �ؽ�Ʈ ���� �Ǵ� ����
        var cardCountText = spawnPoint.GetComponentInChildren<TextMesh>();
        if (cardCountText == null)
        {
            var textObject = new GameObject($"OtherPlayer{targetPlayer}_CardCount");
            textObject.transform.SetParent(spawnPoint);
            switch(targetPlayer)
            {
                case 1:
                    textObject.transform.localPosition = new Vector3(-1.82f, 0.16f, 0); // �ؽ�Ʈ ��ġ ����
                    break;
                case 2:
                    textObject.transform.localPosition = new Vector3(-3f, -5f, 0); // �ؽ�Ʈ ��ġ ����
                    break;
                case 3:
                    textObject.transform.localPosition = new Vector3(+3f, -5f, 0); // �ؽ�Ʈ ��ġ ����
                    break;
                case 4:
                    textObject.transform.localPosition = new Vector3(1.92f, -0.5f, 0); // �ؽ�Ʈ ��ġ ����
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
        if (cardList == myCards) // MyPlayer�� ����
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
        // Ÿ�� �÷��̾ �������� ���� (�ڱ� �ڽ� ����)
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager�����޵� PlayerData ����: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");
        if (targetPlayer != null)
        {
            switch (targetPlayer.id)
            {
                case 1:
                    // ��� �÷��̾��� ī�� ��� ��������
                    targetCardList = GetOtherCardList(0);

                    // ��� ī�尡 �ִٸ� ��Ż
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // ���� ī�� ����

                        // ī���� �ո����� ���� (��Ż�� ī�尡 �޸����� �������� �ʵ���)
                        stolenCard.Setup(stolenCard.item, true); // true�� ī���� �ո��� �ǹ�

                        // ī�带 �� ī�� ��Ͽ� �߰�
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

                        // ��� ī�� ��Ͽ��� �ش� ī�� ����
                        targetCardList.RemoveAt(randomIndex);

                        // ī�� ���� �� ������Ʈ
                        CardAlignment(myCards);  // �� ī�� ����� ����
                        SetOriginOrder(myCards); // ���� ���� ����
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // ��� ī�� �� ������Ʈ
                        Debug.Log($"{currentPlayer.playerName}�� {targetPlayer.playerName}�� ī�带 ��Ż�߽��ϴ�.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}�� ī�带 ������ ���� �ʽ��ϴ�.");
                    }
                    break;
                case 2:
                    // ��� �÷��̾��� ī�� ��� ��������
                    targetCardList = GetOtherCardList(1);
                    // ��� ī�尡 �ִٸ� ��Ż
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // ���� ī�� ����

                        // ī���� �ո����� ���� (��Ż�� ī�尡 �޸����� �������� �ʵ���)
                        stolenCard.Setup(stolenCard.item, true); // true�� ī���� �ո��� �ǹ�

                        // ī�带 �� ī�� ��Ͽ� �߰�
                        myCards.Add(stolenCard);

                        // ��� ī�� ��Ͽ��� �ش� ī�� ����
                        targetCardList.RemoveAt(randomIndex);

                        // ī�� ���� �� ������Ʈ
                        CardAlignment(myCards);  // �� ī�� ����� ����
                        SetOriginOrder(myCards); // ���� ���� ����
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // ��� ī�� �� ������Ʈ
                        Debug.Log($"{currentPlayer.playerName}�� {targetPlayer.playerName}�� ī�带 ��Ż�߽��ϴ�.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}�� ī�带 ������ ���� �ʽ��ϴ�.");
                    }
                    break;
                case 3:
                    // ��� �÷��̾��� ī�� ��� ��������
                    targetCardList = GetOtherCardList(2);
                    // ��� ī�尡 �ִٸ� ��Ż
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // ���� ī�� ����

                        // ī���� �ո����� ���� (��Ż�� ī�尡 �޸����� �������� �ʵ���)
                        stolenCard.Setup(stolenCard.item, true); // true�� ī���� �ո��� �ǹ�

                        // ī�带 �� ī�� ��Ͽ� �߰�
                        myCards.Add(stolenCard);

                        // ��� ī�� ��Ͽ��� �ش� ī�� ����
                        targetCardList.RemoveAt(randomIndex);

                        // ī�� ���� �� ������Ʈ
                        CardAlignment(myCards);  // �� ī�� ����� ����
                        SetOriginOrder(myCards); // ���� ���� ����
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // ��� ī�� �� ������Ʈ
                        Debug.Log($"{currentPlayer.playerName}�� {targetPlayer.playerName}�� ī�带 ��Ż�߽��ϴ�.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}�� ī�带 ������ ���� �ʽ��ϴ�.");
                    }
                    break;
                case 4:
                    // ��� �÷��̾��� ī�� ��� ��������
                    targetCardList = GetOtherCardList(3);
                    // ��� ī�尡 �ִٸ� ��Ż
                    if (targetCardList != null && targetCardList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, targetCardList.Count);
                        Card stolenCard = targetCardList[randomIndex];  // ���� ī�� ����

                        // ī���� �ո����� ���� (��Ż�� ī�尡 �޸����� �������� �ʵ���)
                        stolenCard.Setup(stolenCard.item, true); // true�� ī���� �ո��� �ǹ�

                        // ī�带 �� ī�� ��Ͽ� �߰�
                        myCards.Add(stolenCard);

                        // ��� ī�� ��Ͽ��� �ش� ī�� ����
                        targetCardList.RemoveAt(randomIndex);

                        // ī�� ���� �� ������Ʈ
                        CardAlignment(myCards);  // �� ī�� ����� ����
                        SetOriginOrder(myCards); // ���� ���� ����
                        UpdateOtherPlayerCardDisplay(0, targetCardList.Count); // ��� ī�� �� ������Ʈ
                        Debug.Log($"{currentPlayer.playerName}�� {targetPlayer.playerName}�� ī�带 ��Ż�߽��ϴ�.");
                    }
                    else
                    {
                        Debug.LogWarning($"{targetPlayer.playerName}�� ī�带 ������ ���� �ʽ��ϴ�.");
                    }
                    break;
                default:
                    Debug.LogError($"Invalid targetPlayer: {targetPlayer}");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("��Ż�� ��븦 ������ �� �����ϴ�.");
        }
    }
    public void curse(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        List<Card> targetCardList;
        // Ÿ�� �÷��̾ �������� ���� (�ڱ� �ڽ� ����)
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, TargetType.Single, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager�����޵� PlayerData ����: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");
        if (targetPlayer != null)
        {
            // targetPlayer.id�� �̿��� �ùٸ� ��� ī�� ����� �����ɴϴ�.
            targetCardList = GetOtherCardList(targetPlayer.id - 1); // targetPlayer.id�� ���� �ε����� ����

            // ��� ī�尡 �ִٸ� ��Ż
            if (targetCardList != null && targetCardList.Count > 0)
            {
                int randomIndex = Random.Range(0, targetCardList.Count);
                Card stolenCard = targetCardList[randomIndex];  // ���� ī�� ����

                // ��� ī�� ��Ͽ��� �ش� ī�� ����
                targetCardList.RemoveAt(randomIndex);

                // ī�� ���� �� ������Ʈ
                UpdateOtherPlayerCardDisplay(targetPlayer.id, targetCardList.Count); // �ùٸ� �÷��̾� ID�� ī�� �� ������Ʈ
                Debug.Log($"{currentPlayer.playerName}�� {targetPlayer.playerName}�� ī�带 ��Ż�߽��ϴ�.");
            }
            else
            {
                Debug.LogWarning($"{targetPlayer.playerName}�� ī�带 ������ ���� �ʽ��ϴ�.");
            }
        }
        else
        {
            Debug.LogError("Ÿ�� �÷��̾ ��ȿ���� �ʽ��ϴ�.");
        }
    }

    public void AWanderingMerchant(PlayerData currentPlayer, TargetType targetType)
    {
        List<PlayerData> allPlayers = playerSO.players;
        // �ڽ��� ����
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        if (targetPlayer != null)
        {
            List<Item> equipCards = new List<Item>();
            // itemSO.items���� ���� ī�� ����
            foreach (var item in itemSO.items)
            {
                if (item.type == "���� ī��" && item.quantity > 0) // ���� ī���̰� ������ 1 �̻��� ī�常
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
                Item selectedItem = equipCards[randomIndex]; // ���� ī�� ����

                // ���õ� ���� ī��� ���ο� Card ��ü ����
                GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Quaternion.identity);
                Card selectedCard = cardObject.GetComponent<Card>();
                selectedCard.Setup(selectedItem, true);
                // ���õ� ī���� �ո��� ����
                // �� ī�� ��Ͽ� �߰�
                switch (currentPlayer.id)
                {
                    case 0:
                        // ī�� �ո� ����
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

                // ������ ���� ����
                itemBuffer.RemoveAll(item => item.name == selectedItem.name && item.quantity == selectedItem.quantity);

                CardAlignment(myCards);  // �� ī�� ����� ����
                SetOriginOrder(myCards); // ���� ���� ����
            }
        }
        else
        {
            Debug.LogWarning("��Ż�� ��븦 ������ �� �����ϴ�.");
        }
    }
    public void GiftFairy(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        // �ڽ��� ����
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager�����޵� PlayerData ����: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");

        if (targetPlayer != null)
        {
            List<Item> drawnItems = new List<Item>(); // ��ο�� ī����� ������ ����Ʈ

            // ī�� 2���� �̱�
            for (int i = 0; i < 2; i++)
            {
                if (itemBuffer.Count == 0 || itemBuffer.Count == 1)
                {
                    SetupItemBuffer(); // ���� ����� ��� ����
                }

                if (itemBuffer.Count > 0)
                {
                    // ù ��° ī����� ���������� ��������
                    Item selectedItem = itemBuffer[0]; // ù ��° ī�带 �����ɴϴ�.

                    // ��ο�� ī�� ��Ͽ� �߰�
                    drawnItems.Add(selectedItem);

                    // ���õ� ī�带 �� ī�� ��Ͽ� �߰�
                    GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
                    Card selectedCard = cardObject.GetComponent<Card>();
                    selectedCard.Setup(selectedItem, true);  // ī�� �ո� ����

                    myCards.Add(selectedCard); // �� ī�� ��Ͽ� �߰�

                    // itemBuffer���� ù ��° ������ ����
                    itemBuffer.RemoveAt(0); // ù ��° ī�常 ����
                }
                else
                {
                    Debug.LogWarning("���� ī�尡 �����ϴ�.");
                    break;  // ī�尡 ������ �ݺ� ����
                }
            }

            // ī�� ���� �� ��ġ ����
            CardAlignment(myCards);  // �� ī�� ����� ����
            SetOriginOrder(myCards); // ���� ���� ����
        }
        else
        {
            Debug.LogWarning("��Ż�� ��븦 ������ �� �����ϴ�.");
        }
    }
    public void GoldenGoblin(PlayerData currentPlayer, TargetType targetType)
    {
        Debug.Log($"{currentPlayer.id}");
        List<PlayerData> allPlayers = playerSO.players;
        // �ڽ��� ����
        PlayerData targetPlayer = TargetingManager.Inst.SelectTarget(allPlayers, targetType, currentPlayer);
        Debug.Log($"[ApplyHealthChange] PlayerManager�����޵� PlayerData ����: " +
                $"ID: {targetPlayer.id}, Name: {targetPlayer.playerName}, " +
                $"Health: {targetPlayer.health}, BaseHealth: {targetPlayer.characterData.baseHealth}");

        if (targetPlayer != null)
        {
            List<Item> drawnItems = new List<Item>(); // ��ο�� ī����� ������ ����Ʈ

            // ī�� 2���� �̱�
            for (int i = 0; i < 3; i++)
            {
                if (itemBuffer.Count == 0 || itemBuffer.Count == 1 || itemBuffer.Count == 2)
                {
                    SetupItemBuffer(); // ���� ����� ��� ����
                }

                if (itemBuffer.Count > 0)
                {
                    // ù ��° ī����� ���������� ��������
                    Item selectedItem = itemBuffer[0]; // ù ��° ī�带 �����ɴϴ�.

                    // ��ο�� ī�� ��Ͽ� �߰�
                    drawnItems.Add(selectedItem);

                    // ���õ� ī�带 �� ī�� ��Ͽ� �߰�
                    GameObject cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
                    Card selectedCard = cardObject.GetComponent<Card>();
                    selectedCard.Setup(selectedItem, true);  // ī�� �ո� ����

                    myCards.Add(selectedCard); // �� ī�� ��Ͽ� �߰�

                    // itemBuffer���� ù ��° ������ ����
                    itemBuffer.RemoveAt(0); // ù ��° ī�常 ����
                }
                else
                {
                    Debug.LogWarning("���� ī�尡 �����ϴ�.");
                    break;  // ī�尡 ������ �ݺ� ����
                }
            }

            // ī�� ���� �� ��ġ ����
            CardAlignment(myCards);  // �� ī�� ����� ����
            SetOriginOrder(myCards); // ���� ���� ����
        }
        else
        {
            Debug.LogWarning("��Ż�� ��븦 ������ �� �����ϴ�.");
        }
    }
    public bool TryPutCard(bool isMine, int targetPlayer, PlayerData currentPlayer)
    {
        // Ÿ�� ī�� ����Ʈ ��������
        List<Card> targetCards = isMine ? myCards : GetOtherCardList(targetPlayer);

        // Ÿ�� ī�� ����Ʈ�� null�̰ų� ��� ���� ��� �α� ��� �� �Լ� ����
        if (targetCards == null || targetCards.Count == 0)
        {
            Debug.LogError($"No cards available for target player {targetPlayer}. Cannot proceed.");
            return false;
        }

        // ī�� ���� (�� ī���� ��� selectCard, �ƴ� ��� �������� ����)
        Card card = isMine ? selectCard : targetCards[Random.Range(0, targetCards.Count)];

        // ���õ� ī�尡 null�� ��� �α� ��� �� �Լ� ����
        if (card == null)
        {
            Debug.LogError("Selected card is null. Cannot proceed.");
            return false;
        }

        List<PlayerData> allPlayers = playerSO.players;

        // ī�尡 null�� �ƴ� ��� ī�� ��� ���� ����
        try
        {
            card.UseCard(currentPlayer, allPlayers); // ī�� ȿ�� �ߵ�
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error using card: {ex.Message}");
        }
        finally
        {
            // ī�� ���� ó��
            if (isMine)
            {
                myCards.Remove(card); // �� ī�忡�� ����
            }
            else
            {
                targetCards.Remove(card); // ��� ī�忡�� ����
            }

            // ī�� ������Ʈ ����
            Destroy(card.gameObject);

            // �� ī�� ����
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
   
        // ī�尡 �� ī�� ���� �ȿ� ���� ���
        if (onMyCardArea)
        {
            Debug.Log("CardManager-CardMouseUP : ī�带 �ٽ� ���з� �ǵ����ϴ�.");
            // ���з� ī�� ���� (�ƹ� �۾��� ���� ����)
        }
        else
        {
            // ī�带 ����Ϸ��� �õ�
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

            bool cardUsed = TryPutCard(true, 0, currentPlayer);
            if (cardUsed && selectCard != null)
                try
                {
                    Debug.Log($"CardManager -> CardMouseUP : Card {selectCard.item.name} effect executed!");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"ī�� ��� �� ���� �߻�: {ex.Message}");
                }
                finally
                {
                    myCards.Remove(selectCard);
                    Destroy(selectCard.gameObject); // ī�� ����
                    selectCard = null;
                }
        }
    }

    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        if (!onMyCardArea) //ī�� ��� ����
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
            if (selectCard != null && selectCard.item.type == "���� ī��" && currentPlayer.attackCount >= 1)
            {
                // �� �������� �̹� ���� ī�带 ����� ���, �ش� ���� ī��� ���콺 ������ ����
                eCardState = ECardState.CanMouseOver;
            }
            else
            {
                // �ٸ� ī���̰ų� ���� ī�带 ó�� ����ϴ� ���, �巡�� ����
                eCardState = ECardState.CanMouseDrag;
            }
        }

    }

    #endregion

}