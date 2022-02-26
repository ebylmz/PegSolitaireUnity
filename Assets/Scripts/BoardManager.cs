using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private BoardDataBase _boardDB;
    [SerializeField] private Text _boardNameText;
    [SerializeField] private Image _boardImage;
    // [SerializeField] private SpriteRenderer _artworkSprite;
    // [SerializeField] private CanvasRenderer _artworkSprite;
    private int _selectedOption;

    private void Start() {
        if (PlayerPrefs.HasKey("_selectedOption"))
            Load();
        else
            _selectedOption = 0;
        UpdateBoard();
    }

    public void NextOption() {
        _selectedOption = (_selectedOption + 1 < _boardDB.BoardCount) ? _selectedOption + 1 : 0; 
        UpdateBoard();
        Save();
    }

    public void backOption() {
        _selectedOption = (_selectedOption - 1 >= 0) ? _selectedOption - 1 : _boardDB.BoardCount - 1; 
        UpdateBoard();
        Save();
    }

    private void UpdateBoard() {
        // retrieve the board information from DataBase and update the sprite
        Board board = _boardDB.GetBoard(_selectedOption);
        _boardImage.sprite = board.boardSprite;
        _boardNameText.text = board.boardName;
    }

    private void Load() {
        // access the stored player preference
        _selectedOption = PlayerPrefs.GetInt("_selectedOption");
    }

    private void Save() {
        // store player preference to use another game sessions
        PlayerPrefs.SetInt("_selectedOption", _selectedOption);
    }

    public void ChangeScene(int sceneID) {
        SceneManager.LoadScene(sceneID);
    }
}
