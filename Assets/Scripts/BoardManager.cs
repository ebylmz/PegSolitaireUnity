using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private BoardDataBase _boardDB;
    [SerializeField] private Text _boardNameText;
    [SerializeField] private Image _boardImage;
    // [SerializeField] private SpriteRenderer _artworkSprite;
    // [SerializeField] private CanvasRenderer _artworkSprite;
    private int _selectedOption;

    private void Start() {
        _selectedOption = 0;
        UpdateBoard();
    }

    public void NextOption() {
        _selectedOption = (_selectedOption + 1 < _boardDB.BoardCount) ? _selectedOption + 1 : 0; 
        UpdateBoard();
    }

    public void backOption() {
        _selectedOption = (_selectedOption - 1 >= 0) ? _selectedOption - 1 : _boardDB.BoardCount - 1; 
        UpdateBoard();
    }

    private void UpdateBoard() {
        // retrieve the board information from DataBase and update the sprite
        Board board = _boardDB.GetBoard(_selectedOption);
        _boardImage.sprite = board.boardSprite;
        _boardNameText.text = board.boardName;
    }
}
