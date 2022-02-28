using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pegsolitaire {
    [CreateAssetMenu]
    public class BoardDataBase : ScriptableObject {
        [SerializeField] private Board[] _board;

        public int BoardCount {get {return _board.Length;}}

        public Board GetBoard(int index) {
            return _board[index];
        }
        public List<string> GetBoardLayout(int index) {
            List<string> board = new List<string>();
            switch (index) {
                default:
                case 0: 
                    board.Add("7 7 0");
                    board.Add("    P P P    ");
                    board.Add("  P P P P P  ");
                    board.Add("P P P . P P P");
                    board.Add("P P P P P P P");
                    board.Add("P P P P P P P");
                    board.Add("  P P P P P  ");
                    board.Add("    P P P    ");
                    break;
                case 1: 
                    board.Add("9 9 0");
                    board.Add("      P P P      ");
                    board.Add("      P P P      ");
                    board.Add("      P P P      ");
                    board.Add("P P P P P P P P P");
                    board.Add("P P P P . P P P P");
                    board.Add("P P P P P P P P P");
                    board.Add("      P P P      ");
                    board.Add("      P P P      ");
                    board.Add("      P P P      ");
                    break;
                case 2: 
                    board.Add("8 8 0");
                    board.Add("    P P P      ");
                    board.Add("    P P P      ");
                    board.Add("    P P P      ");
                    board.Add("P P P P P P P P");
                    board.Add("P P P . P P P P");
                    board.Add("P P P P P P P P");
                    board.Add("    P P P      ");
                    board.Add("    P P P      ");
                    break;
                case 3: 
                    board.Add("7 7 0");
                    board.Add("    P P P    ");
                    board.Add("    P P P    ");
                    board.Add("P P P P P P P");
                    board.Add("P P P . P P P");
                    board.Add("P P P P P P P");
                    board.Add("    P P P    ");
                    board.Add("    P P P    ");
                    break;
                case 4: 
                    board.Add("9 9 0");
                    board.Add("        P        ");
                    board.Add("      P P P      ");
                    board.Add("    P P P P P    ");
                    board.Add("  P P P P P P P  ");
                    board.Add("P P P P . P P P P");
                    board.Add("  P P P P P P P  ");
                    board.Add("    P P P P P    ");
                    board.Add("      P P P      ");
                    board.Add("        P        ");
                    break;
                case 5:
                    board.Add("11 7 0");
                    board.Add("                     ");
                    board.Add("P P P P P P P P P P P");
                    board.Add("  P P P P P P P P P  ");
                    board.Add("    P P P P P P P    ");
                    board.Add("      P P P P P      ");
                    board.Add("        P P P        ");
                    board.Add("          .          ");
                    break;
            }
            return board;
        }
    }
}

