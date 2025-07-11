using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Pickup Objects/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public float weight;
    public int value;
    public int maxStack;

    public bool IsStackable => maxStack > 1;
}
