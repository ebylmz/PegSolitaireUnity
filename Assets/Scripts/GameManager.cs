using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace pegsolitaire {
    public class GameManager : MonoBehaviour {
        private int _width;
        private int _height;
        [SerializeField] private Cell _cellPrefab; 
        [SerializeField] private Camera _camera;
        private List<Cell> _selectedCells;
        private Dictionary<Vector2Int, Cell> _cells;

        // Start is called before the first frame update
        void Start() {
            _width = 9;
            _height = 9;
            _cells = createBoard();
            _selectedCells = new List<Cell>();

            // change the position of the camera as center of the grid
            _camera.transform.position = new Vector3((float) _width / 2 - 1.5f, (float) _height / 2 - 1.5f, -10);

            foreach(var e in _cells)
                Debug.Log(e);
        }

        void Update() {
            getSelection();
        }   

        private Dictionary<Vector2Int, Cell> createBoard() {
            _cells = new Dictionary<Vector2Int, Cell>();

            for (int y = 0, n = 2, m = 5; y < 2; ++y, --n, ++m)
                for (int x = n; x < m; ++x)
                    InstantiateCell(new Vector2Int(y, x));

            for (int y = 2; y < 5; ++y)
                for (int x = 0; x < 7; ++x) 
                    InstantiateCell(new Vector2Int(y, x));

            for (int y = 5, n = 1, m = 6; y < 7; ++y, ++n, --m)
                for (int x = n; x < m; ++x) 
                    InstantiateCell(new Vector2Int(y, x));

            /*
            if (_cells.TryGetValue(new Vector2Int(2, 3), out var cell))
                cell.setEmpty();
            */
            _cells[new Vector2Int(2, 3)].setValue(Cell.CellValue.Empty);
            return _cells;
        }

        /* gets the cell selected by user */
        private void getSelection() {
            if (Input.GetMouseButtonDown(0)) {
                // get the current location of the mouse
                // then capture the cell at that location (if there is)
                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                    Cell cell = hitInfo.collider.gameObject.GetComponent<Cell>();
                    if (cell != null) {
                        // user first selection should be Peg, and the next is Empty cell
                        if (cell.getValue() == (_selectedCells.Count == 0 ? Cell.CellValue.Peg : Cell.CellValue.Empty))
                        // cell.getValue() == Cell.CellValue.Peg 
                        // make sure to don't select same cell two times
                        if (!_selectedCells.Contains(cell)) {
                            _selectedCells.Add(cell);    
                            cell.setValue(Cell.CellValue.Selected);

                            // clear the list after movement made
                            if (_selectedCells.Count == 2) {
                                // make movement 
                                if (! makeMove(_selectedCells[0], _selectedCells[1])) {
                                    // turn back the previos values to cell
                                    // first cell was peg, second one was empty cell
                                    _selectedCells[0].setValue(Cell.CellValue.Peg);
                                    _selectedCells[1].setValue(Cell.CellValue.Empty);
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
                Debug.Log("Valid movement");
                Cell jump = mov.getJump();

                // update the cell values    
                start.setValue(Cell.CellValue.Empty);
                jump.setValue(Cell.CellValue.Empty);
                end.setValue(Cell.CellValue.Peg);

                //! update game status (numMov, numPeg)

                return true;
            }
            else 
                Debug.Log("InValid movement");

            return false;
        }

        public bool makeRandomMove() {
            //! NOT IMPLEMENTED YET
            return true;
        }

        public bool isGameOver() {return true;}
        public void undo() {}
        public void saveGame() {}
        public void loadGame() {}

        private Cell InstantiateCell(Vector2Int pos) {
            Cell cell = Instantiate(_cellPrefab, new Vector3(pos.y, pos.x, 0), Quaternion.identity);
            cell.Init(pos.ToString(), pos, Cell.CellValue.Peg);
            _cells.Add(pos, cell);
            return cell;
        }
    }

}