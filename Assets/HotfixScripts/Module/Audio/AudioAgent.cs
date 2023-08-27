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
    /// 获得正在播放的音频
    /// </summary>
    /// <returns></returns>
    public AudioClip GetClip()
    {
        return audioSource.clip;
    }
    /// <summary>
    /// 暂停播放
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        audioSource.Stop();
    }
    IEnumerator FinishedPlaying(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        //完成以后自己回收
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
