using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// ��Ƶ���ŵ�ĳ������ķ�װ
    /// </summary>
    public class AudioLayerWarpper
    {
        private readonly List<AudioAgent> audioAgents;
        private float currentVolume = 1f;
        private bool isMuted;

        public AudioLayerWarpper(AudioLayer layer)
        {
            audioAgents = new List<AudioAgent>(8);
        }

        /// <summary>
        /// ���Ӳ�����
        /// </summary>
        public void AddAgent(AudioAgent[] agents)
        {
            if (agents == null || agents.Length == 0)
                return;

            foreach (var agent in agents)
            {
                AddAgent(agent);
            }
        }

        public void AddAgent(AudioAgent agent)
        {
            if (agent == null)
                return;

            audioAgents.Add(agent);
            ApplyCurrentSettings(agent);
            CleanupNullAgents();
        }

        public void RemoveAgent(AudioAgent agent)
        {
            if (agent == null)
                return;

            audioAgents.Remove(agent);
        }

        /// <summary>
        /// ����Ƶ����������С
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            currentVolume = Mathf.Clamp01(volume);
            foreach (var agent in audioAgents)
            {
                if (agent != null)
                {
                    agent.SetVolume(currentVolume);
                }
            }
            CleanupNullAgents();
        }
        /// <summary>
        /// ����Ƶ��������ȫ��������
        /// </summary>
        /// <param name="mute"></param>
        public void SetMute(bool mute)
        {
            isMuted = mute;
            foreach (var agent in audioAgents)
            {
                if (agent != null)
                {
                    agent.SetMute(isMuted);
                }
            }
            CleanupNullAgents();
        }

        public void ApplyCurrentSettings(AudioAgent agent)
        {
            if (agent == null)
                return;

            agent.SetVolume(currentVolume);
            agent.SetMute(isMuted);
        }

        private void CleanupNullAgents()
        {
            audioAgents.RemoveAll(a => a == null);
        }
    }
}