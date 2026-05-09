using PurrNet;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayerDied() {
        if (isServer) {
            RestartLevel();
        }
    }

    [ObserversRpc]
    private void RestartLevel() {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }


}
