using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoleSO", menuName = "Game/RoleSO")]
public class RoleSO : ScriptableObject
{
    public List<RoleData> roles; // ���� ����Ʈ
    public void InitializeRoles()
    {
        roles = new List<RoleData>
        {
            new RoleData { roleName = "������", roleType = Role.TeamLeader },
            new RoleData { roleName = "�ΰ�����", roleType = Role.Lieutenant },
            new RoleData { roleName = "���ð���", roleType = Role.Doppelganger },
            new RoleData { roleName = "���汳��", roleType = Role.DarkSorcerer }
        };

        Debug.Log("RoleSO �����Ͱ� �ʱ�ȭ�Ǿ����ϴ�.");
    }
}

[System.Serializable]
public class RoleData
{
    public string roleName; // ���� �̸�
    public Role roleType;   // ���� Enum (������, �δ��� ��)
}

public enum Role
{
    TeamLeader,      // ������
    Lieutenant,      // �δ���
    Doppelganger,    // ���ð���
    DarkSorcerer     // ���汳��
}
