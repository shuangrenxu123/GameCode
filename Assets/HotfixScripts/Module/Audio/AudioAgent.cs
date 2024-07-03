using ObjectPool;
using System;
using System.Collections;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.Events;

namespace Audio
{
    public class AudioAgent : PoolObject
    {
        private AudioSource audioSource;
        private Coroutine Coroutine;
        private Transform target;

        public UnityAction OnPlay;
        /// <summary>
        /// ��������¼�
        /// </summary>
        public UnityAction OnEnd;
        public UnityAction OnPause;
        /// <summary>
        /// �ֶ�ֹͣ�¼�
        /// </summary>
        public UnityAction OnStop;
        public override void Init()
        {
            audioSource = gameObject.AddComponent<AudioSource>();

            OnPlay = null;
            OnStop = null;
            OnPause = null;
            OnEnd = null;
        }
        public void PlayAudio(AudioClip clip, bool hasLoop)
        {
            audioSource.clip = clip;
            audioSource.loop = hasLoop;
            audioSource.time = 0f;
            audioSource.Play();
            OnPlay?.Invoke();
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
            OnStop?.Invoke();
            if (Coroutine != null)
                StopCoroutine(Coroutine);
            PoolManager.Instance.ReturnObjectToPool("audio", this);
        }
        IEnumerator FinishedPlaying(float clipLength)
        {
            yield return new WaitForSeconds(clipLength);
            //����Ժ��Լ�����
            OnEnd?.Invoke();
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