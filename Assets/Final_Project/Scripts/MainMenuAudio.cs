using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAudio : MonoBehaviour
{
    public AudioClip[] musicList;
    private AudioSource audioSource;
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying) 
        {
            audioSource.clip = PlayRandomBackGroundMusic();
            audioSource.Play();
        }
    }
    AudioClip PlayRandomBackGroundMusic() 
    {
        int rand = Random.Range(0, musicList.Length - 1);
       // Debug.Log(rand);
       // Debug.Log(musicList.Length);
        return musicList[rand];
    }
}
