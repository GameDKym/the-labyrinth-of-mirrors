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

    public List<PlayerData> allPlayers; // ��� �÷��̾� ������ ����Ʈ
    public RoleSO roleSO; // ���� ����
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
            Debug.LogError("RoleSO�� �Ҵ���� �ʾҽ��ϴ�.");
        }
        UISetup();
        if (playerSO != null)
        {
            playerSO.InitializePlayers();
        }
        else
        {
            Debug.LogError("PlayerSO�� �Ҵ���� �ʾҽ��ϴ�.");
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
        // Null üũ�� �߰��Ͽ�, �ʿ��� UI ��Ұ� �Ҵ�Ǿ����� Ȯ��
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

    void InputCheatKey() //Ű�� ���� ���� ����
    {
        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();

        if (Input.GetKeyDown(KeyCode.Keypad5)) // ���ϴ� Ű�� ���� ����
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

    // ���� ���� ����

    // �¸� ���� üũ
    public void CheckVictoryConditions()
    {
        foreach (var player in allPlayers)
        {
            // ����� �÷��̾�� �¸� ���ǿ��� ����
            if (player.isDead) continue;

            // ���ҿ� ���� �¸� ���� üũ
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


    // ������� �δ����� ���ð�� ��� óġ�ߴ��� Ȯ��
    private bool IsAllDoppelgangersDefeated(PlayerData player)
    {
        return allPlayers.Where(p => p.roleCard.roleType == Role.Doppelganger).All(p => p.isDead);
    }

    // ���ð�� �������� óġ�ߴ��� Ȯ��
    private bool IsTeamLeaderDead()
    {
        return allPlayers.Any(p => p.roleCard.roleType == Role.TeamLeader && p.isDead);
    }

    // ���汳���� �ڽ��� ������ ��� �÷��̾ �׿����� Ȯ��
    private bool IsAllOtherPlayersDead(PlayerData player)
    {
        return allPlayers.Where(p => p != player).All(p => p.isDead);
    }

    // ���� ����
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
            case "���ð���":
                resultPanel.Show($"{winner.roleCard.roleName}�� \n�¸��߽��ϴ�!");
                break;
            default:
                resultPanel.Show($"{winner.roleCard.roleName}�� \n�¸��߽��ϴ�!");
                break;
        }
    }
}
