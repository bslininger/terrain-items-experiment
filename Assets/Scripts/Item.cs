using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Pickup Objects/Item")]
public class Item : ScriptableObject
{
    public string name;
    public Sprite icon;
    public float weight;
    public int value;
}
