using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils;

public class ModalManager : Singleton<ModalManager>
{
    [Header("UI Elements")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    
    private void Start()
    {
        modalPanel.SetActive(false);
    }

    public void ShowModal(ModalDetails modalDetails)
    {
        titleText.text = modalDetails.titleText;
        messageText.text = modalDetails.messageText;
        
        confirmButton.onClick.RemoveAllListeners();
        confirmButtonText.text = modalDetails.confirmText;
        confirmButton.onClick.AddListener(() =>
        {
            modalDetails.onConfirm?.Invoke();
            CloseModal();
        });

        if (!string.IsNullOrEmpty(modalDetails.cancelText))
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButtonText.text = modalDetails.cancelText;
            cancelButton.onClick.AddListener(() =>
                {
                    modalDetails.onCancel?.Invoke();
                    CloseModal();
                }
            );
        }
        else
        {
            cancelButton.gameObject.SetActive(false);
        }
        
        modalPanel.SetActive(true);
    }

    //Funcao meio inutil agr, mas depois vamos ter audio
    private void CloseModal()
    {
        modalPanel.SetActive(false);
    }
}
