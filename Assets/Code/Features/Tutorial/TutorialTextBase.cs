using UnityEngine;

[CreateAssetMenu(menuName = "Game/Configs/TutorialConfig")]
public class TutorialTextBase : ScriptableObject
{
    [TextArea] public string[] Text;
}