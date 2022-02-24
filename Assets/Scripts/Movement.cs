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
        
        public enum Direction {Up, Down, Left, Right};

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
        
        public Cell getJump() {/* setJump(); */ return _jump;}
        
        public Cell getEnd() {return _end;}

        public void setStart(Cell start) {
            if (_board.ContainsValue(start)) // && start.getValue() == Cell.CellValue.Peg // may be in selected mode 
                _start = start;
            else {
                _start = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }
        
        public void setJump() {
            if (_board == null || _start == null || _end == null) 
                _jump = null;
            else {
                // a movement is either horizontal (right, left) or vertical(up, down)
                // so there is two possible cases for valid movement
                var startLoc = _start.getLocation();
                var endLoc = _end.getLocation();
                int jumpx = -1, jumpy = -1; // jump cell location
                
                // vertical movement
                if (startLoc.x == endLoc.x) {
                    jumpx = startLoc.x;
                    
                    int diff = endLoc.y - startLoc.y;
                    if (diff == 2) // up movement
                        jumpy = endLoc.y - 1;
                    else if (diff == -2) // down movement 
                        jumpy = endLoc.y + 1;
                }
                // horizontal movement  
                else if (startLoc.y == endLoc.y) {
                    jumpy = startLoc.y;

                    int diff = endLoc.x - startLoc.x;
                    if (diff == 2) // right movement  
                        jumpx = endLoc.x - 1;
                    else if (diff == -2) // left movement
                        jumpx = endLoc.x + 1; 
                }

                // set jump cell null if this is an invalid movement
                _jump = (_board.TryGetValue(new Vector2Int(jumpx, jumpy), out var jumpCell) && jumpCell.getValue() == Cell.CellValue.Peg) ?
                            jumpCell : null;
            }
        }

        public void setEnd(Cell end) {
            if (_board.ContainsValue(end)) // && end.getValue() == Cell.CellValue.Empty 
                _end = end;
            else {
                _end = null;
                throw new System.ArgumentException("Given cell does't exist in game board");
            }
        }

        public bool setMovement(Cell start, Direction d) {
            try {
                setStart(start);
                Vector2Int startLoc = start.getLocation();

                switch (d) {
                    case Direction.Up:
                        setEnd(getEndUp(startLoc)); break;
                    case Direction.Down: 
                        setEnd(getEndDown(startLoc)); break;
                    case Direction.Left: 
                        setEnd(getEndLeft(startLoc)); break;
                    case Direction.Right: 
                        setEnd(getEndRight(startLoc)); break;
                }
                setJump();
            }
            catch (System.ArgumentException) {
                // just catch the exception
            }
            return _jump != null;
        }

        private Cell getEndUp(Vector2Int startLoc) {
            return (_board.TryGetValue(new Vector2Int(startLoc.x, startLoc.y + 2), out Cell end)) ? end : null;
        }

        private Cell getEndDown(Vector2Int startLoc) {
            return (_board.TryGetValue(new Vector2Int(startLoc.x, startLoc.y - 2), out Cell end)) ? end : null;
        }

        private Cell getEndLeft(Vector2Int startLoc) {
            return (_board.TryGetValue(new Vector2Int(startLoc.x - 2, startLoc.y), out Cell end)) ? end : null;
        }

        private Cell getEndRight(Vector2Int startLoc) {
            return (_board.TryGetValue(new Vector2Int(startLoc.x + 2, startLoc.y), out Cell end)) ? end : null;
        }

        public bool isValidMovement() {
            // jump cell becomes null when start and end positions don't indicate a valid movement
            setJump();

            // Debug.Log($"Start value: {_start.getValue()}\nJump value : {_jump.getValue()} \nEnd value  : {_end.getValue()}");

            return  (_start.getValue() == Cell.CellValue.Peg || _start.getValue() == Cell.CellValue.Selected) && 
                    (_jump != null && _jump.getValue() == Cell.CellValue.Peg) &&
                    (_end.getValue() == Cell.CellValue.Selected || _end.getValue() == Cell.CellValue.Empty || _end.getValue() == Cell.CellValue.Predicted);
        }
    }
}
