using UnityEngine;

public class NetworkUIHandler : MonoBehaviour
{
    private void OnEnable()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.OnSelfConnectionLost += ShowSelfDisconnectModal;
            SessionManager.Instance.OnTeammateConnectionLost += ShowTeammateDisconnectModal;
        }
    }

    private void OnDisable()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.OnSelfConnectionLost -= ShowSelfDisconnectModal;
            SessionManager.Instance.OnTeammateConnectionLost -= ShowTeammateDisconnectModal;
        }
    }

    private void ShowSelfDisconnectModal()
    {
        ModalManager.Instance.ShowModal(new ModalDetails
        {
            title = "Erro de Network",
            message = "Você foi desconectado da sessão.",
            confirmText = "Voltar para o menu",
            onConfirm = () => 
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        });
    }

    private void ShowTeammateDisconnectModal()
    {
        ModalManager.Instance.ShowModal(new ModalDetails
        {
            title = "Erro de Network",
            message = "Seu amigo perdeu conexão com o servidor.",
            confirmText = "Voltar para o menu",
            onConfirm = () => 
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        });
    }
}