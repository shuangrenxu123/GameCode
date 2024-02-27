using System.Collections.Generic;


namespace Audio
{


    /// <summary>
    /// 音频播放的某个轨道的封装
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
        /// 添加播放器
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
        /// 设置频道的音量大小
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
        /// 将该频道的音乐全都给静音
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