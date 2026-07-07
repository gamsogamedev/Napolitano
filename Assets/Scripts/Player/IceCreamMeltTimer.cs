using UnityEngine;

[RequireComponent(typeof(PlayerCollision))]
public class IceCreamMeltTimer : MonoBehaviour {
    [SerializeField] private float meltDuration = 12f;

    private float timer;
    private bool isRunning;
    private bool hasMelted;

    private PlayerCollision playerCollision;
    private IceCreamTimerUI timerUI;

    private void Awake() {
        playerCollision = GetComponent<PlayerCollision>();
        timerUI = GetComponentInChildren<IceCreamTimerUI>(true);
    }

    private void Update() {
        if (!isRunning || hasMelted)
            return;

        timer += Time.deltaTime;

        if (timer >= meltDuration) {
            hasMelted = true;
            isRunning = false;

            timerUI?.StopNetworkTimer();
            playerCollision.IceCream_Melted();
        }
        Debug.Log("Timer Update");
    }

    public void StartTimer() {
        timer = 0f;
        hasMelted = false;
        isRunning = true;

        timerUI?.StartNetworkTimer(meltDuration);
    }

    public void StopTimer() {
        timer = 0f;
        hasMelted = false;
        isRunning = false;

        timerUI?.StopNetworkTimer();
    }

    public float RemainingTime => Mathf.Max(0f, meltDuration - timer);
}