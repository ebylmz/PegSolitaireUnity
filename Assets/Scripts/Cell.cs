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
    private void Awake() {
        _rend = GetComponent<Renderer>();
    }

    public void Init(Vector2Int pos, CellValue val) {
        this.name = pos.ToString();
        _pos = pos;
        SetValue(val);  
    }

    public void SetPosition(Vector2Int pos) {_pos = pos;}
    public void SetPosition(int x, int y) {
        _pos.x = x;
        _pos.y = y;
    }

    public Vector2Int GetPosition() {return _pos;}

    public void SetValue(CellValue v) {
        _value = v;
        switch (v) {
            case CellValue.PEG: _rend.material = _pegMaterial; break;
            case CellValue.EMPTY: _rend.material = _emptyMaterial; break;
            case CellValue.SELECTED: _rend.material = _selectedMaterial; break;
            case CellValue.PREDICTED: _rend.material = _predictedMaterial; break;
        }        
    }
    public CellValue GetValue() {return _value;}

    private void OnMouseEnter() {
        if (GetValue() == CellValue.PEG)
            _rend.material = _selectedMaterial;
    }

    // private void OnMouseOver() {
    // if (GetValue() == CellValue.PEG)
    //     _rend.material = __highlightMaterial;
    // }

    private void OnMouseExit() {
        if (GetValue() != CellValue.EMPTY)
            _rend.material = _pegMaterial;
    }
}
