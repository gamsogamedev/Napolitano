using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IceCreamTimerUI : NetworkBehaviour {
    [SerializeField] private Image timerImage;

    private CancellationTokenSource cts;
    private bool isPaused;

    private void Awake() {
        HideLocal();
    }

    public void StartNetworkTimer(float duration) {

        Debug.Log($"StartNetworkTimer chamado. IsOwner: {IsOwner}, IsSpawned: {IsSpawned}");

        if (!IsOwner) return;

        StartTimerRpc(duration);
    }

    public void StopNetworkTimer() {
        if (!IsOwner) return;

        StopTimerRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void StartTimerRpc(float duration) {

        Debug.Log("StartTimerRpc recebido por este cliente");

        StartLocalTimer(duration);
    }

    [Rpc(SendTo.Everyone)]
    private void StopTimerRpc() {
        HideLocal();
    }

    private void StartLocalTimer(float duration) {
        CancelLocalTimer();

        cts = new CancellationTokenSource();
        RunTimer(duration, cts.Token).Forget();
    }

    private async UniTaskVoid RunTimer(float duration, CancellationToken token) {
        timerImage.gameObject.SetActive(true);
        timerImage.fillAmount = 1f;

        float elapsed = 0f;

        try {
            while (elapsed < duration) {
                if (isPaused) {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    continue;
                }

                elapsed += Time.deltaTime;

                float remaining = 1f - Mathf.Clamp01(elapsed / duration);
                timerImage.fillAmount = remaining;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            timerImage.fillAmount = 0f;
        }
        catch (OperationCanceledException) {
        }
    }

    public void HideLocal() {
        CancelLocalTimer();

        if (timerImage != null) {
            timerImage.gameObject.SetActive(false);
            timerImage.fillAmount = 1f;
        }
    }

    private void CancelLocalTimer() {
        if (cts == null) return;

        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    private void OnDestroy() {
        CancelLocalTimer();
    }
    public void Pause() {
        isPaused = true;
    }

    public void Resume() {
        isPaused = false;
    }
}