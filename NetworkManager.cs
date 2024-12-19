using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    private List<RoomInfo> roomList = new List<RoomInfo>();
    private bool inLobby;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // ������ ��������
    public int GetUserCount()
    {
        return PhotonNetwork.CountOfPlayers;
    }

    // ��� ��������
    public int GetRoomsCount()
    {
        return PhotonNetwork.CountOfRooms;
    }

    // �κ� ����
    public IEnumerator JoinLobby()
    {
        int count = 0;
        while (!inLobby)
        {
            count++;
            yield return StartCoroutine(TryJoinLobby());
            if (count > 3)
                break;
        }
    }

    public bool CheckInLobby()
    {
        return inLobby;
    }

    // �� ����
    public void CreateRoom(string room_Name)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(room_Name, options);
    }

    // ���� �� ����
    public bool JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        if (PhotonNetwork.InRoom)
            return true;
        else
            return false;
    }

    // �� ����
    public bool JoinRoom(string room_Name)
    {
        return PhotonNetwork.JoinRoom(room_Name);
    }

    // �� ��� ��������
    public List<LobbyRoom> GetRoomList()
    {
        List<LobbyRoom> room_list = new List<LobbyRoom>();
        foreach (RoomInfo room_info in roomList)
        {
            LobbyRoom lobby_room = new LobbyRoom();
            lobby_room.roomName = room_info.Name;
            lobby_room.maxPlayers = room_info.MaxPlayers;
            lobby_room.playerCount = room_info.PlayerCount;
            room_list.Add(lobby_room);
        }

        return room_list;
    }

    private IEnumerator TryJoinLobby()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("���� ���� �õ�");
            PhotonNetwork.ConnectUsingSettings();
            yield return new WaitForSeconds(0.5f);
        }
        else if (!inLobby)
        {
            Debug.Log("�κ� ���� �õ�");
            PhotonNetwork.JoinLobby();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        inLobby = true;
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        inLobby = false;
    }

    public override void OnRoomListUpdate(List<RoomInfo> room_List)
    {
        base.OnRoomListUpdate(room_List);
        roomList = room_List;
    }
}