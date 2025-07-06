using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FancySound : MonoBehaviour
{
    static FancySound instance;

    static Dictionary<(AudioClip, GameObject), AudioSource> playing = new Dictionary<(AudioClip, GameObject), AudioSource>();

    const float DEFAULT_PITCH = 1;
    const float DEFAULT_VOLUME = 0.8f;
    const float DEFAULT_MIN_DIST = 0.001f;
    const float DEFAULT_MAX_DIST = 1.4f;

    private void Start()
    {
        instance = this;
    }

    public static void Play(AudioClip clip, Vector3 pos, GameObject obj = null, bool overlap = true, float volume = DEFAULT_VOLUME, float pitch = DEFAULT_PITCH, float minDist = DEFAULT_MIN_DIST, float maxDist = DEFAULT_MAX_DIST)
    {
        GameObject playAt = new GameObject();
        playAt.transform.position = pos;
        if (obj != null)
        {
            playAt.transform.parent = obj.transform;
        }

        if (!overlap)
        {
            if (playing.ContainsKey((clip, obj)))
                return;
        }

        instance.StartCoroutine(instance.Remove(clip, obj, clip.length / pitch));

        AudioSource source = playAt.AddComponent<AudioSource>();
        if (playing.ContainsKey((clip, obj)))
            playing[(clip, obj)] = source;
        else
            playing.Add((clip, obj), source);

        source.clip = clip;
        source.Play();
    }

    public static void Stop(AudioClip clip, GameObject obj)
    {
        if (playing.ContainsKey((clip, obj)))
        {
            playing[(clip, obj)].Stop();
            GameObject.Destroy((playing[(clip, obj)].gameObject));
            playing.Remove((clip, obj));
        }
    }

    IEnumerator Remove(AudioClip clip, GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);

        Stop(clip, obj);
    }
}
