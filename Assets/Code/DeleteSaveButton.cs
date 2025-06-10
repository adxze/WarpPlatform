using UnityEngine;

public class DeleteSaveButton : MonoBehaviour
{
    public void DeleteSaveData()
    {
        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.ClearSaveDataAndStartFromLevel1();
            Debug.Log("Delete Save Data");
        }
        else
        {
            Debug.LogWarning("SaveSystem not found!");
        }
    }
}