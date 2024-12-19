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
    [SerializeField][Tooltip("시작 턴 모드를 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; // 게임 끝나면 isLoading을 true로 하면 카드와 엔티티 클릭방지
    public bool myTurn;

    public PlayerSO playerSO;

    enum ETurnMode { Random, My, Other }
    WaitForSeconds delay05 = new WaitForSeconds(0.3f); // 시간 조절
    WaitForSeconds delay07 = new WaitForSeconds(1.3f); // 시간 조절

    public static Action<bool, int> OnAddCard; // isMine, targetPlayer
    public static event Action<int> OnTurnStarted; // 현재 플레이어 인덱스를 전달
    private int currentPlayerIndex = 0; // 현재 플레이어 인덱스 (0: MyPlayer, 1~4: OtherPlayers)
    private int totalPlayers = 5; // 총 플레이어 수

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
                Debug.LogError("PlayerSO 또는 players 리스트가 초기화되지 않았습니다.");
                return null; // null 반환
            }
        }
    }

    private int GetNextPlayerIndex()
    {
        if (playerSO != null && playerSO.players != null && playerSO.players.Count > 0)
        {
            // 무한 루프 방지용 변수
            int loopCounter = 0;

            // 게임 승리 조건 체크
            GameManager.Inst.CheckVictoryConditions();

            // 다음 플레이어를 찾을 때, 살아있는 플레이어만 찾도록 함
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % playerSO.players.Count;
                loopCounter++;

                // 만약 모든 플레이어가 죽었다면 무한 루프 방지를 위해 탈출
                if (loopCounter > playerSO.players.Count)
                {
                    Debug.LogWarning("모든 플레이어가 죽었습니다. 게임이 종료되어야 합니다.");
                    GameManager.Inst.CheckVictoryConditions();
                    break;
                }
            }
            while (playerSO.players[currentPlayerIndex].isDead); // 죽은 플레이어는 건너뛰기

            return currentPlayerIndex;
        }
        else
        {
            Debug.LogError("PlayerSO 또는 players 리스트가 초기화되지 않았습니다.");
            return -1; // 잘못된 인덱스 반환
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
                currentPlayerIndex = 0; // 항상 플레이어 0이 나의 턴 시작
                myTurn = true;
                break;
            case ETurnMode.Other:
                currentPlayerIndex = Random.Range(1, totalPlayers); // AI 플레이어부터 시작
                myTurn = false;
                break;
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;

        // 플레이어 순서대로 5장씩 분배
        int cardsPerPlayer = 5;

        for (int cardCount = 0; cardCount < cardsPerPlayer; cardCount++)
        {
            for (int playerIndex = 0; playerIndex < totalPlayers; playerIndex++)
            {
                yield return delay05;

                // MyPlayer는 playerIndex 0, OtherPlayers는 1~4로 처리
                bool isMine = (playerIndex == 0);
                OnAddCard?.Invoke(isMine, playerIndex);
            }
        }

        StartCoroutine(StartTurnCo());
    }

    IEnumerator StartTurnCo()
    {
        myTurn = (currentPlayerIndex == 0); // 현재 턴이 나의 턴인지 설정
        isLoading = true;
        if (myTurn)
        {
            GameManager.Inst.Notification("나의 턴");
        }
        else GameManager.Inst.Notification($"플레이어\n{CurrentPlayer.playerName}의\n턴입니다");
        yield return delay07;

        if (currentPlayerIndex >= 0 && currentPlayerIndex < totalPlayers) // 유효성 검사
        {
            OnTurnStarted?.Invoke(currentPlayerIndex); // 현재 플레이어 인덱스를 전달
        }

        if (!myTurn)
        {
            // AI가 카드를 사용하도록 유도
            bool cardUsed = false;

            // CurrentPlayer가 null인지 체크
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
