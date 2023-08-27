using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public void Test()
    {
        var data = GetComponent<AudioData>();
        AudioManager.Instance.PlayAudio(data);
    }
}
