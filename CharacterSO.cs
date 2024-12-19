using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Game/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [Header("캐릭터 기본 정보")]
    public string characterName;         // 캐릭터 이름
    public int baseAttack = 0;           // 기본 공격력
    public int baseHealth = 10;          // 기본 체력
    public int baseDefense = 0;          // 기본 방어력
    public Sprite characterSprite;       // 캐릭터 이미지

    [Header("캐릭터 고유 능력")]
    public string abilityName;           // 능력 이름
    [TextArea] public string description; // 능력 설명
    public AbilityEffect abilityEffect;  // 고유 능력 Enum
}
