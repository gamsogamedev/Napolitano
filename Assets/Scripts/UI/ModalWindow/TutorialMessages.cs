using UnityEngine;
using UnityEngine.UI;

public class TutorialMessages : MonoBehaviour
{
    [SerializeField] private bool firstTutorial;

    [SerializeField] private Sprite moveImage;
    [SerializeField] private Sprite spoonImage;
    [SerializeField] private Sprite meltImage;
    

    private ModalDetails movementModal;
    
    private ModalDetails meltModal;
    
    private ModalDetails spoonModal;
    
    private void Start()
    {
        SetUpModals();

        if (firstTutorial) ShowFirstTutorial();
        else ShowSecondTutorial();
    }

    private void SetUpModals()
    {
        movementModal = new ModalDetails
        {
            title = "Movimentacao",
            message = "(A-D) movimentam o personagem.\n" +
                      "Use (espaco) para pular como um sorvete e para pular fora da casquinha ou colher!\n" +
                      "Interaja com objetos apertando (E)",
            contentImage = moveImage,
            layout = ModalLayout.Vertical,
        
            confirmText = "Proximo",
            onConfirm = () => {ModalManager.Instance.ShowModal(meltModal, autoCloseConfirm:false);}
        };
        
        meltModal = new ModalDetails
        {
            title = "Derretimento do sorvete",
            message = "Tome cuidado em ficar muito tempo fora de uma casquinha ou colher, o sorvete derrete bem rapido!",
            contentImage = meltImage,
            layout = ModalLayout.Vertical,
        
            confirmText = "Voltar",
            onConfirm = () => {ModalManager.Instance.ShowModal(movementModal, autoCloseConfirm:false);},
            cancelText =  "Fechar",
            onCancel = null
        };

        spoonModal = new ModalDetails
        {
            title = "Uso da colher",
            message = "Entre na colher de seu amigo (apertando E) e pule no momento certo enquanto a colher esta em movimento",
            contentImage = spoonImage,
            layout = ModalLayout.Vertical,

            confirmText = "Fechar",
            onConfirm = null
        };
    }
    
    private void ShowFirstTutorial()
    {
        ModalManager.Instance.ShowModal(movementModal, autoCloseConfirm:false);
    }
    
    private void ShowSecondTutorial()
    {
        ModalManager.Instance.ShowModal(spoonModal);
    }
}
