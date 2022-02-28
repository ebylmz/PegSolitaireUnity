using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace pegsolitaire {
    public class GameManager : MonoBehaviour {
        public enum BoardType {FRENCH, GERMAN, ASYMETRICAL, ENGLISH, DIAMOND, TRIANGULAR, SQUARE}
        public enum GameMode {USER, COMPUTER}

        [SerializeField] private BoardDataBase _boardDB;
        [SerializeField] private Image _gameOverPanel;
        [SerializeField] private GameObject _gameRulesPanel;
        [SerializeField] private Cell _cellPrefab; 
        [SerializeField] private Camera _camera;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _loserSound;
        [SerializeField] private AudioClip _winnerSound;
        [SerializeField] private AudioClip _wrongMovSound;
        [SerializeField] private TextMeshProUGUI _textNumMov;
        [SerializeField] private TextMeshProUGUI _textNumPeg;
        [SerializeField] private Button _undoBtn; 
        [SerializeField] private Button _nextMoveBtn; 
        private GameMode _gameMode;
        private BoardType _gameBoardType; 
        private int _width; // width of the game board 
        private int _height; // height of game board
        private Dictionary<Vector2Int, Cell> _cells;
        private Stack<Movement> _allMov; // keeps all the movement made so far
        private List<Cell> _possibleEndCells; // to show the player end positions for their movement
        private Cell _startCell;    // keeps the start position of the movement
        private int _numMov; 
        private int _numPeg; 

        void Awake() {
            _cells = new Dictionary<Vector2Int, Cell>();
            _startCell = null;
            _allMov = new Stack<Movement>();
            _possibleEndCells = new List<Cell>();
            
            // access the stored player preference
            _gameMode = (PlayerPrefs.HasKey("_selectedGameModeOption")) ?
                (GameMode) PlayerPrefs.GetInt("_selectedGameModeOption") : 0;
            _gameBoardType = (PlayerPrefs.HasKey("_selectedBoardOption")) ? 
                (BoardType) PlayerPrefs.GetInt("_selectedBoardOption") : 0;
        }

        void Start() {
            // create the game board
            LoadGame();

            // change the position of the camera as shows center of the game board
            _camera.transform.position = new Vector3((float) _width / 2 - 0.5f, (float) _height / 2 , -10);

            // there is no movement made yet
            _undoBtn.gameObject.SetActive(false);
            
            // start computer game  
            if (_gameMode == GameMode.COMPUTER) {
                _nextMoveBtn.gameObject.SetActive(false);
                StartCoroutine(PlayAuto());
            }
        }

        void Update() {
            // user cannot make movement in ComputerPlay mode
            // do not get movement mouse if another panel is active
            if (_gameMode == GameMode.USER && !_gameRulesPanel.gameObject.activeSelf)
                GetSelection();
        }   

        /* Gets the cell selected by user */
        public void GetSelection() {
            // check if there is a mouse click
            if (Input.GetMouseButtonDown(0)) {
                // Get the current position of the mouse
                // then get the cell at that position if there is
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                    Cell selectedCell = hitInfo.collider.gameObject.GetComponent<Cell>();
                    if (selectedCell != null) {
                        // user first selection should be PEG, and the second one is EMPTY cell
                        // set the start position of the movement as most recent selection
                        if (selectedCell.GetValue() == Cell.CellValue.PEG) {
                            if (_startCell != null) {
                                _startCell.SetValue(Cell.CellValue.PEG);
                                DisplayPossibleMove(false);
                            }
                            
                            _startCell = selectedCell;
                            _startCell.SetValue(Cell.CellValue.SELECTED);
                            // show user to end positions for their movement (movement prediction)
                            DisplayPossibleMove(true);
                        }
                        // make sure user select a start position before selecting an end position
                        else if ((  selectedCell.GetValue() == Cell.CellValue.EMPTY || 
                                    selectedCell.GetValue() == Cell.CellValue.PREDICTED) && 
                                    _startCell != null) { 
                            // here we have start and end position of the movement, so try to move!
                            // convert the hovering cell to it's original view
                            if (! MakeMove(_startCell, selectedCell))
                                _startCell.SetValue(Cell.CellValue.PEG);
                            // set selected startCell as null for next movement
                            DisplayPossibleMove(false);
                            _startCell = null;
                        }
                    }
                }
            }
        }

        public void DisplayPossibleMove(bool disyplayOn) {
            if (disyplayOn) {
                _possibleEndCells = GetPossibleEndPos(_startCell);
                if (_possibleEndCells != null)
                    foreach (var cell in _possibleEndCells)
                        cell.SetValue(Cell.CellValue.PREDICTED);
            }
            else if (_possibleEndCells != null) {
                Cell endCell = (_allMov.Count > 0) ? _allMov.Peek().GetEnd() : null;
                foreach (var cell in _possibleEndCells)
                    if (cell != endCell) 
                        cell.SetValue(Cell.CellValue.EMPTY);
            }
        }

        public IEnumerator PlayAuto() {
            // MakeRandomMove takes 1 sec to execute so we need to wait wait 
            // at least one second to finish it it's job. then we can call it again
            while (! IsGameOver()) {
                yield return new WaitForSeconds(0.2f);
                MakeRandomMove();
                yield return new WaitForSeconds(1f);            
            }
        } 

        private IEnumerator AutoMove(Cell start, Cell end) {
            if (_gameMode == GameMode.USER)
                _nextMoveBtn.gameObject.SetActive(false);
            start.SetValue(Cell.CellValue.SELECTED);
            yield return new WaitForSeconds(0.5f);            
            end.SetValue(Cell.CellValue.PREDICTED);
            yield return new WaitForSeconds(0.5f);            
            MakeMove(start, end); 
            if (_gameMode == GameMode.USER)
                _nextMoveBtn.gameObject.SetActive(true);
        }

        public bool MakeMove(Cell start, Cell end) {
            Movement mov = new Movement(_cells, start, end);
            if (mov.isValidMovement()) {
                Cell jump = mov.GetJump();

                // update the cell values    
                start.SetValue(Cell.CellValue.EMPTY);
                jump.SetValue(Cell.CellValue.EMPTY);
                end.SetValue(Cell.CellValue.PEG);

                // update game status (numMov, numPeg)
                ++_numMov;
                --_numPeg;
                _allMov.Push(mov);
                UpdateGameStatus();

                // make undo button active (only in user game)
                if (_gameMode == GameMode.USER && !_undoBtn.gameObject.activeSelf)
                    _undoBtn.gameObject.SetActive(true);
                
                // after a valid movement check if the game is over
                if (IsGameOver())
                    StartCoroutine(DisplayGameOverPanel());
                
                return true;
            }
            _audioSource.PlayOneShot(_wrongMovSound, 0.1f);
            return false;
        }

        public void MakeRandomMove() {
            // select an cell position and try each cell till make a movement 
            int x = Random.Range(0, _width);
            int y = Random.Range(0, _height);

            // to keep end positions and randomly select one of them
            List<Cell> endPos = null; 
            for (int i = 0; i < _width && endPos == null; ++i) {
                for (int j = 0; ++j < _height && endPos == null; ++j) {
                    if (_cells.TryGetValue(new Vector2Int(x, y), out Cell start)) {
                        endPos = GetPossibleEndPos(start);
                        // select an random movement and apply it
                        if (endPos != null) {
                            Cell end = endPos[Random.Range(0, endPos.Count)];

                            // before movement higligt the cells
                            StartCoroutine(AutoMove(start, end));
                        }
                    }
                    // iterate in y axis
                    y = (y + 1 < _height) ? y + 1 : 0; 
                }
                // iterate in x axis
                x = (x + 1 < _width) ? x + 1 : 0; 
            }
        }

        public void Undo() {
            if (_allMov.Count > 0) {
                var mov = _allMov.Pop();
                // update the cell values    
                mov.GetStart().SetValue(Cell.CellValue.PEG);
                mov.GetJump().SetValue(Cell.CellValue.PEG);
                mov.GetEnd().SetValue(Cell.CellValue.EMPTY);

                // update game status (numMov, numPeg)
                --_numMov;
                ++_numPeg;

                if (_allMov.Count == 0)
                    _undoBtn.gameObject.SetActive(false);

                UpdateGameStatus();
            }
        }

        /* returns all the possible movement that can be made with given cell */
        public List<Cell> GetPossibleEndPos(Cell start) {
            List<Cell> endPosition = new List<Cell>();
            if (start != null && (start.GetValue() == Cell.CellValue.PEG || start.GetValue() == Cell.CellValue.SELECTED)) {
                Movement mov = new Movement(_cells, start);
                // try the four main direction  
                for (int dir = 0; dir < 4; ++dir) {
                    mov.SetMovement(start, (Movement.Direction) dir);
                    if (mov.isValidMovement())
                        endPosition.Add(mov.GetEnd());
                }
            }
            // return null instead of empty list
            return endPosition.Count > 0 ? endPosition : null; 
        }

        public bool IsGameOver() {
            // check if a movement can be made with any cell 
            foreach (var cell in _cells.Values) 
                if (GetPossibleEndPos(cell) != null)
                    return false;
            return true;
        }

        public void LoadGame() {
            List<string> lines = _boardDB.GetBoardLayout((int) _gameBoardType);
            
            // first line contains values width, height and number of movements
            if (lines.Count > 0) {
                string[] values = lines[0].Split();
                if (values.Length < 3) 
                    throw new System.ArgumentException();

                _width = int.Parse(values[0]);
                _height = int.Parse(values[1]);
                _numMov = int.Parse(values[2]);
                _numPeg = 0;

                Vector2Int pos = new Vector2Int(); // position of the cell 
                
                // we already scan the first line (line[0])
                for (int y = lines.Count - 1; y > 0; --y) {
                    pos.y = _height - y; // ++ also works
                    for (int x = 0; x <= lines[y].Length / 2; ++x) {
                        pos.x = x;
                        switch (lines[y][x * 2]) { 
                            case '.': // Empty
                                InstantiateCell(pos, Cell.CellValue.EMPTY); 
                                break;
                            case 'P': // Peg
                                InstantiateCell(pos, Cell.CellValue.PEG); 
                                ++_numPeg;
                                break;
                        }
                    }
                }
            }  
        }

        public void ReturnMainMenu() {
            // turn back the MainMenuScene (following two line does the same thing)
            // SceneManager.LoadScene("Scenes/MainMenuScene");
            _gameMode = (PlayerPrefs.HasKey("_selectedGameModeOption")) ?
                (GameMode) PlayerPrefs.GetInt("_selectedGameModeOption") : 0;
            if (_gameMode == GameMode.USER)
                PlayerPrefs.SetInt("_" + (int) _gameBoardType + "BoardScore", _numPeg);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        private IEnumerator DisplayGameOverPanel() {
            yield return new WaitForSeconds(1f);
            _audioSource.PlayOneShot(_numPeg == 1 ? _winnerSound : _loserSound);
            _gameOverPanel.gameObject.SetActive(true);
        }

        private void UpdateGameStatus() {
           _textNumMov.text = $"Number Of Movement:  {_numMov.ToString()}";
           _textNumPeg.text = $"Peg Remain:  {_numPeg.ToString()}";
        }

        public void ReplayGame() {
            // reload the game scene again this will cause
            // to recreating each object in the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private Cell InstantiateCell(Vector2Int pos, Cell.CellValue val) {
            Cell cell = Instantiate(_cellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            cell.Init(pos, val);
            _cells.Add(pos, cell);
            return cell;
        }
    }

}