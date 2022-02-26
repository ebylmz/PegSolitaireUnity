using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//* SceneManager.LoadScene("Scenes/GameScene");

namespace pegsolitaire {
    public class GameManager : MonoBehaviour {
        private int _width; //! is it required to keep (for now yes to save it, actually it depends on SaveGame func)
        private int _height;
        [SerializeField] private Cell _cellPrefab; 
        [SerializeField] private Camera _camera;
        private List<Cell> _selectedCells;
        private Dictionary<Vector2Int, Cell> _cells;

        private TMPro.TextMeshProUGUI _numberOfMovement; // file
        
        private Stack<Movement> _allMov; // keeps all the movement made so far
        private int _numMov; 
        private int _numPeg; 

        public enum BoardType {FRENCH, GERMAN, ASYMETRICAL, ENGLISH, DIAMOND, TRIANGULAR}

        public enum GameMode {USER, COMPUTER}

        // Start is called before the first frame update
        void Awake() {
            _cells = new Dictionary<Vector2Int, Cell>();
            _selectedCells = new List<Cell>();
            _allMov = new Stack<Movement>();
            _numberOfMovement = GameObject.Find("Canvas/NumberOfMovement").GetComponent<TMPro.TextMeshProUGUI>();
            CreateBoard(BoardType.FRENCH);
            // _numMov = 0;
            // _numPeg = _cells.Count;

            // change the position of the camera as shows center of the game board
            _camera.transform.position = new Vector3((float) _width / 2 - 0.5f, (float) _height / 2 , -10);
        }

        void Update() {
            GetSelection();
            
            // checks random movement
            if (Input.GetKeyDown(KeyCode.W))
                MakeRandomMove();   //! debugging needed
            // checks Undo
            if (Input.GetKeyDown(KeyCode.S)) 
                Undo();
        }   

        public void Init() {
            gameObject.SetActive(true);
        }

        /* Gets the cell selected by user */
        public void GetSelection() {
            if (Input.GetMouseButtonDown(0)) {
                // Get the current posation of the mouse
                // then capture the cell at that posation (if there is)
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                    Cell cell = hitInfo.collider.gameObject.GetComponent<Cell>();
                    if (cell != null) {
                        // user first selection should be Peg, and the next is Empty cell
                        if (cell.GetValue() == (_selectedCells.Count == 0 ? Cell.CellValue.PEG : Cell.CellValue.EMPTY))
                        // cell.GetValue() == Cell.CellValue.PEG 
                        // make sure to don't select same cell two times
                        if (!_selectedCells.Contains(cell)) {
                            _selectedCells.Add(cell);    
                            cell.SetValue(Cell.CellValue.SELECTED);

                            // clear the list after movement made
                            if (_selectedCells.Count == 2) {
                                // make movement 
                                if (MakeMove(_selectedCells[0], _selectedCells[1])) {
                                    // after a valid movement check if game is over
                                    if (IsGameOver()) {
                                        Debug.Log($"GAME IS OVER \n#Mov: {_numMov} #Peg: {_numPeg}");
                                        SceneManager.LoadScene("Scenes/MainMenuScene");
                                    } //! Game is over shows only in user mod so do something else
                                } 
                                
                                else {
                                    // turn back the previos values to cell
                                    // first cell was peg, second one was empty cell
                                    _selectedCells[0].SetValue(Cell.CellValue.PEG);
                                    _selectedCells[1].SetValue(Cell.CellValue.EMPTY);
                                }
                                _selectedCells.Clear();
                            }
                        }
                    }
                }
            }
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
                return true;
            }
            Debug.Log("Invalid Movement");
            return false;
        }

        public bool MakeMove(Movement mov) {return mov != null ? MakeMove(mov.GetStart(), mov.GetEnd()) : false;}
        
        public void MakeRandomMove() {
            int x = Random.Range(0, _width);
            int y = Random.Range(0, _height);

            for (int i = 0; i < _width; ++i) {
                for (int j = 0; ++j < _height; ++j) {
                    if (_cells.TryGetValue(new Vector2Int(x, y), out Cell start)) {
                        var movs = GetAllMovements(start);
                        // select an random movement and apply it
                        if (movs != null) {
                            MakeMove(movs[Random.Range(0, movs.Count)]); // returns always true
                            return;
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
                UpdateGameStatus();
            }
        }

        /* returns all the possible movement that can be made with given cell */
        public List<Movement> GetAllMovements(Cell start) {
            List<Movement> allMov = new List<Movement>();

            if (start.GetValue() == Cell.CellValue.PEG || start.GetValue() == Cell.CellValue.SELECTED) {
                // try the four movement direction
                for (int dir = 0; dir < 4; ++dir) {
                    Movement mov = new Movement(_cells);
                    mov.SetMovement(start, (Movement.Direction) dir);
                    if (mov.isValidMovement())
                        allMov.Add(mov);
                }
            }

            // return null istead of empty list
            return allMov.Count > 0 ? allMov : null; 
        }

        public bool IsGameOver() {
            // check if a movement can be made with any cell 
            foreach (var cell in _cells.Values) 
                if (GetAllMovements(cell) != null)
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

        public void ExitGame() {
            SceneManager.LoadScene("Scenes/MainMenuScene");
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