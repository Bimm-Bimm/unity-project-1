using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISoundEffect : MonoBehaviour
{
    public static UISoundEffect Instance;
    public AudioClip _clickSound;
    public AudioClip _dialogSound_ON;
    public AudioClip _dialogSound_OFF;
    private new AudioSource audio;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    void Start()
    {
        audio = gameObject.GetComponent<AudioSource>();
    }
    public void PlaySound(AudioClip sound) 
    {
        audio.PlayOneShot(sound);
    }
    public void PlayClickSound()
    {
        PlaySound(_clickSound);
    }
    public void PlayDialogOn()
    {
        PlaySound(_dialogSound_ON);
    }

    public void PlayDialogOff()
    {
        PlaySound(_dialogSound_OFF);
    }



}
