using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class InGamePause : MonoBehaviour
    {
        [SerializeField] private List<GameObject> ownerOnlyGameObjects;

        [Space(5)]
        [SerializeField] private GameObject pauseHUD;
        [SerializeField] private GameObject pausePanel;

        [Space(5)]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject settingsMenu;

        private void Start()
        {
            if (!NetworkManager.Singleton.LocalClient.IsSessionOwner) RemoveOwnerOnlyObjects();
        }

        private void RemoveOwnerOnlyObjects()
        {
            foreach (GameObject ownerOnlyGameObject in ownerOnlyGameObjects)
            {
                ownerOnlyGameObject.SetActive(false);
            }
        }

        public void OpenPauseMenu()
        {
            pausePanel.SetActive(true);
            
            pauseMenu.SetActive(true);
            settingsMenu.SetActive(false);
            pauseHUD.SetActive(false);
        }

        public void Resume()
        {
            pausePanel.SetActive(false);
            
            pauseMenu.SetActive(false);
            pauseHUD.SetActive(true);
        }

        public void OpenSettingsMenu()
        {
            pauseMenu.SetActive(false);
            settingsMenu.SetActive(true);
        }

        public void Restart()
        {
            if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
                NetworkManager.Singleton.SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        public void Quit()
        {
            if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
            {
                ModalManager.Instance.ShowModal(new ModalDetails
                {
                    message = "Deseja voltar para a seleção de mapas? Você perderá todo o progresso da fase.",
                    confirmText = "Voltar para menu",
                    onConfirm = () =>
                    {
                        NetworkManager.Singleton.SceneManager.LoadScene("LevelSelector", LoadSceneMode.Single);
                    },
                    cancelText = "Cancelar",
                    onCancel =  Resume
                });
            }
            else
            {
                ModalManager.Instance.ShowModal(new ModalDetails
                {
                    title = "Desconectar da sessão",
                    message = "Apenas o host pode voltar para a seleção de mapas. Deseja se desconectar da sessão multiplayer atual?",
                    confirmText = "Desconectar",
                    onConfirm = DisconnectClient,
                    cancelText = "Cancelar",
                    onCancel = Resume
                });
            }
        }

        private static async void DisconnectClient()
        {
            try
            {
                await SessionManager.Instance.LeaveSession();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
