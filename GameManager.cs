using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Multiline(10)]
    [SerializeField] string cheatInfo;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] TitlePanel titlePanel;
    [SerializeField] CameraEffect cameraEffect;
    [SerializeField] GameObject endTurnBtn;

    WaitForSeconds delay2 = new WaitForSeconds(2);

    public List<PlayerData> allPlayers; // 모든 플레이어 데이터 리스트
    public RoleSO roleSO; // 역할 정보
    public PlayerSO playerSO;
    public ItemSO Items;

    void Start()
    {
        if (roleSO != null)
        {
            roleSO.InitializeRoles();
        }
        else
        {
            Debug.LogError("RoleSO가 할당되지 않았습니다.");
        }
        UISetup();
        if (playerSO != null)
        {
            playerSO.InitializePlayers();
        }
        else
        {
            Debug.LogError("PlayerSO가 할당되지 않았습니다.");
        }

        allPlayers = GetAllPlayers();

        TurnManager.Inst.isGameOver = false;

    }

    public List<PlayerData> GetAllPlayers()
    {
        return playerSO.players;
    }

    void UISetup()
    {
        // Null 체크를 추가하여, 필요한 UI 요소가 할당되었는지 확인
        if (notificationPanel != null)
            notificationPanel.ScaleZero();
        else
            Debug.LogError("notificationPanel is not assigned!");

        if (resultPanel != null)
            resultPanel.ScaleZero();
        else
            Debug.LogError("resultPanel is not assigned!");

        if (titlePanel != null)
            titlePanel.Active(true);
        else
            Debug.LogError("titlePanel is not assigned!");

        if (cameraEffect != null)
            cameraEffect.SetGrayScale(false);
        else
            Debug.LogError("cameraEffect is not assigned!");
    }

    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }

    void InputCheatKey() //키로 게임 진행 가능
    {
        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();

        if (Input.GetKeyDown(KeyCode.Keypad5)) // 원하는 키로 변경 가능
        {
            TurnManager.Inst.EndTurn();
        }
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void Notification(string message)
    {
        notificationPanel.Show(message);
    }

    // 게임 종료 로직

    // 승리 조건 체크
    public void CheckVictoryConditions()
    {
        foreach (var player in allPlayers)
        {
            // 사망한 플레이어는 승리 조건에서 제외
            if (player.isDead) continue;

            // 역할에 따라 승리 조건 체크
            switch (player.roleCard.roleType)
            {
                case Role.TeamLeader:
                    if (IsAllDoppelgangersDefeated(player))
                    {
                        StartCoroutine(EndGame(player));
                    }
                    break;

                case Role.Lieutenant:
                    if (IsAllDoppelgangersDefeated(player))
                    {
                        StartCoroutine(EndGame(player));
                    }
                    break;

                case Role.Doppelganger:
                    if (IsTeamLeaderDead())
                    {
                        StartCoroutine(EndGame(player));
                    }
                    break;

                case Role.DarkSorcerer:
                    if (IsAllOtherPlayersDead(player))
                    {
                        StartCoroutine(EndGame(player));
                    }
                    break;
            }
        }
    }


    // 공대장과 부대장이 도플갱어를 모두 처치했는지 확인
    private bool IsAllDoppelgangersDefeated(PlayerData player)
    {
        return allPlayers.Where(p => p.roleCard.roleType == Role.Doppelganger).All(p => p.isDead);
    }

    // 도플갱어가 공대장을 처치했는지 확인
    private bool IsTeamLeaderDead()
    {
        return allPlayers.Any(p => p.roleCard.roleType == Role.TeamLeader && p.isDead);
    }

    // 암흑교단이 자신을 제외한 모든 플레이어를 죽였는지 확인
    private bool IsAllOtherPlayersDead(PlayerData player)
    {
        return allPlayers.Where(p => p != player).All(p => p.isDead);
    }

    // 게임 종료
    public IEnumerator EndGame(PlayerData winner)
    {
        TurnManager.Inst.isGameOver = true;
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        
        yield return delay2;

        TurnManager.Inst.isLoading = true;
        cameraEffect.SetGrayScale(true);

        switch (winner.roleCard.roleName)
        {
            case "도플갱어":
                resultPanel.Show($"{winner.roleCard.roleName}가 \n승리했습니다!");
                break;
            default:
                resultPanel.Show($"{winner.roleCard.roleName}이 \n승리했습니다!");
                break;
        }
    }
}
