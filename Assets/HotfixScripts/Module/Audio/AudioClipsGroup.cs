using UnityEngine;
using Utility;

namespace Audio
{
    /// <summary>
    /// һ����Ƶ��
    /// </summary>
    [CreateAssetMenu(menuName = ("audio"))]
    class AudioClipsGroup : ScriptableObject
    {
        public SequenceMode sequenceMode = SequenceMode.Sequential;
        [SerializeField]
        private AudioClip[] audioClips;
        private int nextClipToPlay = -1;
        public AudioClip GetNextClip()
        {
            if (audioClips.Length == 1)
                return audioClips[0];
            if (sequenceMode == SequenceMode.RandomNoImmediateRepeat)
            {
                Probability.Shuffle<AudioClip>(ref audioClips);
            }
            //�����ֹһ����Ƶ���Ǿ��ж�֮ǰ�Ƿ񲥷Ź�
            switch (sequenceMode)
            {
                case SequenceMode.Random:
                    nextClipToPlay = Random.Range(0, audioClips.Length);
                    break;
                case SequenceMode.RandomNoImmediateRepeat:

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
                if (clip.name == name)
                    return clip;
            }
            return null;
        }
    }
}