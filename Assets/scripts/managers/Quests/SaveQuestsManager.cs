using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField]
    private QuestManager questManager;

    private string SavePath => Path.Combine(Application.persistentDataPath, "quests.save");

    private void Awake()
    {
        Load();
    }

    public void Save()
    {
        var saveData = questManager.GetSaveData();
        var json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
            return;

        var json = File.ReadAllText(SavePath);
        var saveData = JsonUtility.FromJson<QuestStateSaveData>(json);

        questManager.LoadFromSaveData(saveData);
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
