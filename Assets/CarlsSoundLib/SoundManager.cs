using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundLib sound;

    [System.Serializable]
    public struct Sound
    {
        public Sound(string f, float v = 0.5f, float vv = 0.05f, float p = 1f, float pv = 0.05f, float s = 0.1f, float e = -1f, float d = 20f, bool attch = false, bool canOverlap = true, bool hasVariations = false, int variationCount = 0, float extraDelay = 0, float overrideDelay = -1f, bool stoppable = false)
        {
            file = f;
            vol = v;
            volVar = vv;
            pitch = p;
            pitchVar = pv;
            start = s;
            end = e;
            dist = d;
            played = false;
            attach = attch;
            playing = false;
            this.canOverlap = canOverlap;
            this.hasVariations = hasVariations;
            this.variationCount = variationCount;
            this.extraDelay = extraDelay;
            this.overrideDelay = overrideDelay;
            this.stoppable = stoppable;
            lastPlayed = 0;
        }
        public string file;
        public float vol;
        public float volVar;
        public float pitch;
        public float pitchVar;
        public float start;
        public float end;
        public float dist;
        public bool played;
        public bool attach;
        public bool playing;
        public bool canOverlap;
        public bool hasVariations;
        public int variationCount;
        public float extraDelay;
        public float lastPlayed;
        public float overrideDelay;
        public bool stoppable;

        public void stop()
        {
            if (file.Length < 1)
                return;
            sound.stop(this);
        }

        public void play(Vector3 pos, GameObject source = null)
        {
            if (file.Length < 1) return;
            if (!canOverlap && playing)
            {
                float funnyDelay = overrideDelay > 0 ? overrideDelay : (end > 0 ? end : ((StartSound.instance.sound.clips[file].length / pitch)) - start + extraDelay);
                float nextPlay = lastPlayed + funnyDelay;
                //Debug.Log("Realtime = " + Time.realtimeSinceStartup + " last time = " + lastPlayed + " funny delay = " + funnyDelay + " with funny delay = " + nextPlay);
                if (Time.realtimeSinceStartup > nextPlay)
                    playing = false;
            }
            //Debug.Log("Play");
            if (!canOverlap && playing)
                return;
            else
            {
                //Debug.Log("Yeet");
                string append = "";
                if (hasVariations)
                    append = "" + Random.Range(0, variationCount);
                GameObject soundObj = sound.playAt(this, file + append, pos, vol + Random.Range(-volVar, volVar), pitch + Random.Range(-pitchVar, pitchVar), start, end, dist);
                played = true;
                if (attach && source != null)
                    soundObj.transform.parent = source.transform;
                playing = true;
                lastPlayed = Time.realtimeSinceStartup;
            }
        }
    }

}
