using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.VisualScripting;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("���� �� ��带 ���մϴ�")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("ī�� ����� �ſ� �������ϴ�")] bool fastMode;
    [SerializeField][Tooltip("���� ī�� ������ ���մϴ�")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; // ���� ������ isLoading�� true�� �ϸ� ī��� ��ƼƼ Ŭ������
    public bool myTurn;

    public PlayerSO playerSO;

    enum ETurnMode { Random, My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.3f); // �ð� ����
    WaitForSeconds delay07 = new WaitForSeconds(1.3f); // �ð� ����

    public static Action<bool, int> OnAddCard; // isMine, targetPlayer
    public static event Action<int> OnTurnStarted; // ���� �÷��̾� �ε����� ����
    private int currentPlayerIndex = 0; // ���� �÷��̾� �ε��� (0: MyPlayer, 1~4: OtherPlayers)
    private int totalPlayers = 5; // �� �÷��̾� ��

    public bool isGameOver = false;

    public PlayerData CurrentPlayer
    {
        get
        {
            if (playerSO != null && playerSO.players != null && playerSO.players.Count > 0)
            {
                return playerSO.players[currentPlayerIndex];
            }
            else
            {
                Debug.LogError("PlayerSO �Ǵ� players ����Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
                return null; // null ��ȯ
            }
        }
    }

    private int GetNextPlayerIndex()
    {
        if (playerSO != null && playerSO.players != null && playerSO.players.Count > 0)
        {
            // ���� ���� ������ ����
            int loopCounter = 0;

            // ���� �¸� ���� üũ
            GameManager.Inst.CheckVictoryConditions();

            // ���� �÷��̾ ã�� ��, ����ִ� �÷��̾ ã���� ��
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % playerSO.players.Count;
                loopCounter++;

                // ���� ��� �÷��̾ �׾��ٸ� ���� ���� ������ ���� Ż��
                if (loopCounter > playerSO.players.Count)
                {
                    Debug.LogWarning("��� �÷��̾ �׾����ϴ�. ������ ����Ǿ�� �մϴ�.");
                    GameManager.Inst.CheckVictoryConditions();
                    break;
                }
            }
            while (playerSO.players[currentPlayerIndex].isDead); // ���� �÷��̾�� �ǳʶٱ�

            return currentPlayerIndex;
        }
        else
        {
            Debug.LogError("PlayerSO �Ǵ� players ����Ʈ�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return -1; // �߸��� �ε��� ��ȯ
        }
    }


    void GameSetup()
    {
        if (fastMode)
            delay05 = new WaitForSeconds(0.05f);

        switch (eTurnMode)
        {
            case ETurnMode.Random:
                currentPlayerIndex = Random.Range(0, totalPlayers);
                myTurn = (currentPlayerIndex == 0);
                break;
            case ETurnMode.My:
                currentPlayerIndex = 0; // �׻� �÷��̾� 0�� ���� �� ����
                myTurn = true;
                break;
            case ETurnMode.Other:
                currentPlayerIndex = Random.Range(1, totalPlayers); // AI �÷��̾���� ����
                myTurn = false;
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        // �÷��̾� ������� 5�徿 �й�
        int cardsPerPlayer = 5;

        for (int cardCount = 0; cardCount < cardsPerPlayer; cardCount++)
        {
            for (int playerIndex = 0; playerIndex < totalPlayers; playerIndex++)
            {
                yield return delay05;

                // MyPlayer�� playerIndex 0, OtherPlayers�� 1~4�� ó��
                bool isMine = (playerIndex == 0);
                OnAddCard?.Invoke(isMine, playerIndex);
            }
        }

        StartCoroutine(StartTurnCo());
    }

    IEnumerator StartTurnCo()
    {
        myTurn = (currentPlayerIndex == 0); // ���� ���� ���� ������ ����
        isLoading = true;
        if (myTurn)
        {
            GameManager.Inst.Notification("���� ��");
        }
        else GameManager.Inst.Notification($"�÷��̾�\n{CurrentPlayer.playerName}��\n���Դϴ�");
        yield return delay07;

        if (currentPlayerIndex >= 0 && currentPlayerIndex < totalPlayers) // ��ȿ�� �˻�
        {
            OnTurnStarted?.Invoke(currentPlayerIndex); // ���� �÷��̾� �ε����� ����
        }

        if (!myTurn)
        {
            // AI�� ī�带 ����ϵ��� ����
            bool cardUsed = false;

            // CurrentPlayer�� null���� üũ
            if (CurrentPlayer != null)
            {
                cardUsed = CardManager.Inst.TryPutCard(false, currentPlayerIndex, CurrentPlayer);
            }
            else
            {
                Debug.LogError("CurrentPlayer is null. AI cannot take action.");
            }

            if (cardUsed)
            {
                TurnManager.Inst.EndTurn();
            }
        }

        yield return delay07;
        isLoading = false;
    }

    public void EndTurn()
    {
        if (isGameOver) return;
        currentPlayerIndex = GetNextPlayerIndex();
        GameManager.Inst.CheckVictoryConditions();
        StartCoroutine(StartTurnCo());
    }
}
