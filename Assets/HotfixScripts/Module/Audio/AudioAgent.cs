using ObjectPool;
using System.Collections;
using UnityEngine;

namespace Audio
{
    public class AudioAgent : PoolObject
    {
        private AudioSource audioSource;
        private Coroutine Coroutine;
        private Transform target;
        public override void Init()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        public void PlayAudio(AudioClip clip, bool hasLoop)
        {
            audioSource.clip = clip;
            audioSource.loop = hasLoop;
            audioSource.time = 0f;
            audioSource.Play();
            if (!hasLoop)
                Coroutine = StartCoroutine(FinishedPlaying(audioSource.clip.length));
        }
        private void Update()
        {
            if (target != null)
                transform.position = target.position;
        }
        public void BindPosition(Transform target)
        {
            this.target = target;
        }
        /// <summary>
        /// ������ڲ��ŵ���Ƶ
        /// </summary>
        /// <returns></returns>
        public AudioClip GetClip()
        {
            return audioSource.clip;
        }
        /// <summary>
        /// ����������Ƶ
        /// </summary>
        public void Continue()
        {
            audioSource.Play();
        }
        /// <summary>
        /// ��ͣ����
        /// </summary>
        public void Pause()
        {
            audioSource.Pause();
        }
        /// <summary>
        /// ֹͣ����
        /// </summary>
        public void Stop()
        {
            audioSource.Stop();
            if (Coroutine != null)
                StopCoroutine(Coroutine);
            PoolManager.Instance.ReturnObjectToPool("audio", this);
        }
        IEnumerator FinishedPlaying(float clipLength)
        {
            yield return new WaitForSeconds(clipLength);
            //����Ժ��Լ�����
            PoolManager.Instance.ReturnObjectToPool("audio", this);
        }
        public override void Pull()
        {
            base.Pull();
            audioSource.volume = 1f;
            target = null;
        }
        public bool IsLoop()
        {
            return audioSource.loop;
        }
        public bool isPlaying()
        {
            return audioSource.isPlaying;
        }
        public void SetVolume(float v)
        {
            audioSource.volume = v * audioSource.volume;
        }
        public void SetMute(bool v)
        {
            audioSource.mute = v;
        }
    }
}