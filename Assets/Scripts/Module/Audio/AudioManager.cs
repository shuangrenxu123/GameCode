using System.Collections.Generic;
using UnityEngine;

public class AudioManager : ModuleSingleton<AudioManager>, IModule
{
    /// <summary>
    /// 音频源封装类
    /// </summary>
    private class AudioSourceWrapper
    {
        public GameObject Go { private set; get; }
        public AudioSource Source { private set; get; }
        public AudioSourceWrapper(string name, Transform emitter)
        {
            // Create an empty game object
            Go = new GameObject(name);
            Go.transform.position = emitter.position;
            Go.transform.parent = emitter;

            // Create the source
            Source = Go.AddComponent<AudioSource>();
            Source.volume = 1.0f;
            Source.pitch = 1.0f;
        }
    }

    private Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();
    private Dictionary<AudioLayer, AudioSourceWrapper> audioWrappers = new Dictionary<AudioLayer, AudioSourceWrapper>();
    public void OnCreate(object createParam)
    {
        var root = new GameObject("[AudioMananger]");
        root.AddComponent<AudioListener>();
        Object.DontDestroyOnLoad(root);
        foreach (var v in System.Enum.GetValues(typeof(AudioLayer)))
        {
            var layer = (AudioLayer)v;
            audioWrappers.Add(layer, new AudioSourceWrapper(layer.ToString(), root.transform));
        }
    }

    public void OnUpdate()
    {

    }
    public void Stop(AudioLayer layer)
    {
        audioWrappers[layer].Source.Stop();
    }
    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySound(string name)
    {
        if (IsMute(AudioLayer.Sound))
            return;
        PlayAudioClip(AudioLayer.Sound, name, false);
    }
    /// <summary>
    /// 播放语音
    /// </summary>
    public void PlayVoice(string name)
    {
        if (IsMute(AudioLayer.Voice))
            return;
        PlayAudioClip(AudioLayer.Voice, name, false);
    }
    /// <summary>
    /// 播放环境音效
    /// </summary>
    public void PlayAmbient(string name, bool isLoop)
    {
        PlayAudioClip(AudioLayer.Ambient, name, isLoop);
    }
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayMusic(string name, bool isLoop)
    {
        PlayAudioClip(AudioLayer.Music, name, isLoop);
    }
    /// <summary>
    /// 设置某个频道静音
    /// </summary>
    public void Mute(AudioLayer layer, bool isMute)
    {
        audioWrappers[layer].Source.mute = isMute;
    }
    /// <summary>
    /// 设置某个层级音量大小
    /// </summary>
    public void SetVolume(AudioLayer layer, float volume)
    {
        float v = Mathf.Clamp01(volume);
        audioWrappers[layer].Source.volume = v;
    }
    /// <summary>
    /// 设置所有的音量大小
    /// </summary>
    public void SetVolumeAll(float volume)
    {
        float v = Mathf.Clamp01(volume);
        foreach (var i in audioWrappers)
        {
            i.Value.Source.volume = v;
        }
    }
    public bool IsMute(AudioLayer layer)
    {
        return audioWrappers[layer].Source.mute;
    }
    private void PlayAudioClip(AudioLayer layer, string name, bool isLoop)
    {
        if (string.IsNullOrEmpty(name))
            return;
        if (!audios.ContainsKey(name))
        {
            var audio = Resources.Load<AudioClip>(name);
            if (audio == null)
            {
                Debug.LogError("没有找到对应的音频文件");
                return;
            }
            audios[name] = audio;
        }

        if (layer == AudioLayer.Music || layer == AudioLayer.Ambient || layer == AudioLayer.Voice)
        {
            audioWrappers[layer].Source.clip = audios[name];
            audioWrappers[layer].Source.loop = isLoop;
            audioWrappers[layer].Source.Play();
        }
        else if (layer == AudioLayer.Sound)
        {
            audioWrappers[layer].Source.PlayOneShot(audios[name]);
        }

    }
}
public enum AudioLayer
{
    Music,
    Ambient,
    Voice,
    Sound,
}
