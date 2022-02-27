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
    }
}