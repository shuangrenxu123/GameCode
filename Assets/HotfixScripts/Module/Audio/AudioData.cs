using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Audio
{
    /// <summary>
    /// һ����������������Ƶ���ݣ������ڶ��Groupʱ������ͬʱ����ÿ��Group������
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
    /// ��Ƶ����ģʽ
    /// </summary>
    enum SequenceMode
    {
        /// <summary>
        /// �������
        /// </summary>
        Random,
        /// <summary>
        /// ���ظ����
        /// </summary>
        RandomNoImmediateRepeat,
        /// <summary>
        /// ˳�򲥷�
        /// </summary>
        Sequential,
    }
}