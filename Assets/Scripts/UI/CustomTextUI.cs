using TMPro;
using UnityEngine;

namespace UI
{
    public class CustomTextUI: MonoBehaviour, ITextButton
    {
        [SerializeField] private TextMeshProUGUI text;
        
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