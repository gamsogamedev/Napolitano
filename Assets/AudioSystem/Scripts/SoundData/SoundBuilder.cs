using UnityEngine;

namespace AudioSystem
{
    public class SoundBuilder
    {
        private readonly SoundManager soundManager;
        
        private Vector3 soundPosition;
        private bool randomPitch;

        public SoundBuilder(SoundManager manager)
        {
            soundManager = manager;
        }

        public SoundBuilder SetPosition(Vector3 position)
        {
            soundPosition = position;
            return this;
        }

        public SoundBuilder WithRandomPitch()
        {
            randomPitch = true;
            return this;
        }

        public void Play(SoundData soundData) => PlayOnSoundEmitter(soundData);

        public SoundEmitter PlayOnSoundEmitter(SoundData soundData)
        {
            if (!soundManager.CanPlaySound(soundData)) return null;
            
            SoundEmitter soundEmitter = soundManager.GetSoundEmitter();
            
            soundEmitter.Initialize(soundData);
            soundEmitter.transform.position = soundPosition;

            if (randomPitch) soundEmitter.WithRandomPitch();
            
            if (soundData.frequentSound) 
            {
                soundManager.FrequentSoundEmitters.AddLast(soundEmitter);
            }
            
            soundEmitter.Play();
            return soundEmitter;
        }
    }
}