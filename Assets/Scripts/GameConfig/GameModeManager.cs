using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private GameModeDataBase _gameModeDB;
    [SerializeField] private Text _gameModeText;
    [SerializeField] private Image _gameModeImage;
    private int _selectedGameModeOption;

    private void Start() {
        if (PlayerPrefs.HasKey("_selectedGameModeOption"))
            Load();
        else
            _selectedGameModeOption = 0;
        UpdateBoard();
    }

    public void NextOption() {
        _selectedGameModeOption = (_selectedGameModeOption + 1 < _gameModeDB.GameModeCount) ? _selectedGameModeOption + 1 : 0; 
        UpdateBoard();
        Save();
    }

    public void backOption() {
        _selectedGameModeOption = (_selectedGameModeOption - 1 >= 0) ? _selectedGameModeOption - 1 : _gameModeDB.GameModeCount - 1; 
        UpdateBoard();
        Save();
    }

    private void UpdateBoard() {
        // retrieve the board information from DataBase and update the sprite
        GameMode gmode = _gameModeDB.GetGameMode(_selectedGameModeOption);
        _gameModeImage.sprite = gmode.gameModeSprite;
        _gameModeText.text = gmode.gameModeName;
    }

    private void Load() {
        // access the stored player preference
        _selectedGameModeOption = PlayerPrefs.GetInt("_selectedGameModeOption");
    }

    private void Save() {
        // store player preference to use another game sessions
        PlayerPrefs.SetInt("_selectedGameModeOption", _selectedGameModeOption);
    }

    public void ChangeScene(int sceneID) {
        SceneManager.LoadScene(sceneID);
    }
}
