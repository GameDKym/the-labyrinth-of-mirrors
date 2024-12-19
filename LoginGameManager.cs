using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginGameManager : MonoBehaviour
{
    public static LoginGameManager instance;
    public Transform canvas; // �ؽ�Ʈ�� ǥ�õ� ĵ����
    public Font customFont; // ����� �۲�
    public float messageDuration = 1f; // �޽��� ���� �ð�
    public int gameMode;

    void Awake()
    {
        instance = this;
        gameMode = 0;
        DontDestroyOnLoad(this);
    }

    // ���� ���� ����
    public void FailedToConnect()
    {
        StartCoroutine(PrintMessage("���� ���� ����"));
    }

    // ���� ������
    public void RoomIsFull()
    {
        StartCoroutine(PrintMessage("���� ���� á���ϴ�"));
    }

    // �� ���� ����
    public void JoinFailed()
    {
        StartCoroutine(PrintMessage("�濡 ������ �� �����ϴ�"));
    }

    // �޽��� ���
    private IEnumerator PrintMessage(string message)
    {
        // �ؽ�Ʈ ��ü ����
        GameObject tempTextObj = new GameObject("MessageText");
        tempTextObj.transform.SetParent(canvas); // �ؽ�Ʈ�� ĵ������ �ڽ����� ����

        // Text ������Ʈ �߰�
        Text tempText = tempTextObj.AddComponent<Text>();

        // ����� ���� �۲� ����
        if (customFont != null)
        {
            tempText.font = customFont; // �۲� ����
        }

        // �ؽ�Ʈ ����
        tempText.text = message;
        tempText.fontSize = 24; // �۲� ũ�� ����
        tempText.color = Color.white; // �۲� ���� ����

        // �ؽ�Ʈ ��ġ ���� (���ϴ� ��ġ�� ����)
        tempText.rectTransform.anchoredPosition = new Vector2(0, 0);

        // �޽����� ���� �ð� �Ŀ� ��������� ����
        yield return new WaitForSeconds(messageDuration);

        // �ؽ�Ʈ ����
        Destroy(tempTextObj);
    }
}
