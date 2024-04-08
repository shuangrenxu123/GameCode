using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Audio
{
    /// <summary>
    /// 一个物体所包含的音频数据，当存在多个Group时，他会同时播放每个Group的内容
    /// </summary>
    public class AudioData : MonoBehaviour
    {
        [SerializeField]
        AudioClipsGroup[] clipGroups;
        public bool looping = false;
        public AudioLayer layer;
        public AudioClip[] GetClips()
        {
            int numberOfClips = clipGroups.Length;
            AudioClip[] resultingClips = new AudioClip[numberOfClips];

            for (int i = 0; i < numberOfClips; i++)
            {
                resultingClips[i] = clipGroups[i].GetNextClip();
            }

            return resultingClips;
        }
        public AudioClip[] GetClips(string name)
        {
            List<AudioClip> clips = new List<AudioClip>();
            foreach (var clipgroup in clipGroups)
            {
                var clip = clipgroup.GetClip(name);
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }
            return clips.ToArray();
        }
        public AudioClip GetClip(string name)
        {
            return clipGroups[0].GetClip(name);
        }
    }


   
    /// <summary>
    /// 音频播放模式
    /// </summary>
    enum SequenceMode
    {
        /// <summary>
        /// 随机播放
        /// </summary>
        Random,
        /// <summary>
        /// 不重复随机
        /// </summary>
        RandomNoImmediateRepeat,
        /// <summary>
        /// 顺序播放
        /// </summary>
        Sequential,
    }
}