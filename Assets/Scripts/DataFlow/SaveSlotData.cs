using UnityEngine;


// TODO: fill out with actual information
// e.g   play time, top score?, but also flags possibly
[CreateAssetMenu(fileName = "SaveSlotData", menuName = "Scriptable Objects/SaveSlotData")]
public class SaveSlotData : ScriptableObject
{
    // Instead of creating / deleting the slot, which is troublesome for reassigning
    // Keep the slots (instances) but check if it has data in it
    // And have the option to clear the data
    public bool IsSlotEmpty = true;
    public void ClearSlot()
    {
        IsSlotEmpty = true;

    }

    public void CreateSlot()
    {
        IsSlotEmpty = false;
    }
    print amogus

}
