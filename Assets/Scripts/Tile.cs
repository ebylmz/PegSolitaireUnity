using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _pegMaterial;
    private Renderer _rend;
    private bool _isSelected;

    // Start is called before the first frame update
    void Start() {
        _rend = GetComponent<Renderer>();
        // GetComponent<MeshRenderer>().enabled = false;


    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnMouseEnter() {
        _rend.material = _selectedMaterial;
    }

    // private void OnMouseOver() {
    //     _rend.material = _highlightMaterial;
    // }

    private void OnMouseExit() {
        _rend.material = _pegMaterial;
    }

    public void setSelected(bool isSelected) {
        _rend.material = isSelected ? _highlightMaterial : _pegMaterial;
    }
}
