using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// TargetType ������ ����
public enum TargetType
{
    Self,           // �� �ڽ�
    Single,         // Ư�� ��� 1�� (�ڽ� ����)
    All,            // ��ü ��� (�ڽ� ����)
    AllExceptSelf   // ��ü ��� (�ڽ� ����)
}

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Inst { get; private set; } // �̱��� �ν��Ͻ�
    private PlayerData selectedTarget;
    int remain = 0;
    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        Debug.Log("TargetingManager�� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    //Targeting ���� �ż���
    public void StartTargetSelection(List<PlayerData> players, System.Action<PlayerData> onTargetSelected)
    {
        TargetSelectionUI.Instance.Show(players, (target) =>
        {
            selectedTarget = target;
            onTargetSelected?.Invoke(selectedTarget);
        });
    }


    // Ư�� ���(Ÿ��)�� �����ϴ� �޼���
    public PlayerData SelectTarget(List<PlayerData> players, TargetType targetType, PlayerData currentPlayer)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("TargetType -> SelectTarget �÷��̾� ����� ����ְų� null�Դϴ�.");
            return null;
        }

        switch (targetType)
        {
            case TargetType.Self:
                if (players != null && players.Count > 0)
                {
                    // �� �ڽŸ��� ����
                    PlayerData target = players.Find(player => player.id == currentPlayer.id && !player.isDead);
                    if (target != null)
                    {
                        Debug.Log($"�ڽ��� Ÿ������ ����: {target.playerName}");
                        return target;
                    }
                }
                break;

            case TargetType.Single:
                if (players != null && players.Count > 0)
                {
                    // �� �ڽ��� ������ Ÿ�� ��� ����
                    List<PlayerData> potentialTargets = players.FindAll(player => player.id != currentPlayer.id && !player.isDead);

                    Debug.Log($"Ÿ�� �ĺ� ��: {potentialTargets.Count}");
                    if (potentialTargets.Count > 0)
                    {
                        // ���� ���: �������� ����
                        PlayerData target = potentialTargets[Random.Range(0, potentialTargets.Count)];
                        Debug.Log($"���õ� Ÿ��: {target.playerName}");
                        return target;
                    }
                    else
                    {
                        Debug.LogWarning("Ÿ�� �ĺ��� �����ϴ�!");
                    }
                }
                break;

            case TargetType.All:
                Debug.Log("��ü ��� (�ڽ� ����)");
                if (players != null && players.Count > 0)
                {
                    // ��� �÷��̾� �� ���� ���� �÷��̾ ����
                    List<PlayerData> potentialTargets = players.FindAll(player => !player.isDead);
                    Debug.Log($"Ÿ�� �ĺ� ��: {potentialTargets.Count}");

                    if (potentialTargets.Count > 0)
                    {
                        // remain�� 0���� �ĺ� �������� �ݺ��ǵ��� ����
                        int targetIndex = remain % potentialTargets.Count; // remain�� players ����� ������ ���� ������ �ε��� ��
                        PlayerData target = potentialTargets[targetIndex];
                        Debug.Log($"���õ� Ÿ��: {target.playerName} (�ε���: {targetIndex})");

                        // remain ���� �������Ѽ� ���� ȣ�� �� �ٸ� Ÿ���� �����ϵ��� ����
                        remain++;
                        return target;
                    }
                }
                break;

            case TargetType.AllExceptSelf:
                if (players != null && players.Count > 0)
                {
                    // �ڽ��� ������ Ÿ�� ��� ���� (���� �÷��̾� ����)
                    List<PlayerData> potentialTargets = players.FindAll(player => player.id != currentPlayer.id && !player.isDead);

                    Debug.Log($"Ÿ�� �ĺ� ��: {potentialTargets.Count}");
                    if (potentialTargets.Count > 0)
                    {
                        // ���� ���: �������� ����
                        PlayerData target = potentialTargets[Random.Range(0, potentialTargets.Count)];
                        Debug.Log($"���õ� Ÿ��: {target.playerName}");
                        return target;
                    }
                    else
                    {
                        Debug.LogWarning("Ÿ�� �ĺ��� �����ϴ�!");
                    }
                }
                break;

            default:
                Debug.LogWarning("�� �� ���� TargetType�Դϴ�.");
                break;
        }

        Debug.LogWarning("Ÿ�� ���� ����!");
        return null;
    }
}
