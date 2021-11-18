using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public string clipName;

    public AudioClip[] clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(0f, 256f)]
    public int priority;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
