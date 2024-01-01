using UnityEngine;
using Random = UnityEngine.Random;

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
}

[CreateAssetMenu(menuName = ("audio"))]
class AudioClipsGroup : ScriptableObject
{
    public SequenceMode sequenceMode = SequenceMode.Sequential;
    public AudioClip[] audioClips;
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