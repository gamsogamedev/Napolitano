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
    
        private Image image;
        private bool isHovering = false;
    
        public event Action OnHoverEnter;
        public event Action OnHoverExit;
        public event Action OnStartClick;
    
        private void Awake()
        {
            image = GetComponent<Image>();
            if(idleSprite == null) idleSprite = image.sprite;
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(hoverSprite != null)
            {
                image.sprite = hoverSprite;
                OnHoverEnter?.Invoke();
            }
            isHovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.sprite = idleSprite;
            OnHoverExit?.Invoke();
        
            isHovering = false;
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            if(clickedSprite != null) image.sprite = clickedSprite;
            OnStartClick?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
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
            onClickEvent?.Invoke();
        }
    }
}
