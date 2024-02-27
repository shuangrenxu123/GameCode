using System.Collections.Generic;


namespace Audio
{


    /// <summary>
    /// ��Ƶ���ŵ�ĳ������ķ�װ
    /// </summary>
    public class AudioLayerWarpper
    {
        private AudioLayer audioLayer;
        private List<AudioAgent> audioAgents;

        public AudioLayerWarpper(AudioLayer layer)
        {
            audioAgents = new List<AudioAgent>();
            audioLayer = layer;
        }

        /// <summary>
        /// ��Ӳ�����
        /// </summary>
        public void AddAgent(AudioAgent[] agent)
        {
            audioAgents.AddRange(agent);
        }
        public void AddAgent(AudioAgent agent)
        {
            audioAgents.Add(agent);
        }
        /// <summary>
        /// ����Ƶ����������С
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            foreach (AudioAgent agent in audioAgents)
            {
                agent.SetVolume(volume);
            }
        }
        /// <summary>
        /// ����Ƶ��������ȫ��������
        /// </summary>
        /// <param name="mute"></param>
        public void SetMute(bool mute)
        {
            foreach (AudioAgent agent in audioAgents)
            {
                agent.SetMute(mute);
            }
        }
    }
}