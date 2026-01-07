using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Door Data", menuName = "Doors/DoorData")]
public class DoorData : ScriptableObject
{
    public DoorType doorType;
    public string doorName;

    [ShowIf("@doorType == DoorType.KeyRequired")]
    public KeyId openKey;

    [ShowIf("@doorType == DoorType.Locked")]
    public string lockedMessage;

    [ShowIf("@doorType == DoorType.KeyRequired")]
    public string neededKeyMessage;

    [ShowIf("@doorType == DoorType.KeyRequired")]
    public string usedKeyMessage;
}
