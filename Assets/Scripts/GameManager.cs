using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace pegsolitaire {
    public class GameManager : MonoBehaviour {
        private int _width; //! is it required to keep (for now yes to save it, actually it depends on saveGame func)
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
        void Start() {
            _cells = new Dictionary<Vector2Int, Cell>();
            _selectedCells = new List<Cell>();
            _allMov = new Stack<Movement>();
            _numberOfMovement = GameObject.Find("Canvas/NumberOfMovement").GetComponent<TMPro.TextMeshProUGUI>();

            createBoard(BoardType.DIAMOND);
            // _numMov = 0;
            // _numPeg = _cells.Count;

            // change the position of the camera as shows center of the game board
            _camera.transform.position = new Vector3((float) _width / 2 - 0.5f, (float) _height / 2 - 0.5f, -10);
        }

        void Update() {
            getSelection();
            
            // checks random movement
            if (Input.GetKeyDown(KeyCode.W))
                makeRandomMove();   //! debugging needed
            // checks undo
            if (Input.GetKeyDown(KeyCode.S)) 
                undo();
        }   

        /* gets the cell selected by user */
        private void getSelection() {
            if (Input.GetMouseButtonDown(0)) {
                // get the current posation of the mouse
                // then capture the cell at that posation (if there is)
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                    Cell cell = hitInfo.collider.gameObject.GetComponent<Cell>();
                    if (cell != null) {
                        // user first selection should be Peg, and the next is Empty cell
                        if (cell.getValue() == (_selectedCells.Count == 0 ? Cell.CellValue.PEG : Cell.CellValue.EMPTY))
                        // cell.getValue() == Cell.CellValue.PEG 
                        // make sure to don't select same cell two times
                        if (!_selectedCells.Contains(cell)) {
                            _selectedCells.Add(cell);    
                            cell.setValue(Cell.CellValue.SELECTED);

                            // clear the list after movement made
                            if (_selectedCells.Count == 2) {
                                // make movement 
                                if (makeMove(_selectedCells[0], _selectedCells[1])) {
                                    // after a valid movement check if game is over
                                    if (isGameOver()) //! Game is over shows only in user mod so do something else
                                        Debug.Log($"GAME IS OVER \n#Mov: {_numMov} #Peg: {_numPeg}");
                                } 
                                
                                else {
                                    // turn back the previos values to cell
                                    // first cell was peg, second one was empty cell
                                    _selectedCells[0].setValue(Cell.CellValue.PEG);
                                    _selectedCells[1].setValue(Cell.CellValue.EMPTY);
                                }
                                _selectedCells.Clear();
                            }
                        }
                    }
                }
            }
        }

        public bool makeMove(Cell start, Cell end) {
            Movement mov = new Movement(_cells, start, end);
            if (mov.isValidMovement()) {
                Cell jump = mov.getJump();

                // update the cell values    
                start.setValue(Cell.CellValue.EMPTY);
                jump.setValue(Cell.CellValue.EMPTY);
                end.setValue(Cell.CellValue.PEG);

                // update game status (numMov, numPeg)
                ++_numMov;
                --_numPeg;
                _allMov.Push(mov);
                updateGameStatus();
                return true;
            }
            Debug.Log("Invalid Movement");
            return false;
        }

        public bool makeMove(Movement mov) {return mov != null ? makeMove(mov.getStart(), mov.getEnd()) : false;}
        
        public void makeRandomMove() {
            int x = Random.Range(0, _width);
            int y = Random.Range(0, _height);

            for (int i = 0; i < _width; ++i) {
                for (int j = 0; ++j < _height; ++j) {
                    if (_cells.TryGetValue(new Vector2Int(x, y), out Cell start)) {
                        var movs = getAllMovements(start);
                        // select an random movement and apply it
                        if (movs != null) {
                            makeMove(movs[Random.Range(0, movs.Count)]); // returns always true
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

        public void undo() {
            if (_allMov.Count > 0) {
                var mov = _allMov.Pop();
                // update the cell values    
                mov.getStart().setValue(Cell.CellValue.PEG);
                mov.getJump().setValue(Cell.CellValue.PEG);
                mov.getEnd().setValue(Cell.CellValue.EMPTY);

                // update game status (numMov, numPeg)
                --_numMov;
                ++_numPeg;
                updateGameStatus();
            }
        }

        /* returns all the possible movement that can be made with given cell */
        public List<Movement> getAllMovements(Cell start) {
            List<Movement> allMov = new List<Movement>();

            if (start.getValue() == Cell.CellValue.PEG || start.getValue() == Cell.CellValue.SELECTED) {
                // try the four movement direction
                for (int dir = 0; dir < 4; ++dir) {
                    Movement mov = new Movement(_cells);
                    mov.setMovement(start, (Movement.Direction) dir);
                    if (mov.isValidMovement())
                        allMov.Add(mov);
                }
            }

            // return null istead of empty list
            return allMov.Count > 0 ? allMov : null; 
        }

        public bool isGameOver() {
            // check if a movement can be made with any cell 
            foreach (var cell in _cells.Values) 
                if (getAllMovements(cell) != null)
                    return false;
            return true;
        }

        public void createBoard(BoardType t) {
            switch (t) {
                case BoardType.FRENCH: loadGame("Assets/System/GameBoards/French.txt"); break; 
                case BoardType.GERMAN: loadGame("Assets/System/GameBoards/German.txt"); break; 
                case BoardType.ASYMETRICAL: loadGame("Assets/System/GameBoards/Asymetrical.txt"); break; 
                case BoardType.ENGLISH: loadGame("Assets/System/GameBoards/English.txt"); break; 
                case BoardType.DIAMOND: loadGame("Assets/System/GameBoards/Diamond.txt"); break; 
                case BoardType.TRIANGULAR: loadGame("Assets/System/GameBoards/Triangular.txt"); break;
            }
            //! return value
        }

        public void createBoard(string boardName) {
            loadGame("Assets/System/GameBoards/" + boardName + ".txt");
        }

        public void saveGame() {
            //! dumb everything in the _cells dictionary which contains all the cells 
        }
        public void loadGame(string path) {
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

        private void updateGameStatus() {
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