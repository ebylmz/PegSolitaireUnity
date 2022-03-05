using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pegsolitaire {
    public class MusicManager : MonoBehaviour {
        private static MusicManager _musicManager;
        void Awake() {
            // to do not destroy the this object when loading a new scene
            DontDestroyOnLoad(this);

            // when we again turn back our beginnig scene each object 
            // created again and there will be two same object so keep the
            // first one and destroy the second one
            if (_musicManager == null)
                _musicManager = this;
            else
                Destroy(gameObject);
        }
    }
}