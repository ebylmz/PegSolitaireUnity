using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pegsolitaire {
    public class MainMenuManager : MonoBehaviour {
        public void StartGame() {
            // start the game (following two line does the same thing)
            // SceneManager.LoadScene("Scenes/GameScene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void ExitGame() {
            Debug.Log("Exit Application");
            Application.Quit();
        }
    }
}