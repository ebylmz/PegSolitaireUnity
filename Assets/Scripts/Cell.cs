using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _predictedMaterial;
    [SerializeField] private Material _pegMaterial;
    [SerializeField] private Material _emptyMaterial;
    private Vector2Int _pos; // position of the cell
    private Renderer _rend;
    private CellValue _value;

    public enum CellValue {PEG, EMPTY, SELECTED, PREDICTED};

    // Start is called before the first frame update
    private void Start() {
        // _rend = GetComponent<Renderer>();
        // __isSelected = false;
        // GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    private void Update() {

    }

    public void Init(Vector2Int pos, CellValue val) {
        this.name = pos.ToString();
        _pos = pos;
        // first get the renderer since function setValue uses renderer
        _rend = GetComponent<Renderer>();
        setValue(val);  
    }

    public void setPosition(Vector2Int pos) {_pos = pos;}
    public void setPosition(int x, int y) {
        _pos.x = x;
        _pos.y = y;
    }

    public Vector2Int getPosition() {return _pos;}

    public void setValue(CellValue v) {
        _value = v;
        switch (v) {
            case CellValue.PEG: _rend.material = _pegMaterial; break;
            case CellValue.EMPTY: _rend.material = _emptyMaterial; break;
            case CellValue.SELECTED: _rend.material = _selectedMaterial; break;
            case CellValue.PREDICTED: _rend.material = _predictedMaterial; break;
        }        
    }
    public CellValue getValue() {return _value;}

    private void OnMouseEnter() {
        if (getValue() == CellValue.PEG)
            _rend.material = _selectedMaterial;
    }

    // private void OnMouseOver() {
    // if (getValue() == CellValue.PEG)
    //     _rend.material = __highlightMaterial;
    // }

    private void OnMouseExit() {
        if (getValue() != CellValue.EMPTY)
            _rend.material = _pegMaterial;
    }
}
