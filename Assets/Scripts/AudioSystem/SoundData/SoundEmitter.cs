using System.Collections;
using UnityEngine;

namespace AudioSystem
{
    public class SoundEmitter : MonoBehaviour
    {
        public SoundData SoundData {private set; get;}
        
        private AudioSource audioSource;
        private Coroutine playingCoroutine;
        
        private void Awake() => audioSource = GetComponent<AudioSource>();

        public void Initialize(SoundData soundData)
        {
            SoundData = soundData;
            audioSource.outputAudioMixerGroup = soundData.audioMixerGroup;
            audioSource.clip = soundData.audioClip;
            audioSource.loop = soundData.loop;
            audioSource.pitch = soundData.pitch;

            audioSource.spatialBlend = soundData.spatialBlend ? 1f : 0f;
            audioSource.minDistance = soundData.minDistance;
            audioSource.maxDistance = soundData.maxDistance;
            audioSource.rolloffMode = soundData.rolloffMode;
        }

        public void Play()
        {
            if (playingCoroutine != null) StopCoroutine(playingCoroutine);
            
            audioSource.Play();
            if (!audioSource.loop)
            {
                playingCoroutine = StartCoroutine(WaitClipLength());
            }
        }

        public void Stop()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }
            
            audioSource.Stop();
            SoundManager.Instance.ReturnSoundEmitter(this);
        }

        public void WithRandomPitch(float minPitch = -0.05f, float maxPitch = 0.05f)
        {
            audioSource.pitch += Random.Range(minPitch, maxPitch);
        }

        private IEnumerator WaitClipLength()
        {
            yield return new WaitForSeconds(audioSource.clip.length / Mathf.Abs(audioSource.pitch));
            SoundManager.Instance.ReturnSoundEmitter(this);
        }
    }
}