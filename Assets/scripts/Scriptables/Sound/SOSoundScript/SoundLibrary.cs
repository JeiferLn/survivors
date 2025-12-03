using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
    }

    public List<SoundEntry> sounds = new List<SoundEntry>();

    public AudioClip GetClip(string id)
    {
        foreach (var s in sounds)
        {
            if (s.id == id)
                return s.clip;
        }

        Debug.LogWarning($"[SoundLibrary] No se encontró el sonido con id: {id}");
        return null;
    }
}
