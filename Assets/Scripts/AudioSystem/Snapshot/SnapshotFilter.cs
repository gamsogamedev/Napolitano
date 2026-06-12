using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public class SnapshotFilter : MonoBehaviour
    {
        [SerializeField] private AudioMixer mainMixer;
        private const string DefaultSnapshot = "Default";
        private const string MuffledSnapshot = "Muffled";

        private AudioMixerSnapshot defaultSnapshot;
        private AudioMixerSnapshot muffledSnapshot;

        private void Start()
        {
            defaultSnapshot = mainMixer.FindSnapshot(DefaultSnapshot);
            muffledSnapshot = mainMixer.FindSnapshot(MuffledSnapshot);
        }

        private void OnEnable()
        {
            SnapshotActions.SetDefaultFilter += SetDefaultFilter;
            SnapshotActions.SetMuffledFilter += SetMuffledFilter;
        }

        private void OnDisable()
        {
            SnapshotActions.SetDefaultFilter -= SetDefaultFilter;
            SnapshotActions.SetMuffledFilter -= SetMuffledFilter;
        }

        private void SetDefaultFilter() => defaultSnapshot.TransitionTo(0.5f);
        private void SetMuffledFilter() => muffledSnapshot.TransitionTo(0.5f);
    }
}
