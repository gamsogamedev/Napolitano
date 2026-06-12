using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace AudioSystem
{
    public class AudioSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider uiSlider;
    
        /*
     *          Exemplo de como aproveitar os diferentes grupos de audios
     *
     *          As traducoes de Decibeis para Float e vice-versa sao para
     *          fazer os sliders terem uma progressao linear de volume
     */
    
        private void Start()
        {
            audioMixer.GetFloat("MasterVolume", out float masterVolume);
            audioMixer.GetFloat("MusicVolume", out float musicVolume);
            audioMixer.GetFloat("SFXVolume", out float sfxVolume);
            audioMixer.GetFloat("UIVolume", out float uiVolume);
        
            masterSlider.value = ConversionUtils.DBToFloat(masterVolume);
            musicSlider.value = ConversionUtils.DBToFloat(musicVolume);
            sfxSlider.value = ConversionUtils.DBToFloat(sfxVolume);
            uiSlider.value = ConversionUtils.DBToFloat(uiVolume);
        }
        
        public void OnMasterChange(float volume) => audioMixer.SetFloat("MasterVolume", ConversionUtils.FloatToDB(volume));
        
        public void OnMusicChange(float volume) => audioMixer.SetFloat("MusicVolume", ConversionUtils.FloatToDB(volume));

        public void OnSFXChange(float volume) => audioMixer.SetFloat("SFXVolume", ConversionUtils.FloatToDB(volume));

        public void OnUIChange(float volume) => audioMixer.SetFloat("UIVolume", ConversionUtils.FloatToDB(volume));
    }
}
