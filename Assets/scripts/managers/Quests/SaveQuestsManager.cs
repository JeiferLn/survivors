using System.IO;
using UnityEngine;

public class SaveQuestsManager : MonoBehaviour
{
    [SerializeField]
    private QuestManager questManager;

    private string SavePath => Path.Combine(Application.persistentDataPath, "quests.save");

    private void Awake()
    {
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
        }
    }

    private void Start()
    {
        if (questManager == null)
        {
            questManager = QuestManager.Instance;
        }

        if (questManager != null)
        {
            Load();
        }
        else
        {
            Debug.LogError(
                "[SaveQuestsManager] QuestManager no encontrado. Asegúrate de que QuestManager se inicialice antes."
            );
        }
    }

    public void Save()
    {
        var data = questManager.GetSaveData();
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Quests guardadas");
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
            return;

        var json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<QuestSaveData>(json);

        questManager.LoadFromSaveData(data);
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
