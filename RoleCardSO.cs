using UnityEngine;

[CreateAssetMenu(fileName = "RoleCardSO", menuName = "Game/RoleCardSO")]
public class RoleCardSO : ScriptableObject
{
    public string roleName;             // 역할 이름
    public string description;          // 역할 설명 (승리 조건 포함)
    public Sprite roleFrontImage;       // 역할 카드 앞면 이미지
    public Sprite roleBackImage;        // 역할 카드 뒷면 이미지
    public Role roleType;               // 역할 타입 (열거형)
}
