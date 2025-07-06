using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SoundLib {
    public float volume = 1f;
    public float pitch = 1f;
    public float distMult = 1f;
    public float minDistance = 1;
    public AudioClip[] audioClips;
    //public string[] names;
    public Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    Dictionary<string, List<AudioSource>> sources = new Dictionary<string, List<AudioSource>>();

    public void init()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i] == null || audioClips[i].name == null || audioClips[i].name.Equals(""))
                continue;
            clips.Add(audioClips[i].name, audioClips[i]);
            //Debug.Log("Added " + audioClips[i].name);
        }
    }

    public bool stop(SoundManager.Sound stru)
    {
        //Debug.Log("Trying to stop " + stru.file);
        if (sources.ContainsKey(stru.file))
        {
            foreach (AudioSource source in sources[stru.file])
                if (source != null)
                    source.Stop();
            sources.Remove(stru.file);
            //Debug.Log("Stopping " + stru.file);
            return true;
        }
        //Debug.Log("Not stopping " + stru.file);
        return false;
    }

    public GameObject playAt(SoundManager.Sound stru, string clip, Vector3 pos, float vol = 1f, float pitch = 1f, float startPos = 0f, float endPos = -1f, float maxDistance = 20f)
    {
        //Debug.Log("Searching for " + clip + " in " + clips);
        if (clips.ContainsKey(clip))
        {
            return PlayClipAtPoint(stru, clips[clip], pos, vol * volume, pitch * this.pitch, startPos, endPos, maxDistance * distMult);
        } else
        {
            Debug.LogWarning("Couldn't find sound " + clip);
            return null;
        }
    }

    GameObject PlayClipAtPoint(SoundManager.Sound stru, AudioClip clip, Vector3 position, float volume, float pitch, float startPos = 0f, float endPos = -1f, float maxDistance = 20f)
    {
        if (clip != null)
        {
            GameObject obj = new GameObject();
            //obj.transform.position = position + Vector3.forward * maxDistance * (1f-volume); // Incase volume doesn't work
            obj.transform.position = position;
            AudioSource source = obj.AddComponent<AudioSource>();
            if (stru.stoppable)
            {
                if (sources.ContainsKey(stru.file))
                    sources[stru.file].Add(source);
                else
                    sources.Add(stru.file, new List<AudioSource>() { source });
                //Debug.Log("Adding " + stru.file + " to sources ");
            }
            source.volume = volume;
            //obj.GetComponent<AudioSource>().
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            //source.spatialBlend = 1f;
            source.pitch = pitch;
            source.clip = clip;
            //obj.GetComponent<AudioSource>().volume = volume*this.volume;
            //StartCoroutine(ISetVolume(obj.GetComponent<AudioSource>(), volume * this.volume));
            source.PlayScheduled(AudioSettings.dspTime+0.001f);
            if (startPos > 0)
                source.time = startPos;
            //obj.GetComponent<AudioSource>().SetScheduledStartTime(AudioSettings.dspTime + startPos);
            if (endPos > startPos)
                //obj.GetComponent<AudioSource>().
                source.SetScheduledEndTime(AudioSettings.dspTime + (endPos - startPos)/pitch);
            //obj.GetComponent<AudioSource>().PlayOneShot(clip, volume);
            stru.playing = true;
            if (endPos != -1)
            {
                StartSound.stopPlayingStruct(endPos - startPos + stru.extraDelay, stru);
                GameObject.Destroy(obj, endPos - startPos);
            }
            else
            {
                StartSound.stopPlayingStruct(clip.length / pitch - startPos + stru.extraDelay, stru);
                GameObject.Destroy(obj, clip.length / pitch - startPos);
            }
            return obj;
        }
        return null;
    }


    private System.Collections.IEnumerator ISetVolume(AudioSource m_source, float Volume)
    {
        //Unity can't change volume of Audio Source Component instantly with no reason!
        yield return new WaitForSeconds(0.0012f);
        m_source.volume = Volume;
    }

    public void PlayAtObject(string clip, GameObject obj, float vol = 1f, float pitch = 1f, float startPos = 0f, float endPos = -1f, float maxDistance = 20f)
    {
        if (clips.ContainsKey(clip))
        {
            AudioSource yeet = obj.AddComponent<AudioSource>();
            yeet.rolloffMode = AudioRolloffMode.Linear;
            yeet.minDistance = minDistance;
            yeet.maxDistance = maxDistance * distMult;
            //yeet.spatialBlend = 1f;
            yeet.pitch = pitch * this.pitch;
            yeet.PlayOneShot(clips[clip], vol * volume);
            if (startPos > 0)
                yeet.time = startPos;
            if (endPos > startPos)
                yeet.SetScheduledEndTime(AudioSettings.dspTime + (endPos - startPos));
            //obj.RemoveComponent<AudioSource>();
            AudioSource.Destroy(yeet, clips[clip].length / (pitch * this.pitch));
        }
    }

    public void PlayAtObject(AudioClip clip, GameObject obj, float vol = 1f, float pitch = 1f, float startPos = 0f, float endPos = -1f, float maxDistance = 20f)
    {
        if (true /*clips.ContainsKey(clip)*/)
        {
            AudioSource yeet = obj.AddComponent<AudioSource>();
            yeet.rolloffMode = AudioRolloffMode.Linear;
            yeet.minDistance = minDistance;
            yeet.maxDistance = maxDistance * distMult;
            //yeet.spatialBlend = 1f;
            yeet.pitch = pitch * this.pitch;
            yeet.PlayOneShot(clip, vol * volume);
            if (startPos > 0)
                yeet.time = startPos;
            if (endPos > startPos)
                yeet.SetScheduledEndTime(AudioSettings.dspTime + (endPos - startPos));
            //obj.RemoveComponent<AudioSource>();
            AudioSource.Destroy(yeet, clip.length / (pitch * this.pitch));
        }
    }

}
