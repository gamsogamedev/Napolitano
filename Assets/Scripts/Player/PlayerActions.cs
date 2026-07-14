using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class PlayerActions : MonoBehaviour {
    [Header("Referencias")]
    private PlayerController playerController;
    private PlayerCollision playerCollision;

    [Header("Componentes de Gameplay")]
    [SerializeField] private Behaviour[] gameplayComponents;

    private SpriteRenderer[] spriteRenderers;
    private TextMeshPro playerName;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
        playerCollision = GetComponent<PlayerCollision>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        playerName = GetComponentInChildren<TextMeshPro>(true);
    }


    /// Desabilita toda a jogabilidade do jogador.
    public void DisableGameplay() {
        // Desabilita todos os componentes configurados no Inspector
        foreach (Behaviour component in gameplayComponents) {
            if (component != null)
                component.enabled = false;
        }

        // Para imediatamente o movimento
        if (playerController != null) {
            playerController.Rb.linearVelocity = Vector2.zero;
            playerController.Rb.angularVelocity = 0f;

            // Caso o jogador ainda esteja segurando uma colher,
            // desabilita o script dela para impedir que continue
            // seguindo o mouse.
            if (playerController.CarriedSpoon != null) {
                playerController.CarriedSpoon.enabled = false;
            }
        }

        var timerUI = GetComponentInChildren<IceCreamTimerUI>();

        if (timerUI != null)
            timerUI.Pause();
    }

    /// <summary>
    /// Reabilita toda a jogabilidade do jogador.
    /// Utilizado ao voltar de uma pausa.
    /// </summary>
    public void EnableGameplay() {
        // Reabilita todos os componentes configurados no Inspector
        foreach (Behaviour component in gameplayComponents) {
            if (component != null)
                component.enabled = true;

            Debug.Log($"Desabilitando: {component.GetType().Name}");
        }

        // Reabilita a colher caso ela exista
        if (playerController != null && playerController.CarriedSpoon != null) {
            playerController.CarriedSpoon.enabled = true;
        }

        var timerUI = GetComponentInChildren<IceCreamTimerUI>();

        if (timerUI != null)
            timerUI.Resume();
    }


    public void DisablePlayer(bool hidePlayer) {
        DisableGameplay();

        playerController.enabled = false;
        playerCollision.enabled = false;

        playerController.Rb.linearVelocity = Vector2.zero;
        playerController.Rb.angularVelocity = 0f;

        if (playerController.CarriedSpoon != null)
            playerController.CarriedSpoon.enabled = false;

        if (hidePlayer) {
            foreach (var sprite in spriteRenderers)
                sprite.enabled = false;

            if (playerName != null)
                playerName.enabled = false;

            var timerUI = GetComponentInChildren<IceCreamTimerUI>(true);
            if (timerUI != null)
                timerUI.HideLocal();
        }
    }
}