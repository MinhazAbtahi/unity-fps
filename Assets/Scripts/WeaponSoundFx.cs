using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponSoundFx : MonoBehaviour
{

    private AudioSource audioSource;

    public AudioClip ShotSoundFx;
    public AudioClip ReloadSoundFx;
    public AudioClip CockSoundFx;
    public AudioClip BulletShellSoundFx;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Plays Weapon Sound Fx
    /// </summary>
    /// <param name="soundFx"></param>
    /// <param name="scaleValue"></param>
    public void PlaySoundFx(AudioClip soundFx, float scaleValue)
    {
        audioSource.PlayOneShot(soundFx, scaleValue);
    }
}
