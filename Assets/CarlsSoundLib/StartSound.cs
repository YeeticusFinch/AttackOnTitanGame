using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StartSound : MonoBehaviour
{

    public SoundLib sound = new SoundLib();
    public static StartSound instance;

    public float minDistance;

    // Use this for initialization
    void Start()
    {
        instance = this;
        sound.init();
        sound.minDistance = minDistance;
        SoundManager.sound = sound;

        //GameObject charSelect = Instantiate(Resources.Load("CharSelect"), Vector3.zero, transform.rotation) as GameObject;
        //charSelect.transform.GetChild(0).gameObject.SetActive(false);

    }

    public static void stopPlayingStruct(float time, SoundManager.Sound stru)
    {
        //GameObject.FindObjectOfType<PlayerShip>().StartCoroutine(StopPlaying(time, stru));
    }

    public static IEnumerator StopPlaying(float time, SoundManager.Sound stru)
    {
        //stru.playing = true;
        //Debug.Log("Sound: " + stru.file + " stru.playing = " + stru.playing);
        yield return new WaitForSecondsRealtime(time);
        //stru.playing = false;
        //Debug.Log("Sound: " + stru.file + " stru.playing = " + stru.playing);
        try
        {
            if (stru.file.Length > 0)
                stru.playing = false;
        }
        catch (System.Exception e)
        {

        }
    }
}
