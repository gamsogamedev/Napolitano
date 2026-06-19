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

        [Header("Sounds")]
        [SerializeField] private SoundData hoverSound;
        [SerializeField] private SoundData clickSound;
    
        [Space(10)]
        [SerializeField] private UnityEvent onClickEvent;
    
        private Image image;
        private bool isHovering = false;
    
        public event Action OnHoverEnter;
        public event Action OnHoverExit;
        //private ITextButton textButton;
    
        private void Awake()
        {
            image = GetComponent<Image>();
            if(idleSprite == null) idleSprite = image.sprite;
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if(hoverSound != null) SoundManager.Instance.CreateSound().Play(hoverSound);
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
            if(clickSound != null) SoundManager.Instance.CreateSound().Play(clickSound);
            if(clickedSprite != null) image.sprite = clickedSprite;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isHovering && hoverSprite)
            {
                image.sprite = hoverSprite;
                OnHoverEnter?.Invoke();
            }
            else
            {
                image.sprite = idleSprite;
                OnHoverExit?.Invoke();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClickEvent?.Invoke();
        }
    }
}
