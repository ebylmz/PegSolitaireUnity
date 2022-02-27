using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pegsolitaire {
    [CreateAssetMenu]
    public class GameModeDataBase : ScriptableObject {
        [SerializeField] private GameMode[] _gameMode;

        public int GameModeCount {
            get {
                return _gameMode.Length;
            }
        }

        public GameMode GetGameMode(int index) {
            return _gameMode[index];
        }
    }
}