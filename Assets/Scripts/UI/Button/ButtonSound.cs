using AudioSystem;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CustomButton))]
    public class ButtonSound : MonoBehaviour
    {
        private CustomButton customButton;
        
        [Header("Sounds")]
        [SerializeField] private SoundData hoverSound;
        [SerializeField] private SoundData clickSound;

        private void Awake()
        {
            customButton = GetComponent<CustomButton>();
        }

        private void OnEnable()
        {
            customButton.OnHoverEnter += OnHoverEnter;
            customButton.OnStartClick += OnStartClick;
        }

        private void OnDisable()
        {
            customButton.OnHoverEnter -= OnHoverEnter;
            customButton.OnStartClick -= OnStartClick;
        }
        
        private void OnHoverEnter()
        {
            if (hoverSound) SoundManager.Instance.CreateSound().Play(hoverSound);
        }
        
        private void OnStartClick()
        {
            if (clickSound) SoundManager.Instance.CreateSound().Play(clickSound);
        }
    }
}
