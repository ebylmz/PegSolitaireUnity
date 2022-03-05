using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

namespace pegsolitaire {
    public class MainMenuManager : MonoBehaviour {
        public AudioMixer _audioMixer;

        public void StartGame() {
            // start the game (following two line does the same thing)
            // SceneManager.LoadScene("Scenes/GameScene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void SetVolume(float volume) {
            // _audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
            _audioMixer.SetFloat("MasterVolume", volume);
        }

        public void ExitGame() {
            Application.Quit();
        }
    }
}