using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private int _width;
    private int _height;
    [SerializeField] private Tile _tilePrefab; 
    [SerializeField] private Camera _camera;

    // Start is called before the first frame update
    void Start() {
        _width = 9;
        _height = 9;
        GenerateGrid();
    }

    void Update() {
        //! seperate function
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo)) {
                Tile tile = hitInfo.collider.gameObject.GetComponent<Tile>();
                tile.setSelected(true);
                
            }
        }

        // // when player press the button
        // if (Input.GetKeyDown(KeyCode.RightArrow))
        //     Debug.Log("Right arrow key is pressed");
        // if (Input.GetKey(KeyCode.RightArrow))
        //     Debug.Log("Right arrow key is still pressing");
        // if (Input.GetKeyUp(KeyCode.RightArrow))
        //     Debug.Log("Right arrow key has beeen pressed");

        // if (Input.GetMouseButton(0))
        //     Debug.Log("Mouse left button is pressed");
        // if (Input.GetMouseButton(0))
        //     Debug.Log("Mouse left button is still pressing");
        // if (Input.GetMouseButton(0))
        //     Debug.Log("Mouse left button has been pressed");
    }

    private void GenerateGrid() {
        for (int x = 0, n = 2, m = 5; x < 2; ++x, --n, ++m)
            for (int y = n; y < m; ++y) {
                Vector2 position = new Vector2(x, y); //! vector2
                Tile tile = Instantiate(_tilePrefab, position, Quaternion.identity);
                tile.name = position.ToString(); // tile.name = $"Tile {x} {y}";
            }

        for (int x = 2; x < 5; ++x)
            for (int y = 0; y < 7; ++y) {
                Vector2 position = new Vector2(x, y);
                Tile tile = Instantiate(_tilePrefab, position, Quaternion.identity);
                tile.name = position.ToString();
            }

        for (int x = 5, n = 1, m = 6; x < 7; ++x, ++n, --m)
            for (int y = n; y < m; ++y) {
                Vector2 position = new Vector2(x, y);
                Tile tile = Instantiate(_tilePrefab, position, Quaternion.identity);
                tile.name = position.ToString();
            }

        // __board[2][3].setCellType(CellType.EMPTY);

        // change the position of the camera as center of the grid
        _camera.transform.position = new Vector3((float) _width / 2 - 0.5f, (float) _height / 2 - 0.5f, -10);
    }
}
