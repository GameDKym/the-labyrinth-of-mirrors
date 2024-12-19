using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

// TargetType 열거형 정의
public enum TargetType
{
    Self,           // 나 자신
    Single,         // 특정 대상 1명 (자신 포함)
    All,            // 전체 대상 (자신 포함)
    AllExceptSelf   // 전체 대상 (자신 제외)
}

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Inst { get; private set; } // 싱글턴 인스턴스
    private PlayerData selectedTarget;
    int remain = 0;
    private void Awake()
    {
        // 싱글턴 초기화
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        Inst = this;
        Debug.Log("TargetingManager가 초기화되었습니다.");
    }

    //Targeting 과정 매서드
    public void StartTargetSelection(List<PlayerData> players, System.Action<PlayerData> onTargetSelected)
    {
        TargetSelectionUI.Instance.Show(players, (target) =>
        {
            selectedTarget = target;
            onTargetSelected?.Invoke(selectedTarget);
        });
    }


    // 특정 대상(타겟)을 선택하는 메서드
    public PlayerData SelectTarget(List<PlayerData> players, TargetType targetType, PlayerData currentPlayer)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("TargetType -> SelectTarget 플레이어 목록이 비어있거나 null입니다.");
            return null;
        }

        switch (targetType)
        {
            case TargetType.Self:
                if (players != null && players.Count > 0)
                {
                    // 나 자신만을 선택
                    PlayerData target = players.Find(player => player.id == currentPlayer.id && !player.isDead);
                    if (target != null)
                    {
                        Debug.Log($"자신을 타겟으로 선택: {target.playerName}");
                        return target;
                    }
                }
                break;

            case TargetType.Single:
                if (players != null && players.Count > 0)
                {
                    // 나 자신을 제외한 타겟 목록 생성
                    List<PlayerData> potentialTargets = players.FindAll(player => player.id != currentPlayer.id && !player.isDead);

                    Debug.Log($"타겟 후보 수: {potentialTargets.Count}");
                    if (potentialTargets.Count > 0)
                    {
                        // 단일 대상: 랜덤으로 선택
                        PlayerData target = potentialTargets[Random.Range(0, potentialTargets.Count)];
                        Debug.Log($"선택된 타겟: {target.playerName}");
                        return target;
                    }
                    else
                    {
                        Debug.LogWarning("타겟 후보가 없습니다!");
                    }
                }
                break;

            case TargetType.All:
                Debug.Log("전체 대상 (자신 포함)");
                if (players != null && players.Count > 0)
                {
                    // 모든 플레이어 중 죽지 않은 플레이어만 선택
                    List<PlayerData> potentialTargets = players.FindAll(player => !player.isDead);
                    Debug.Log($"타겟 후보 수: {potentialTargets.Count}");

                    if (potentialTargets.Count > 0)
                    {
                        // remain이 0부터 후보 개수까지 반복되도록 수정
                        int targetIndex = remain % potentialTargets.Count; // remain을 players 목록의 개수로 나눈 나머지 인덱스 값
                        PlayerData target = potentialTargets[targetIndex];
                        Debug.Log($"선택된 타겟: {target.playerName} (인덱스: {targetIndex})");

                        // remain 값을 증가시켜서 다음 호출 때 다른 타겟을 선택하도록 설정
                        remain++;
                        return target;
                    }
                }
                break;

            case TargetType.AllExceptSelf:
                if (players != null && players.Count > 0)
                {
                    // 자신을 제외한 타겟 목록 생성 (죽은 플레이어 제외)
                    List<PlayerData> potentialTargets = players.FindAll(player => player.id != currentPlayer.id && !player.isDead);

                    Debug.Log($"타겟 후보 수: {potentialTargets.Count}");
                    if (potentialTargets.Count > 0)
                    {
                        // 단일 대상: 랜덤으로 선택
                        PlayerData target = potentialTargets[Random.Range(0, potentialTargets.Count)];
                        Debug.Log($"선택된 타겟: {target.playerName}");
                        return target;
                    }
                    else
                    {
                        Debug.LogWarning("타겟 후보가 없습니다!");
                    }
                }
                break;

            default:
                Debug.LogWarning("알 수 없는 TargetType입니다.");
                break;
        }

        Debug.LogWarning("타겟 선택 실패!");
        return null;
    }
}
