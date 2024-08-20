using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] swordClashAudio;
    [SerializeField] private AudioClip[] damageInflictedAudio;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            PlayRandomClip(damageInflictedAudio);
        }
        else
        {
            PlayRandomClip(swordClashAudio);
        }

    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        int rnd = Random.Range(0, clips.Length);

        source.clip = clips[rnd];
        source.Play();
    }
}
