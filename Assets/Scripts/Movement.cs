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
        
        public enum Direction {UP, DOWN, LEFT, RIGHT};

        public Movement(Dictionary<Vector2Int, Cell> board, Cell start, Cell end) {
            _board = board;
            try {
                SetStart(start);
                SetEnd(end);
                // SetJump();
            }
            catch (System.ArgumentException e) {
                System.Console.WriteLine($"Invalid arguments: {e}");
            }
        }

        public Movement(Dictionary<Vector2Int, Cell> board, Cell start) : this(board, start, null) {}

        public Movement(Dictionary<Vector2Int, Cell> board) : this(board, null, null) {}
        
        public Movement() : this(null, null, null) {}

        public Cell GetStart() {return _start;}
        
        public Cell GetJump() {/* SetJump(); */ return _jump;}
        
        public Cell GetEnd() {return _end;}

        public void SetStart(Cell start) {
            if (_board.ContainsValue(start)) // && start.GetValue() == Cell.CellValue.PEG // may be in selected mode 
                _start = start;
            else {
                _start = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }
        
        public void SetJump() {
            if (_board == null || _start == null || _end == null) 
                _jump = null;
            else {
                // a movement is either horizontal (right, left) or vertical(up, down)
                // so there is two possible cases for valid movement
                var startPos = _start.GetPosition();
                var endPos = _end.GetPosition();
                int jumpx = -1, jumpy = -1; // jump cell location
                
                // vertical movement
                if (startPos.x == endPos.x) {
                    jumpx = startPos.x;
                    
                    int diff = endPos.y - startPos.y;
                    if (diff == 2) // up movement
                        jumpy = endPos.y - 1;
                    else if (diff == -2) // down movement 
                        jumpy = endPos.y + 1;
                }
                // horizontal movement  
                else if (startPos.y == endPos.y) {
                    jumpy = startPos.y;

                    int diff = endPos.x - startPos.x;
                    if (diff == 2) // right movement  
                        jumpx = endPos.x - 1;
                    else if (diff == -2) // left movement
                        jumpx = endPos.x + 1; 
                }

                // Set jump cell null if this is an invalid movement
                _jump = (_board.TryGetValue(new Vector2Int(jumpx, jumpy), out var jumpCell) && jumpCell.GetValue() == Cell.CellValue.PEG) ?
                            jumpCell : null;
            }
        }

        public void SetEnd(Cell end) {
            if (_board.ContainsValue(end)) // && end.GetValue() == Cell.CellValue.EMPTY 
                _end = end;
            else {
                _end = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }

        public bool SetMovement(Cell start, Direction d) {
            try {
                SetStart(start);
                Vector2Int startPos = start.GetPosition();

                switch (d) {
                    case Direction.UP:
                        SetEnd(GetEndUp(startPos)); break;
                    case Direction.DOWN: 
                        SetEnd(GetEndDown(startPos)); break;
                    case Direction.LEFT: 
                        SetEnd(GetEndLeft(startPos)); break;
                    case Direction.RIGHT: 
                        SetEnd(GetEndRight(startPos)); break;
                }
                SetJump();
            }
            catch (System.ArgumentException) {
                // just catch the exception
            }
            return _jump != null;
        }

        private Cell GetEndUp(Vector2Int startPos) {
            return (_board.TryGetValue(new Vector2Int(startPos.x, startPos.y + 2), out Cell end)) ? end : null;
        }

        private Cell GetEndDown(Vector2Int startPos) {
            return (_board.TryGetValue(new Vector2Int(startPos.x, startPos.y - 2), out Cell end)) ? end : null;
        }

        private Cell GetEndLeft(Vector2Int startPos) {
            return (_board.TryGetValue(new Vector2Int(startPos.x - 2, startPos.y), out Cell end)) ? end : null;
        }

        private Cell GetEndRight(Vector2Int startPos) {
            return (_board.TryGetValue(new Vector2Int(startPos.x + 2, startPos.y), out Cell end)) ? end : null;
        }

        public bool isValidMovement() {
            // jump cell becomes null when start and end positions don't indicate a valid movement
            SetJump();

            // Debug.Log($"Start value: {_start.GetValue()}\nJump value : {_jump.GetValue()} \nEnd value  : {_end.GetValue()}");

            return  (_start.GetValue() == Cell.CellValue.PEG || _start.GetValue() == Cell.CellValue.SELECTED) && 
                    (_jump != null && _jump.GetValue() == Cell.CellValue.PEG) &&
                    (_end.GetValue() == Cell.CellValue.SELECTED || _end.GetValue() == Cell.CellValue.EMPTY || _end.GetValue() == Cell.CellValue.PREDICTED);
        }
    }
}
