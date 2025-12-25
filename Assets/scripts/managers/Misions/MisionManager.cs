using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    public MissionData startingMission;
    public MissionData[] allMissions;

    private MissionState currentMission;
    private Dictionary<string, MissionState> completedMissions = new();

    private string SavePath => Path.Combine(Application.persistentDataPath, "missions.json");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadProgress();
    }

    private void Update()
    {
        if (currentMission == null)
            return;

        currentMission.Update(Time.deltaTime);

        if (currentMission.IsFailed)
        {
            currentMission = null;
            SaveProgress();
        }
    }

    public void ReportEvent(MissionObjectiveType type, string targetID, int amount = 1)
    {
        if (currentMission == null)
            return;

        currentMission.AddProgress(type, targetID, amount);

        if (currentMission.IsCompleted)
        {
            CompleteCurrentMission();
        }

        SaveProgress();
    }

    private void StartMission(MissionData mission)
    {
        if (mission == null)
            return;

        currentMission = new MissionState(mission);
        SaveProgress();
    }

    private void CompleteCurrentMission()
    {
        completedMissions.Add(currentMission.data.missionID, currentMission);

        MissionData next = currentMission.data.nextMission;
        currentMission = null;

        StartMission(next);
    }

    private void SaveProgress()
    {
        MissionSaveData save = new();

        if (currentMission != null)
            save = currentMission.ToSaveData();

        foreach (var id in completedMissions.Keys)
            save.completedMissionIDs.Add(id);

        File.WriteAllText(SavePath, JsonUtility.ToJson(save, true));
    }

    private void LoadProgress()
    {
        if (!File.Exists(SavePath))
        {
            StartMission(startingMission);
            return;
        }

        var json = File.ReadAllText(SavePath);
        var save = JsonUtility.FromJson<MissionSaveData>(json);

        foreach (var id in save.completedMissionIDs)
            completedMissions[id] = null;

        MissionData mission = FindMissionByID(save.currentMissionID);
        if (mission == null)
        {
            StartMission(startingMission);
            return;
        }

        currentMission = new MissionState(mission);
        currentMission.LoadFromSave(save);
    }

    private MissionData FindMissionByID(string id)
    {
        foreach (var mission in allMissions)
            if (mission.missionID == id)
                return mission;

        return null;
    }
}
