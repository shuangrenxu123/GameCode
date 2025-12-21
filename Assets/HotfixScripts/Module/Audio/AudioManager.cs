using System;
using System.Collections.Generic;
using ObjectPool;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Audio
{
    public class AudioManager : ModuleSingleton<AudioManager>, IModule
    {
        private const string PoolName = "audio";
        private const int DefaultPoolSize = 10;

        private readonly Dictionary<AudioLayer, AudioLayerWarpper> layers = new Dictionary<AudioLayer, AudioLayerWarpper>(4);
        private readonly Dictionary<int, AudioAgent> currentPlayAgents = new Dictionary<int, AudioAgent>(32);
        private readonly List<FadeRequest> fadeRequests = new List<FadeRequest>(8);

        private Transform rootTransform;
        private bool poolInitialized;

        public void OnCreate(object createParam)
        {
            EnsureRoot();
            EnsurePool();
            InitializeLayers();

            currentPlayAgents.Clear();
            fadeRequests.Clear();
        }
        #region 控制

        public void SetVolume(AudioLayer layer, float value)
        {
            EnsureLayer(layer).SetVolume(value);
        }
        public void SetMute(AudioLayer layer, bool mute)
        {
            EnsureLayer(layer).SetMute(mute);
        }
        #endregion

        #region 播放
        public AudioAgent[] PlayAudio(AudioData audioData, Vector3 position = default)
        {
            if (audioData == null)
            {
                Debug.LogWarning("AudioData is null");
                return Array.Empty<AudioAgent>();
            }

            AudioClip[] clipsToPlay = audioData.GetClips();
            if (clipsToPlay == null || clipsToPlay.Length == 0)
            {
                Debug.LogWarning($"AudioData {audioData.name} does not contain any clips");
                return Array.Empty<AudioAgent>();
            }

            AudioAgent[] audioAgentArray = new AudioAgent[clipsToPlay.Length];

            for (int i = 0; i < clipsToPlay.Length; i++)
            {
                var clip = clipsToPlay[i];
                if (clip == null)
                {
                    Debug.LogWarning($"AudioData {audioData.name} has a null clip at index {i}");
                    continue;
                }

                var agent = SpawnAgent(position);
                audioAgentArray[i] = agent;
                if (agent == null)
                {
                    continue;
                }

                agent.PlayAudio(clip, audioData.looping);
                RegisterAgent(agent, audioData.layer);
            }

            return audioAgentArray;
        }
        public AudioAgent PlayAudio(AudioClip clip, AudioLayer layer, bool loop = false, Vector3 position = default)
        {
            if (clip == null)
            {
                Debug.LogError("clip is null");
                return null;
            }
            var agent = SpawnAgent(position);
            if (agent != null)
            {
                agent.PlayAudio(clip, loop);
                RegisterAgent(agent, layer);
            }
            return agent;
        }

        public void StopAudio(int id, float duration = 0)
        {
            if (!currentPlayAgents.TryGetValue(id, out var agent) || agent == null)
            {
                Debug.LogWarning($"Audio agent with id {id} is not playing");
                return;
            }

            if (duration <= 0f)
            {
                agent.Stop();
                return;
            }

            RemovePendingFade(agent);
            fadeRequests.Add(new FadeRequest
            {
                Agent = agent,
                Duration = Mathf.Max(0.01f, duration),
                StartVolume = agent.GetVolume(),
                Elapsed = 0f
            });
        }
        #endregion
        public void OnUpdate()
        {
            if (fadeRequests.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = fadeRequests.Count - 1; i >= 0; i--)
            {
                var request = fadeRequests[i];
                if (request.Agent == null)
                {
                    fadeRequests.RemoveAt(i);
                    continue;
                }

                request.Elapsed += deltaTime;
                float t = Mathf.Clamp01(request.Elapsed / request.Duration);
                request.Agent.SetVolume(Mathf.Lerp(request.StartVolume, 0f, t));

                if (t >= 1f || !request.Agent.isPlaying())
                {
                    fadeRequests.RemoveAt(i);
                    request.Agent.Stop();
                }
                else
                {
                    fadeRequests[i] = request;
                }
            }
        }

        private void InitializeLayers()
        {
            foreach (AudioLayer layer in Enum.GetValues(typeof(AudioLayer)))
            {
                EnsureLayer(layer);
            }
        }

        private AudioLayerWarpper EnsureLayer(AudioLayer layer)
        {
            if (!layers.TryGetValue(layer, out var wrapper))
            {
                wrapper = new AudioLayerWarpper(layer);
                layers[layer] = wrapper;
            }

            return wrapper;
        }

        private AudioAgent SpawnAgent(Vector3 position)
        {
            EnsureRoot();
            EnsurePool();

            var agent = PoolManager.Instance.GetGameObjectToPool<AudioAgent>(PoolName, position, Quaternion.identity);
            if (agent == null)
            {
                Debug.LogWarning($"Pool '{PoolName}' is exhausted, unable to play audio.");
                return null;
            }

            if (rootTransform != null)
            {
                agent.transform.SetParent(rootTransform, false);
            }
            agent.transform.position = position;
            return agent;
        }

        private void RegisterAgent(AudioAgent agent, AudioLayer layer)
        {
            if (agent == null)
                return;

            var wrapper = EnsureLayer(layer);
            wrapper.AddAgent(agent);

            int id = agent.GetInstanceID();
            currentPlayAgents[id] = agent;

            UnityAction release = null;
            release = () =>
            {
                agent.OnStop -= release;
                agent.OnEnd -= release;
                currentPlayAgents.Remove(id);
                wrapper.RemoveAgent(agent);
                RemovePendingFade(agent);
            };
            agent.OnStop += release;
            agent.OnEnd += release;
        }

        private void RemovePendingFade(AudioAgent agent)
        {
            if (agent == null || fadeRequests.Count == 0)
                return;

            for (int i = fadeRequests.Count - 1; i >= 0; i--)
            {
                if (fadeRequests[i].Agent == agent)
                {
                    fadeRequests.RemoveAt(i);
                }
            }
        }

        private void EnsureRoot()
        {
            if (rootTransform != null)
                return;

            GameObject root = GameObject.Find("[AudioManager]");
            if (root == null)
            {
                root = new GameObject("[AudioManager]");
            }

            if (Object.FindObjectOfType<AudioListener>() == null && root.GetComponent<AudioListener>() == null)
            {
                root.AddComponent<AudioListener>();
            }

            Object.DontDestroyOnLoad(root);
            rootTransform = root.transform;
        }

        private void EnsurePool()
        {
            if (poolInitialized)
                return;

            var agent = new GameObject("audioAgent");
            agent.AddComponent<AudioAgent>();
            PoolManager.Instance.CreatePool(new PoolInfo(PoolName, DefaultPoolSize, agent), false);
            Object.Destroy(agent);

            poolInitialized = true;
        }

        private struct FadeRequest
        {
            public AudioAgent Agent;
            public float Duration;
            public float Elapsed;
            public float StartVolume;
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