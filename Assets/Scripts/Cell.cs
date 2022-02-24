using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    [SerializeField] private Material _selectedMaterial;
    [SerializeField] private Material _predictedMaterial;
    [SerializeField] private Material _pegMaterial;
    [SerializeField] private Material _emptyMaterial;
    private Vector2Int _loc;
    private Renderer _rend;
    private CellValue _value;

    public enum CellValue {Peg, Empty, Selected, Predicted};

    // Start is called before the first frame update
    private void Start() {
        Debug.Log("Start method called");
        // _rend = GetComponent<Renderer>();
        // __isSelected = false;
        // GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    private void Update() {

    }

    public void Init(string name, Vector2Int loc, CellValue val) {
        this.name = name;
        _loc = loc;
        _rend = GetComponent<Renderer>();
        setValue(val);
    }

    public void setLocation(Vector2Int loc) {_loc = loc;}
    public void setLocation(int x, int y) {
        _loc.x = x;
        _loc.y = y;
    }

    public Vector2Int getLocation() {return _loc;}

    public void setValue(CellValue v) {
        _value = v;
        switch (v) {
            case CellValue.Peg: _rend.material = _pegMaterial; break;
            case CellValue.Empty: _rend.material = _emptyMaterial; break;
            case CellValue.Selected: _rend.material = _selectedMaterial; break;
            case CellValue.Predicted: _rend.material = _predictedMaterial; break;
        }        
    }
    public CellValue getValue() {return _value;}

    private void OnMouseEnter() {
        if (getValue() == CellValue.Peg)
            _rend.material = _selectedMaterial;
    }

    // private void OnMouseOver() {
    // if (getValue() == CellValue.Peg)
    //     _rend.material = __highlightMaterial;
    // }

    private void OnMouseExit() {
        if (getValue() != CellValue.Empty)
            _rend.material = _pegMaterial;
    }
}
