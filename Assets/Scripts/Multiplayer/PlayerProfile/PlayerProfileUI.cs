using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro; //biblioteca que substitui o texto padrão
using Unity.VisualScripting;
using UnityEditor;
using System.Runtime.CompilerServices;

//Controla o painel de perfil do jogador no lobby.
public class PlayerProfileUI : MonoBehaviour
{
    //----------------------------------------------------------------
    //Elementos do Canvas UI
    //----------------------------------------------------------------
    [Header("Painel")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject lobbyPanel; //panel da sala
    [SerializeField] private Button openProfileButton;
    [SerializeField] private Button close;

    [Header("Vizualização do personagem")]
    [SerializeField] private Image characterPreview;

    [Header("Nome")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("Skins")]
    [SerializeField] private Transform skinGridParent;
    [SerializeField] private SkinSlotUI skinSlotPrefab;

    [Header("Confirmar")]
    [SerializeField] private Button confirm;
    [SerializeField] private TextMeshProUGUI feedbackText;

    //----------------------------------------------------------------
    //Variáveis
    //----------------------------------------------------------------
    //Referêncial ao jogador local.
    private PlayerProfileDados DadosPlayerLocal;

    //Skin desejada. Ainda não confirmada.
    private int pendingSkinIndex = -1;

    private List<SkinSlotUI> skinSlots = new List<SkinSlotUI>();

    //----------------------------------------------------------------
    //Funções
    //----------------------------------------------------------------

    //Roda antes de tudo. Esconde o painel.
    //AddListener registra as funções que são chamadas quando os botões forem clicados.
    private void Awake()
    {
        profilePanel.SetActive(false);
        openProfileButton.onClick.AddListener(OpenPanel);
        confirm.onClick.AddListener(OnConfirmClicked);
        close.onClick.AddListener(Closepanel);
    }

    //Roda assim queo painel é aberto.
    //Coroutine é função que pode pausar no meio e continuar depois sem travar o jogo.
    private void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    //Função de inicialização dos dados do player.
    //Loop para encontrar o spawn do objeto do jogador na rede, por isso tenta a cada 0.2s até encontrar DadosPlayerProfile do player local.
    //OnProfileChanged para atualização geral. OnSkinConfirmationReceived para resultado do servidor.
    private IEnumerator WaitForLocalPlayer()
    {
        while (DadosPlayerLocal == null)
        {
            foreach(var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
               if(client.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    var profile = client.PlayerObject?.GetComponent<PlayerProfileDados>();
                    if(profile != null && profile.IsOwner)
                    {
                        DadosPlayerLocal = profile;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        InitializeSkinsSlots();
        RefreshUI();

        DadosPlayerLocal.OnProfileChanged += RefreshUI;
        DadosPlayerLocal.OnSkinConfirmationReceived += OnSkinConfirmationReceived;
    }

    //Cria slots dinamicamente para as skins, cria cópia do prefab como filho do skinGridParent.
    private void InitializeSkinsSlots()
    {
        foreach (var slot in skinSlots)
            if (slot != null) Destroy(slot.gameObject);
        skinSlots.Clear();

        int total = SkinManager.Instance.GetTotalSkins();

        for(int i = 0; i < total; i++)
        {
            int index = i;
            SkinSlotUI slot = Instantiate(skinSlotPrefab, skinGridParent);
            slot.Setup(
                skinSprite: SkinManager.Instance.GetSkinSprite(index),
                onSelected: () => OnSkinSlotSelected(index)
            );
            skinSlots.Add(slot);
        }
    }

    //Ativação do painel
    private void OpenPanel()
    {
        lobbyPanel.SetActive(false);
        profilePanel.SetActive(true);

        pendingSkinIndex = DadosPlayerLocal.SkinIndex.Value;
        nameInput.text = DadosPlayerLocal.PlayerName.Value.ToString();
        feedbackText.text = "";
        confirm.interactable = true;

        Sprite currentSprite = SkinManager.Instance.GetSkinSprite(pendingSkinIndex);
        if (currentSprite != null && characterPreview != null)
            characterPreview.sprite = currentSprite;

        RefreshSkinSlot();
    }

    //Fecha painel.
    public void Closepanel()
    {
        profilePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    //Jogador seleciona skin e vÊ no preview omo ficaria, antes de confirmar.
    private void OnSkinSlotSelected(int skinIndex)
    {
        pendingSkinIndex = skinIndex;
        feedbackText.text = "";

        Sprite previewSprite = SkinManager.Instance.GetSkinSprite(skinIndex);
        if (previewSprite != null && characterPreview != null)
            characterPreview.sprite = previewSprite;

        if (skinIndex != DadosPlayerLocal.SkinIndex.Value)
            DadosPlayerLocal.SetReady(false);

        RefreshSkinSlot();
    }

    //Botão confirmar clicado. Mudança dos dados do player profile, nome vai direto, mas skin é requerida.
    private void OnConfirmClicked()
    {
        DadosPlayerLocal.ApplyName(nameInput.text);

        if(pendingSkinIndex != DadosPlayerLocal.SkinIndex.Value)
        {
            confirm.interactable = false;
            feedbackText.text = "Aguardando...";
            feedbackText.color = Color.white;
            DadosPlayerLocal.RequestSkin(pendingSkinIndex);
        }
        else
        {
            feedbackText.text = "Perfil atualizado";
            feedbackText.color = Color.green;
        }
    }

    //Confirmação da mudança ou não da skin. Se aprovado já faz mudança. As bordas tambem são modificadas paar estado atual.
    private void OnSkinConfirmationReceived(bool approved, int skinIndex)
    {
        confirm.interactable = true;

        if (approved)
        {
            feedbackText.text = "Perfil atualizado";
            feedbackText.color = Color.green;
        }
        else
        {
            pendingSkinIndex = DadosPlayerLocal.SkinIndex.Value;

            Sprite currentSprite = SkinManager.Instance.GetSkinSprite(pendingSkinIndex);
            if(currentSprite != null && characterPreview != null)
            {
                characterPreview.sprite = currentSprite;
            }
            feedbackText.text = "Essa skin já está em uso";
            feedbackText.color = Color.red;
        }

        RefreshSkinSlot();
    }

    //Atualização do UI em caso de mudança.
    private void RefreshUI()
    {
        if (DadosPlayerLocal == null) return;
        if (!profilePanel.activeSelf) return;

        nameInput.text = DadosPlayerLocal.PlayerName.Value.ToString();

        Sprite currentSprite = SkinManager.Instance.GetSkinSprite(DadosPlayerLocal.SkinIndex.Value);

        if (currentSprite != null && characterPreview != null)
            characterPreview.sprite = currentSprite;

        RefreshSkinSlot();
    }

    //Atualização dos estados de ocupação dos slots em tempo real em caso de mudança.
    private void RefreshSkinSlot()
    {
        for(int i = 0; i < skinSlots.Count; i++)
        {
            bool isSelected = (i == pendingSkinIndex);
            bool isTakenByOther = SkinManager.Instance.IsSkinTaken(i, NetworkManager.Singleton.LocalClientId);
            skinSlots[i].UpdateState(isSelected, isTakenByOther);
        }
    }

    //Dois eventos destruídos quando a UI é destruída.
    private void OnDestroy()
    {
        if(DadosPlayerLocal != null)
        {
            DadosPlayerLocal.OnProfileChanged -= RefreshUI;
            DadosPlayerLocal.OnSkinConfirmationReceived -= OnSkinConfirmationReceived;
        }
    }
}


