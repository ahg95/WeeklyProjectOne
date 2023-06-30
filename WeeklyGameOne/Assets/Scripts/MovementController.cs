using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private GridLayout _gridLayout;
    
    [SerializeField]
    private Tilemap _groundTilemap;

    [SerializeField]
    private List<Transform> _characters;

    private List<Vector3Int> _targetGridPositions = new(3);
    
// Reference all characters
// Check the input
// Move them in the corresponding direction
// But only if there is nothing in the way

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _characters.Count; i++)
        {
            _targetGridPositions.Add(new());
        }
    }

    // Update is called once per frame
    void Update()
    {
        var movementVector = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            movementVector = Vector3.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            movementVector = Vector3.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            movementVector = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            movementVector = Vector3.right;

        if (movementVector == Vector3.zero)
            return;

        // Calculate target grid positions of characters
        for (int i = 0; i < _characters.Count; i++)
        {
            var character = _characters[i];
            
            var targetPosition = character.position + movementVector;
            var targetGridPosition = _gridLayout.WorldToCell(targetPosition);
            var targetTile = _groundTilemap.GetTile(targetGridPosition);

            if (targetTile == null)
            {
                targetPosition = character.position;
                targetGridPosition = _gridLayout.WorldToCell(targetPosition);
            }

            _targetGridPositions[i] = targetGridPosition;
        }

        // Move characters if the target grid positions do not conflict
        for (int i = 0; i < _targetGridPositions.Count; i++)
        {
            var character = _characters[i];
            var targetGridPosition = _targetGridPositions[i];

            bool tileIsCccupied = false;

            for (int j = 0; j < _targetGridPositions.Count; j++)
            {
                if (i == j)
                    continue;

                var otherGridPosition = _targetGridPositions[j];

                if (targetGridPosition == otherGridPosition)
                {
                    tileIsCccupied = true;
                    break;
                }
            }

            if (!tileIsCccupied)
            {
                character.position = _gridLayout.CellToWorld(targetGridPosition);
            }
        }
    }
}
