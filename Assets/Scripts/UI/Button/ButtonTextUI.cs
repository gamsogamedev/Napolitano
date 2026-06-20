using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CustomButton))]
    public class ButtonTextUI: MonoBehaviour
    { 
        private CustomButton customButton;
        [SerializeField] private TextMeshProUGUI text;

        private void Awake()
        {
            customButton = GetComponent<CustomButton>();
        }

        private void OnEnable()
        {
            customButton.OnHoverEnter += OnHoverEnter;
            customButton.OnHoverExit += OnHoverExit;
            customButton.onClickEvent.AddListener(OnHoverExit);
        }

        private void OnDisable()
        {
            customButton.OnHoverEnter -= OnHoverEnter;
            customButton.OnHoverExit -= OnHoverExit;
        }
        
        public void OnHoverEnter()
        {
            text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, 0);
        }

        public void OnHoverExit()
        {
            text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, 0.5f);
        }
    }
}