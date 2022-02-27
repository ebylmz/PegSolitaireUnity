using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace pegsolitaire {
    public class GameManager : MonoBehaviour {
        public enum BoardType {FRENCH, GERMAN, ASYMETRICAL, ENGLISH, DIAMOND, TRIANGULAR}
        public enum GameMode {USER, COMPUTER}

        [SerializeField] private Cell _cellPrefab; 
        [SerializeField] private Camera _camera;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _successMov;
        [SerializeField] private AudioClip _failMov;
        private GameMode _gameMode;
        private BoardType _gameBoardType; 
        private int _width; // width of the game board 
        private int _height; // height of game board
        private Dictionary<Vector2Int, Cell> _cells;
        private TMPro.TextMeshProUGUI _numberOfMovement; //! file that keeps number of movement 
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
            _numberOfMovement = GameObject.Find("Canvas/NumberOfMovement").GetComponent<TMPro.TextMeshProUGUI>();
            
            // access the stored player preference
            _gameMode = (PlayerPrefs.HasKey("_selectedGameModeOption")) ?
                (GameMode) PlayerPrefs.GetInt("_selectedGameModeOption") : 0;
            _gameBoardType = (PlayerPrefs.HasKey("_selectedBoardOption")) ? 
                (BoardType) PlayerPrefs.GetInt("_selectedBoardOption") : 0;
        }

        void Start() {
            // create the game board
            CreateBoard(_gameBoardType);

            // change the position of the camera as shows center of the game board
            _camera.transform.position = new Vector3((float) _width / 2 - 0.5f, (float) _height / 2 , -10);

            // start computer game  
            if (_gameMode == GameMode.COMPUTER)
                StartCoroutine(PlayAuto());
        }

        void Update() {
            // user cannot make movement in ComputerPlay mode
            if (_gameMode == GameMode.USER) 
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
                            if (MakeMove(_startCell, selectedCell)) {
                                // after a valid movement check if the game is over
                                //! Game is over showns only in user mod so do something else
                                if (IsGameOver()) {
                                    Debug.Log($"GAME IS OVER \n#Mov: {_numMov} #Peg: {_numPeg}");
                                    // load main menu after game is over
                                    SceneManager.LoadScene("Scenes/MainMenuScene");
                                } 
                            }
                            // convert the hovering cell to it's original view
                            else
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
            start.SetValue(Cell.CellValue.SELECTED);
            yield return new WaitForSeconds(0.5f);            
            end.SetValue(Cell.CellValue.PREDICTED);
            yield return new WaitForSeconds(0.5f);            
            MakeMove(start, end); 
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
                
                // play sound to indicate an valid movemement
                // _audioSource.PlayOneShot(_successMov, 0.2f);
                return true;
            }
            _audioSource.PlayOneShot(_failMov, 0.2f);
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
            DisplayPossibleMove(false); //!!!!!!
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

                DisplayPossibleMove(false); //!!!!!!
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

        public void CreateBoard(BoardType t) {
            switch (t) {
                case BoardType.FRENCH: LoadGame("AsSets/System/GameBoards/French.txt"); break; 
                case BoardType.GERMAN: LoadGame("AsSets/System/GameBoards/German.txt"); break; 
                case BoardType.ASYMETRICAL: LoadGame("AsSets/System/GameBoards/Asymetrical.txt"); break; 
                case BoardType.ENGLISH: LoadGame("AsSets/System/GameBoards/English.txt"); break; 
                case BoardType.DIAMOND: LoadGame("AsSets/System/GameBoards/Diamond.txt"); break; 
                case BoardType.TRIANGULAR: LoadGame("AsSets/System/GameBoards/Triangular.txt"); break;
            }
            //! return value
        }

        public void CreateBoard(string boardName) {
            LoadGame("AsSets/System/GameBoards/" + boardName + ".txt");
        }

        public void SaveGame() {
            //! dumb everything in the _cells dictionary which contains all the cells 
        }

        public void LoadGame(string path) {
            string[] lines = System.IO.File.ReadAllLines(path);

            // first line contains values width, height and number of movements
            if (lines.Length > 0) {
                string[] values = lines[0].Split();
                if (values.Length < 3) 
                    throw new System.ArgumentException();

                _width = int.Parse(values[0]);
                _height = int.Parse(values[1]);
                _numMov = int.Parse(values[2]);
                _numPeg = 0;

                Vector2Int pos = new Vector2Int(); // position of the cell 
                
                // we already scan the first line (line[0])
                for (int y = lines.Length - 1; y > 0; --y) {
                    pos.y = _height - y; // ++ also works
                    for (int x = 0; x <= lines[y].Length / 2; ++x) {
                        pos.x = x;
                        switch (lines[y][x * 2]) { //! x * 2 is not safe
                            case '.': // Empty
                                InstantiateCell(pos, Cell.CellValue.EMPTY); 
                                break;
                            case 'P': // Peg
                                InstantiateCell(pos, Cell.CellValue.PEG); 
                                ++_numPeg;
                                break;
                            case ' ': // Wall
                                // InstantiateCell(pos); 
                                break;
                            // default: 
                            //     return false;
                        }
                    }
                }
            }  
            //! it would be great to return succes return value 
        }

        public void ReturnMainMenu() {
            // turn back the MainMenuScene (following two line does the same thing)
            // SceneManager.LoadScene("Scenes/MainMenuScene");
            _gameMode = (PlayerPrefs.HasKey("_selectedGameModeOption")) ?
                (GameMode) PlayerPrefs.GetInt("_selectedGameModeOption") : 0;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

        private void UpdateGameStatus() {
           _numberOfMovement.text = $"Number Of Movement:  {_numMov.ToString()}";
        }

        private Cell InstantiateCell(Vector2Int pos, Cell.CellValue val) {
            Cell cell = Instantiate(_cellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            cell.Init(pos, val);
            _cells.Add(pos, cell);
            return cell;
        }
    }

}