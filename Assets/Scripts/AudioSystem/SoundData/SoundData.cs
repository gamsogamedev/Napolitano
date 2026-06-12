using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [CreateAssetMenu(fileName = "NewSoundData", menuName = "AudioSystem/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public AudioClip audioClip;
        public AudioMixerGroup audioMixerGroup;
        
        public bool frequentSound;
        public bool loop;
        [Range(0.1f, 3f)] public float pitch = 1f;
        
        [Header("3D Settings")]
        public bool spatialBlend = false;
        public float minDistance = 1.0f;
        public float maxDistance = 20.0f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}
