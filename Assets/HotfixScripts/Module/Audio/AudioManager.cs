using Google.Protobuf.WellKnownTypes;
using ObjectPool;
using System.Collections.Generic;
using UnityEngine;
namespace Audio
{
    public class AudioManager : ModuleSingleton<AudioManager>, IModule
    {
        private Dictionary<AudioLayer, AudioLayerWarpper> layers = new Dictionary<AudioLayer, AudioLayerWarpper>(4);
        private string poolName = "audio";
        private Dictionary<int, AudioAgent> currentPlayAgents;
        public void OnCreate(object createParam)
        {
            var root = new GameObject("[AudioMananger]");
            root.AddComponent<AudioListener>();
            Object.DontDestroyOnLoad(root);

            var agent = new GameObject("audioAgent");
            agent.AddComponent<AudioAgent>();
            PoolManager.Instance.CreatePool(new PoolInfo(poolName, 10, agent),false);

            GameObject.Destroy(agent);
            layers.Add(AudioLayer.Music, new AudioLayerWarpper(AudioLayer.Music));
            layers.Add(AudioLayer.Voice, new AudioLayerWarpper(AudioLayer.Voice));
            layers.Add(AudioLayer.Ambient, new AudioLayerWarpper(AudioLayer.Ambient));
            layers.Add(AudioLayer.Sound, new AudioLayerWarpper(AudioLayer.Sound));

            currentPlayAgents = new Dictionary<int, AudioAgent>(10);
        }
        #region 控制

        public void SetVolume(AudioLayer layer, float value)
        {
            layers[layer].SetVolume(value);
        }
        public void SetMute(AudioLayer layer, bool mute)
        {
            layers[layer].SetMute(mute);
        }
        #endregion

        #region 播放
        public void PlayAudio(AudioData audioData, Vector3 position = default)
        {
            AudioClip[] clipsToPlay = audioData.GetClips();
            AudioAgent[] audioagentrArray = new AudioAgent[clipsToPlay.Length];
            int[] results = new int[clipsToPlay.Length];

            for (int i = 0; i < clipsToPlay.Length; i++)
            {
                var agent =  PoolManager.Instance.GetGameObjectToPool<AudioAgent>(poolName, position, Quaternion.identity);
                audioagentrArray[i] = agent;
                if (audioagentrArray[i] != null)
                {
                    audioagentrArray[i].PlayAudio(clipsToPlay[i], audioData.looping);
                }
            }
            layers[audioData.layer].AddAgent(audioagentrArray);
        }
        public void PlayAudio(AudioClip clip, AudioLayer layer, bool loop = false, Vector3 position = default)
        {
            if(clip== null)
            {
                Debug.LogError("clip is null");
                return;
            }
            var agent = PoolManager.Instance.GetGameObjectToPool<AudioAgent>(poolName, position, Quaternion.identity);
            if (agent != null)
            {
                agent.PlayAudio(clip, loop);
            }
            layers[layer].AddAgent(agent);
        }

        public void StopAudio(int id, float duration = 0)
        {

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
}