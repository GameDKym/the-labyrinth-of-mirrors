using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginGameManager : MonoBehaviour
{
    public static LoginGameManager instance;
    public Transform canvas; // 텍스트가 표시될 캔버스
    public Font customFont; // 사용자 글꼴
    public float messageDuration = 1f; // 메시지 지속 시간
    public int gameMode;

    void Awake()
    {
        instance = this;
        gameMode = 0;
        DontDestroyOnLoad(this);
    }

    // 포톤 접속 실패
    public void FailedToConnect()
    {
        StartCoroutine(PrintMessage("서버 접속 실패"));
    }

    // 방이 가득참
    public void RoomIsFull()
    {
        StartCoroutine(PrintMessage("방이 가득 찼습니다"));
    }

    // 방 참가 실패
    public void JoinFailed()
    {
        StartCoroutine(PrintMessage("방에 참가할 수 없습니다"));
    }

    // 메시지 출력
    private IEnumerator PrintMessage(string message)
    {
        // 텍스트 객체 생성
        GameObject tempTextObj = new GameObject("MessageText");
        tempTextObj.transform.SetParent(canvas); // 텍스트를 캔버스의 자식으로 설정

        // Text 컴포넌트 추가
        Text tempText = tempTextObj.AddComponent<Text>();

        // 사용자 지정 글꼴 적용
        if (customFont != null)
        {
            tempText.font = customFont; // 글꼴 설정
        }

        // 텍스트 설정
        tempText.text = message;
        tempText.fontSize = 24; // 글꼴 크기 설정
        tempText.color = Color.white; // 글꼴 색상 설정

        // 텍스트 위치 설정 (원하는 위치로 설정)
        tempText.rectTransform.anchoredPosition = new Vector2(0, 0);

        // 메시지가 일정 시간 후에 사라지도록 설정
        yield return new WaitForSeconds(messageDuration);

        // 텍스트 삭제
        Destroy(tempTextObj);
    }
}
