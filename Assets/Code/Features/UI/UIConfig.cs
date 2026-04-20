using UnityEngine;

[CreateAssetMenu(menuName = "Game/Configs/UI Config")]
public class UIConfig : ScriptableObject
{
    [Header("Popups")]
    public GameObject[] Popups;
}