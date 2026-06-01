using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    #region UIElements

        [Header("UI Elements")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private GameObject headerGameObject;
        [SerializeField] private GameObject contentGameObject;
        [SerializeField] private GameObject footerGameObject;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        

        [Header("Horizontal Layout")]
        [SerializeField] private GameObject horizontalLayoutGameObject;
        [SerializeField] private GameObject horizontalImageGameObject;
        [SerializeField] private Image horizontalImage;
        [SerializeField] private TextMeshProUGUI horizontalMessageText;
        
        [Header("Vertical Layout")]
        [SerializeField] private GameObject verticalLayoutGameObject;
        [SerializeField] private GameObject verticalImageGameObject;
        [SerializeField] private Image verticalImage;
        [SerializeField] private TextMeshProUGUI verticalMessageText;
        
        [Header("Footer")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;
        [SerializeField] private Button altButton;
        [SerializeField] private TextMeshProUGUI alternativeMessageText;

    #endregion

    #region Singleton

        public static ModalManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(gameObject);
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    #endregion
    
    //TODO: GAMBIARRA PRO NAPOLITANO, TIRAR PRA BUILD DO PACKAGE
    private bool hasButtons;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChange;
    }

    private void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        CloseModal();
    }
    
    
    public void ShowModal(ModalDetails modalDetails, bool autoCloseConfirm = true,  bool autoCloseCancel = true, bool autoCloseAlt = true)
    {
        ConfigureHeader(modalDetails.title);
        
        ConfigureLayout(modalDetails);
        
        hasButtons = false;
        
        ConfigureButton(confirmButton, confirmButtonText, modalDetails.confirmText, modalDetails.onConfirm, autoCloseConfirm);
        ConfigureButton(cancelButton, cancelButtonText, modalDetails.cancelText, modalDetails.onCancel, autoCloseCancel);
        ConfigureButton(altButton, alternativeMessageText, modalDetails.alternativeText, modalDetails.onAlternative, autoCloseAlt);
        
        footerGameObject.SetActive(hasButtons);
        
        modalPanel.SetActive(true);
    }

    private void ConfigureHeader(string titleDetails)
    {
        if (!string.IsNullOrEmpty(titleDetails))
        {
            headerGameObject.SetActive(true);
            titleText.text = titleDetails;
        }
        else
        {
            headerGameObject.SetActive(false);
        }
    }

    private void ConfigureLayout(ModalDetails modalDetails)
    {
        horizontalLayoutGameObject.SetActive(false);
        verticalLayoutGameObject.SetActive(false);

        switch (modalDetails.layout)
        {
            case ModalLayout.Horizontal:
            {
                horizontalLayoutGameObject.SetActive(true);
                horizontalMessageText.text = modalDetails.message;

                if (modalDetails.contentImage != null)
                {
                    horizontalImageGameObject.SetActive(true);
                    horizontalImage.sprite = modalDetails.contentImage;
                }
                else
                {
                    horizontalImageGameObject.SetActive(false);
                }

                break;
            }
            case ModalLayout.Vertical:
            {
                verticalLayoutGameObject.SetActive(true);
                verticalMessageText.text = modalDetails.message;

                if (modalDetails.contentImage != null)
                {
                    verticalImageGameObject.SetActive(true);
                    verticalImage.sprite = modalDetails.contentImage;
                }
                else
                {
                    verticalImageGameObject.SetActive(false);
                }
                
                if(modalDetails.message == null &&  !modalDetails.contentImage) contentGameObject.SetActive(false);

                break;
            }
        }
    }
    
    private void ConfigureButton(Button button, TextMeshProUGUI textContainer, string contentText, Action buttonAction, bool autoClose)
    {
        if (!string.IsNullOrEmpty(contentText))
        {
            button.gameObject.SetActive(true);
            textContainer.text = contentText;
            
            hasButtons = true;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                buttonAction?.Invoke();
                if(autoClose) CloseModal();
            });
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }
    
    public void CloseModal()
    {
        modalPanel.SetActive(false);
    }
}
