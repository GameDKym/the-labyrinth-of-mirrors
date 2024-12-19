using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoleSO", menuName = "Game/RoleSO")]
public class RoleSO : ScriptableObject
{
    public List<RoleData> roles; // 역할 리스트
    public void InitializeRoles()
    {
        roles = new List<RoleData>
        {
            new RoleData { roleName = "공대장", roleType = Role.TeamLeader },
            new RoleData { roleName = "부공대장", roleType = Role.Lieutenant },
            new RoleData { roleName = "도플갱어", roleType = Role.Doppelganger },
            new RoleData { roleName = "암흑교단", roleType = Role.DarkSorcerer }
        };

        Debug.Log("RoleSO 데이터가 초기화되었습니다.");
    }
}

[System.Serializable]
public class RoleData
{
    public string roleName; // 역할 이름
    public Role roleType;   // 역할 Enum (공대장, 부대장 등)
}

public enum Role
{
    TeamLeader,      // 공대장
    Lieutenant,      // 부대장
    Doppelganger,    // 도플갱어
    DarkSorcerer     // 암흑교단
}
