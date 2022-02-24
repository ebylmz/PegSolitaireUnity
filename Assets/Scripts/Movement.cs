using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pegsolitaire {

    public class Movement {
        private Dictionary<Vector2Int, Cell> _board; // game board for checking validty of movement
        private (int, int) _dimension; // board dimension (number of rows and columns)
        private Cell _start; // start position of movement
        private Cell _jump; // jump position of movement (between start and end)
        private Cell _end; // end position of movement
        public Movement(Dictionary<Vector2Int, Cell> board, Cell start, Cell end) {
            _board = board;
            try {
                setStart(start);
                setEnd(end);
                // setJump();
            }
            catch (System.ArgumentException e) {
                System.Console.WriteLine($"Invalid arguments: {e}");
            }
        }

        public Movement(Dictionary<Vector2Int, Cell> board, Cell start) : this(board, start, null) {}

        public Movement(Dictionary<Vector2Int, Cell> board) : this(board, null, null) {}
        
        public Movement() : this(null, null, null) {}

        public Cell getStart() {return _start;}
        
        public Cell getJump() {setJump(); return _jump;}
        
        public Cell getEnd() {return _end;}

        public void setStart(Cell start) {
            if (_board.ContainsValue(start)) // && start.getValue() == Cell.CellValue.Peg // may be in selected mode 
                _start = start;
            else {
                Debug.Log("Invlaid start");
                _start = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }
        public void setJump() {
            if (_board == null || _start == null || _end == null) 
                _jump = null;
            else {
                //! x indicates col, y indicates row
                // a movement is either horizontal (right, left) or vertical(up, down)
                // so there is two possible cases for valid movement
                var startLoc = _start.getLocation();
                var endLoc = _end.getLocation();
                int row = -1, col = -1; // jump cell location
                // same row 
                if (startLoc.x == endLoc.x) {
                    row = startLoc.x;
                    
                    int diff = endLoc.y - startLoc.y;
                    if (diff == 2) // left movement
                        col = endLoc.y - 1;
                    else if (diff == -2) // right movement
                        col = endLoc.y + 1;
                }
                // same column
                else if (startLoc.y == endLoc.y) {
                    col = startLoc.y;

                    int diff = endLoc.x - startLoc.x;
                    if (diff == 2) // down movement
                        row = endLoc.x - 1;
                    else if (diff == -2) // up movement
                        row = endLoc.x + 1; 
                }

                // set jump cell null if this is an invalid movement
                _jump = (_board.TryGetValue(new Vector2Int(row, col), out var jumpCell) && jumpCell.getValue() == Cell.CellValue.Peg) ?
                            jumpCell : null;

                Debug.Log(jumpCell);
                Debug.Log(_jump);
            }
            Debug.Log(_jump);
        }

        public void setEnd(Cell end) {
            if (_board.ContainsValue(end)) // && end.getValue() == Cell.CellValue.Empty 
                _end = end;
            else {
                _end = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }

        public bool isValidMovement() {
            // jump cell becomes null when start and end positions don't indicate a valid movement
            setJump();
            return  _jump != null;
        }
    }
}
