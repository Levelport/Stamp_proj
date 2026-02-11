using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/SoundLibrary")]
public class SoundLibrary_H : ScriptableObject
{
    public List<SoundData> sounds;

    private Dictionary<string, AudioClip> soundDict;

    public void Init()
    {
        soundDict = new Dictionary<string, AudioClip>();

        foreach (var s in sounds)
        {
            if (!soundDict.ContainsKey(s.name))
                soundDict[s.name] = s.clip;
        }
    }

    public AudioClip Get(string name)
    {
        if (soundDict.TryGetValue(name, out var clip))
            return clip;

        Debug.LogWarning($"Sound '{name}' �����C�u�����ɑ��݂��Ȃ��B");
        return null;
    }


}

[System.Serializable]
public class SoundData
{
    public string name;
    public AudioClip clip;
}


