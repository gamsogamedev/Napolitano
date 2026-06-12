using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace AudioSystem
{
    public class SoundManager : MonoBehaviour
    {
        #region Singleton
            public static SoundManager Instance {get; private set;}
            private void Awake()
            {
                if (Instance != null && Instance != this)
                {
                    Destroy(this.gameObject);
                    return;
                }
                
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                InitializePool();
            }
        #endregion
        
        private IObjectPool<SoundEmitter> soundEmitterPool;
        readonly List<SoundEmitter> activeSoundEmitters = new();
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();
        
        [SerializeField] private SoundEmitter soundEmitterPrefab;
        [SerializeField] private bool collectionCheck = false;
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxPoolSize = 32;
        [SerializeField] private int maxFrequentInstances = 10;
        
        public SoundBuilder CreateSound() => new SoundBuilder(this);

        public bool CanPlaySound(SoundData soundData)
        {
            if (!soundData.frequentSound) return true;

            if (FrequentSoundEmitters.Count < maxFrequentInstances) return true;
            
            if (FrequentSoundEmitters.First != null && FrequentSoundEmitters.First.Value)
            {
                FrequentSoundEmitters.First.Value.Stop();
                return true; 
            }
            
            Debug.LogWarning("A lista de FrequentSounds esta nula, ou o valor do primeiro esta nulo");
            return false;
        }

        public SoundEmitter GetSoundEmitter()
        {
            return soundEmitterPool.Get();
        }

        public void ReturnSoundEmitter(SoundEmitter soundEmitter)
        {
            soundEmitterPool.Release(soundEmitter);
        }
        
        #region ObjectPool
            
            private void InitializePool()
                {
                    soundEmitterPool = new ObjectPool<SoundEmitter>(
                        CreateSoundEmitter,
                        OnGetSoundEmitter,
                        OnReleaseSoundEmitter,
                        OnDestroySoundEmitter,
                        collectionCheck,
                        defaultCapacity,
                        maxPoolSize
                    );
            }

            private SoundEmitter CreateSoundEmitter()
            {
                SoundEmitter soundEmitter = Instantiate(soundEmitterPrefab, transform, true);
                soundEmitter.gameObject.SetActive(false);
                return soundEmitter;
            }

            private void OnGetSoundEmitter(SoundEmitter obj)
            {
                obj.gameObject.SetActive(true);
                activeSoundEmitters.Add(obj);
            }

            private void OnReleaseSoundEmitter(SoundEmitter obj)
            {
                obj.gameObject.SetActive(false);
                activeSoundEmitters.Remove(obj);
                
                FrequentSoundEmitters.Remove(obj);
            }

            private void OnDestroySoundEmitter(SoundEmitter obj)
            {
                if (obj != null) Destroy(obj.gameObject);
            }
            
        #endregion
    }
}