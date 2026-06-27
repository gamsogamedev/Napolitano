using System;
using AudioSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite clickedSprite;
    
        [Space(10)]
        [SerializeField] private UnityEvent onClickEvent;

        [Header("Interactable")]
        [Space(10)]
        [SerializeField] private bool interactable = true;
    
        private Image image;
        private bool isHovering = false;
    
        public event Action OnHoverEnter;
        public event Action OnHoverExit;
        public event Action OnStartClick;
        public event Action OnClicked;

        public bool Interactable
        {
            get => interactable;
            set
            {
                if ((interactable == value)) return;

                interactable = value;
                UpdateVisual();
            }
        }
    
        private void Awake()
        {
            image = GetComponent<Image>();
            if(idleSprite == null) idleSprite = image.sprite;
            UpdateVisual();
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;

            if(hoverSprite != null)
            {
                image.sprite = hoverSprite;
                OnHoverEnter?.Invoke();
            }
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;

            image.sprite = idleSprite;
            OnHoverExit?.Invoke();
        
            isHovering = false;
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            if (clickedSprite != null) image.sprite = clickedSprite;
            OnStartClick?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;

            if (isHovering)
            {
                if (hoverSprite != null) image.sprite = hoverSprite;
            }
            else
            {
                image.sprite = idleSprite;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;

            onClickEvent?.Invoke();
            OnClicked?.Invoke();
        }

        private void UpdateVisual()
        {
            image.sprite = idleSprite;

            image.color = interactable ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f); //new Color(1f, 1f, 1f, 0.45f)

            isHovering = false;
        }
    }
}
