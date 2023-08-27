using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAgent : PoolObject
{
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayAudio(AudioClip clip,bool hasLoop)
    {
        audioSource.clip = clip;
        audioSource.loop = hasLoop;
        audioSource.time = 0f;
        audioSource.Play();
        if(!hasLoop)
            StartCoroutine(FinishedPlaying(audioSource.clip.length));
    }
    /// <summary>
    /// ������ڲ��ŵ���Ƶ
    /// </summary>
    /// <returns></returns>
    public AudioClip GetClip()
    {
        return audioSource.clip;
    }
    /// <summary>
    /// ��ͣ����
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
    }
    /// <summary>
    /// ֹͣ����
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
    }
    IEnumerator FinishedPlaying(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        //����Ժ��Լ�����
        PoolManager.Instance.ReturnObjectToPool("audio",this);
    }
    public bool IsLoop()
    {
        return audioSource.loop;
    }

    public void SetVolume(float v)
    {
        audioSource.volume = v;
    }

    public void SetMute(bool v)
    {
        audioSource.mute = v;
    }
}
