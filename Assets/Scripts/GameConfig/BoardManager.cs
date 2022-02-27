using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour {
    [SerializeField] private BoardDataBase _boardDB;
    [SerializeField] private Text _boardNameText;
    [SerializeField] private Image _boardImage;
    private int _selectedBoardOption;

    private void Start() {
        if (PlayerPrefs.HasKey("_selectedBoardOption"))
            Load();
        else
            _selectedBoardOption = 0;
        UpdateBoard();
    }

    public void NextOption() {
        _selectedBoardOption = (_selectedBoardOption + 1 < _boardDB.BoardCount) ? _selectedBoardOption + 1 : 0; 
        UpdateBoard();
        Save();
    }

    public void backOption() {
        _selectedBoardOption = (_selectedBoardOption - 1 >= 0) ? _selectedBoardOption - 1 : _boardDB.BoardCount - 1; 
        UpdateBoard();
        Save();
    }

    private void UpdateBoard() {
        // retrieve the board information from DataBase and update the sprite
        Board board = _boardDB.GetBoard(_selectedBoardOption);
        _boardImage.sprite = board.boardSprite;
        _boardNameText.text = board.boardName;
    }

    private void Load() {
        // access the stored player preference
        _selectedBoardOption = PlayerPrefs.GetInt("_selectedBoardOption");
    }

    private void Save() {
        // store player preference to use another game sessions
        PlayerPrefs.SetInt("_selectedBoardOption", _selectedBoardOption);
    }

    public void ChangeScene(int sceneID) {
        SceneManager.LoadScene(sceneID);
    }
}
