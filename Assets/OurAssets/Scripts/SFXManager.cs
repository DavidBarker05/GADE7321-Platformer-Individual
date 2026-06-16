using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SFXEntry
{
    public string Id;
    public AudioClip Sound;

    // David - Implicitly convert SFXEntry to KeyValuePair for dictionary
    // makes life easier
    public static implicit operator KeyValuePair<string, AudioClip>(SFXEntry entry) => new KeyValuePair<string, AudioClip>(entry.Id, entry.Sound);
}

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [field: SerializeField]
    public AudioSource DefaultAudioSource { get; private set; }
    [SerializeField]
    List<SFXEntry> m_PseudoDictionary;

    HashMapADT<string, AudioClip> m_AudioMap;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            m_AudioMap = new HashMapADT<string, AudioClip>(m_PseudoDictionary.Count);
            AddMultiple(m_PseudoDictionary);
        }
    }

    public AudioClip this[string id]
    {
        get => m_AudioMap[id];
        set => m_AudioMap[id] = value;
    }

    public void Add(SFXEntry entry) => m_AudioMap.Add(entry);
    public void Add(KeyValuePair<string, AudioClip> entry) => m_AudioMap.Add(entry);
    public void Add(string id, AudioClip sound) => m_AudioMap.Add(id, sound);

    public void AddMultiple(IEnumerable<SFXEntry> entries)
    {
        foreach (SFXEntry entry in entries) m_AudioMap.Add(entry);
    }
    public void AddMultiple(IEnumerable<KeyValuePair<string, AudioClip>> entries)
    {
        foreach (KeyValuePair<string, AudioClip> entry in entries) m_AudioMap.Add(entry);
    }

    public void AddUnique(SFXEntry entry)
    {
        if (!m_AudioMap.ContainsKey(entry.Id)) m_AudioMap.Add(entry);
    }
    public void AddUnique(KeyValuePair<string, AudioClip> entry)
    {
        if (!m_AudioMap.ContainsKey(entry.Key)) m_AudioMap.Add(entry);
    }
    public void AddUnique(string id, AudioClip sound)
    {
        if (!m_AudioMap.ContainsKey(id)) m_AudioMap.Add(id, sound);
    }

    public void AddMultipleUnique(IEnumerable<SFXEntry> entries)
    {
        foreach (SFXEntry entry in entries)
        {
            if (!m_AudioMap.ContainsKey(entry.Id)) m_AudioMap.Add(entry);
        }
    }
    public void AddMultipleUnique(IEnumerable<KeyValuePair<string, AudioClip>> entries)
    {
        foreach (KeyValuePair<string, AudioClip> entry in entries)
        {
            if (!m_AudioMap.ContainsKey(entry.Key)) m_AudioMap.Add(entry);
        }
    }

    public void Remove(SFXEntry entry) => m_AudioMap.Remove(entry);
    public void Remove(KeyValuePair<string, AudioClip> entry) => m_AudioMap.Remove(entry);
    public void Remove(string id, AudioClip sound) => m_AudioMap.Remove(id, sound);
    public void Remove(string id) => m_AudioMap.Remove(id);

    public void RemoveMultiple(IEnumerable<SFXEntry> entries)
    {
        foreach (SFXEntry entry in entries) m_AudioMap.Remove(entry);
    }
    public void RemoveMultiple(IEnumerable<KeyValuePair<string, AudioClip>> entries)
    {
        foreach (KeyValuePair<string, AudioClip> entry in entries) m_AudioMap.Remove(entry);
    }
    public void RemoveMultiple(IEnumerable<string> ids)
    {
        foreach (string id in ids) m_AudioMap.Remove(id);
    }

    public bool Contains(SFXEntry entry) => m_AudioMap.Contains(entry);
    public bool Contains(KeyValuePair<string, AudioClip> entry) => m_AudioMap.Contains(entry);
    public bool ContainsId(string id) => m_AudioMap.ContainsKey(id);

    public void PlayAudio(string id, AudioSource audioSource, bool loop = false)
    {
        if (!m_AudioMap.ContainsKey(id)) return;
        if (loop)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.clip = m_AudioMap[id];
            audioSource.loop = true;
            audioSource.Play();
        }
        else audioSource.PlayOneShot(m_AudioMap[id]);
    }

    public void StopAudio(string id, AudioSource audioSource)
    {
        if (!m_AudioMap.ContainsKey(id) || audioSource.clip != m_AudioMap[id]) return;
        audioSource.Stop();
        audioSource.clip = null;
    }
}
