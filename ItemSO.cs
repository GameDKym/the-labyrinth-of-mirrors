using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;       // 카드 이름
    public string type;       // 카드 유형 (공격, 방어, 장착 등)
    public string effect;     // 카드 효과
    public int quantity;   // 카드 수량 (Inspector에서 조정 가능)
    [Range(0, 10)] public int damage;      // 카드 데미지 (공격 카드에 사용)
    [Range(0, 10)] public int heal;        // 회복량 (회복 카드에 사용)
    public TargetType targetType; // TargetType 사용
    public Sprite sprite;     // 카드 이미지 (Unity에서 연결 가능)
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Game/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
