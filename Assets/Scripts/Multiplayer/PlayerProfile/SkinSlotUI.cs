using UnityEngine;
using UnityEngine.UI;

//Quadradinho de skin na grade do painel de perfil
//Estrutura do prefab:
//  Skinslot (Image - fundo/borda) [esse script aqui]
//      SkinPreview (Image - mostra o sprite da skin)
//      LockOverlay (Image - overlay escuro, ativa quando bloqueada)
//      Button (componente no onjteo raiz ou filho)
public class SkinSlotUI : MonoBehaviour
{
    [Header("Referências do Prefab")]
    [SerializeField] private Image skinPreviewImage; //Sprite da skin.
    [SerializeField] private Image borderImage;      // Borda que muda de cor por estado.
    [SerializeField] private Image lockOverlay;     //Overlay quando bloqueado por outro jogador.
    [SerializeField] private Button butao;


    [Header("Cores do Estado")]
    [SerializeField] private Color normalBorderColor = Color.white;
    [SerializeField] private Color selectedBorderColor = Color.yellow;
    //[SerializeField] private Color currentBorderColor = Color.green;
    [SerializeField] private Color takenBorderColor = Color.red;

    private System.Action OnSelected;

    //Chamado pelo PlayerProfileUI ao criar o slot.
    public void Setup(Sprite skinSprite, System.Action onSelected) //SystemAction é um tipo para armazenar uma referência para uma função que será chamada (callback) depois.
    {
        this.OnSelected = onSelected;
        butao.onClick.RemoveAllListeners(); //evita memory leak
        butao.onClick.AddListener(() => this.OnSelected?.Invoke());

        if (skinSprite != null && skinPreviewImage != null)
            skinPreviewImage.sprite = skinSprite;

        UpdateState(isSelected: false, isTakenByOther: false);
    }

    //Atualiza a aparÊncia visual do slot conforme o estado. Chamado toda vez que qualquer jogador muda de skin.
    public void UpdateState(bool isSelected,  bool isTakenByOther)
    {
        if (isTakenByOther)
        {
            borderImage.color = takenBorderColor;
            lockOverlay.gameObject.SetActive(true);
            butao.interactable = false;
            return;
        }

        lockOverlay.gameObject.SetActive(false);
        butao.interactable = true;
        borderImage.color = isSelected ? selectedBorderColor : normalBorderColor;
    }
}
