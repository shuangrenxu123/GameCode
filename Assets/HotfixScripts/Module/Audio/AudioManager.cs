using System.Collections.Generic;
using UnityEngine;

public class AudioManager : ModuleSingleton<AudioManager>, IModule
{
    private Dictionary<AudioLayer, AudioLayerWarpper> layers = new Dictionary<AudioLayer, AudioLayerWarpper>();
    private string poolName = "audio";
    public void OnCreate(object createParam)
    {
        var root = new GameObject("[AudioMananger]");
        root.AddComponent<AudioListener>();
        Object.DontDestroyOnLoad(root);
        var agent = new GameObject("audioAgent");
        agent.AddComponent<AudioAgent>();
        agent.AddComponent<AudioSource>();
        PoolManager.Instance.CreatePool(new PoolInfo(poolName, 10,agent));

        layers.Add(AudioLayer.Music, new AudioLayerWarpper(AudioLayer.Music));
        layers.Add(AudioLayer.Voice, new AudioLayerWarpper(AudioLayer.Voice));
        layers.Add(AudioLayer.Ambient, new AudioLayerWarpper(AudioLayer.Ambient));
        layers.Add(AudioLayer.Sound, new AudioLayerWarpper(AudioLayer.Sound));

    }
    #region 控制
    #endregion

    #region 播放
    public void PlayAudio(AudioData audioData,Vector3 position = default)
    {
        AudioClip[] clipsToPlay = audioData.GetClips();
        AudioAgent[] audioagentrArray = new AudioAgent[clipsToPlay.Length];

        for (int i = 0; i < clipsToPlay.Length; i++)
        {
            audioagentrArray[i] = (AudioAgent)PoolManager.Instance.GetGameObjectToPool(poolName,position,Quaternion.identity);
            if (audioagentrArray[i] != null)
            {
                audioagentrArray[i].PlayAudio(clipsToPlay[i],audioData.looping);
            }
        }
        layers[audioData.layer].AddAgent(audioagentrArray);
    }

    #endregion
    public void OnUpdate()
    {

    }
}
public enum AudioLayer
{
    /// <summary>
    /// 音乐
    /// </summary>
    Music,
    /// <summary>
    /// BGM，背景音乐
    /// </summary>
    Ambient,
    /// <summary>
    /// 语音
    /// </summary>
    Voice,
    /// <summary>
    /// 音效
    /// </summary>
    Sound,
}
