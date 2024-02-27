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
    /// һ����Ƶ��
    /// </summary>
    [CreateAssetMenu(menuName = ("audio"))]
    class AudioClipsGroup : ScriptableObject
    {
        public SequenceMode sequenceMode = SequenceMode.Sequential;
        [SerializeField]
        private AudioClip[] audioClips;
        //====private======
        private AudioClip[] randomAudioClips;
        private int nextClipToPlay = -1;
        public AudioClip GetNextClip()
        {
            if (audioClips.Length == 1)
                return audioClips[0];
            //�����ֹһ����Ƶ���Ǿ��ж�֮ǰ�Ƿ񲥷Ź�
            switch (sequenceMode)
            {
                case SequenceMode.Random:
                    nextClipToPlay = Random.Range(0, audioClips.Length);
                    break;
                case SequenceMode.Sequential:
                    if (nextClipToPlay == -1)
                        nextClipToPlay = 0;
                    else
                        nextClipToPlay = (int)Mathf.Repeat(++nextClipToPlay, audioClips.Length);
                    break;
            }

            return audioClips[nextClipToPlay];
        }

        public AudioClip GetClip(string name)
        {
            foreach (var clip in audioClips)
            {
                if(clip.name == name) 
                    return clip;  
            }
            return null;
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
        /// ����ȫ�������
        /// </summary>
        //RandomNoImmediateRepeat,
        /// <summary>
        /// ˳�򲥷�
        /// </summary>
        Sequential,
    }
}