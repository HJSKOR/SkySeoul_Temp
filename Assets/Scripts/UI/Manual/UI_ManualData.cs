using UnityEngine;

[CreateAssetMenu(fileName = "NewManualData", menuName = "Custom/ManualData")]
public class UI_ManualData : ScriptableObject
{
    public string Title;
    public Sprite Image;
    [TextArea]public string Description;
}
