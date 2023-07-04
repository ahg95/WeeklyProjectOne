using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private GridLayout _gridLayout;
    
    [Header("Tilemaps")]
    [SerializeField]
    private Tilemap _groundTilemap;

    [Header("Tiles")]
    [SerializeField]
    private TileBase _groundTile;
    [SerializeField]
    private TileBase _obstacleTile;
    [SerializeField]
    private TileBase _dangerTile;

    private List<Moveable> _moveables;
    private int _nrOfActiveAnimations;
    private int _currentLevelIndex;
    
    private void Awake()
    {
        _moveables = FindObjectsOfType<Moveable>().ToList();

        foreach (var character in _moveables)
        {
            character._animationStarted += (sender, args) =>
            {
                _nrOfActiveAnimations++;
            };
            
            character._animationFinished += (sender, eventArgs) =>
            {
                _nrOfActiveAnimations--;
                
                var sendingMoveable = sender as Moveable;

                // If the movable has a destination, then check if it is reached. If so, check if the game is won.
                if (sendingMoveable._Destination != null)
                {
                    var position = sendingMoveable.transform.position;
                    var gridPosition = _gridLayout.WorldToCell(position);
                    var tile = _groundTilemap.GetTile(gridPosition);

                    if (tile == sendingMoveable._Destination)
                    {
                        sendingMoveable.Hide();
                        sendingMoveable._IsControllable = false;
                        sendingMoveable._IsMoving = false;
                        _groundTilemap.SetTile(gridPosition, sendingMoveable._OccupiedDestination);
                        
                        // Check if the game is won
                        bool gameIsWon = true;
                        
                        foreach (var moveable in _moveables)
                        {
                            if (moveable._IsControllable && moveable._Destination != null)
                            {
                                gameIsWon = false;
                                break;
                            }
                        }

                        if (gameIsWon)
                        {
                            Debug.Log("Game is won!");
                        }
                    }
                }

                
                // If all animations have been played, and there is still some object that has movement, then move all the moveables
                if (_nrOfActiveAnimations == 0)
                {
                    foreach (var moveable in _moveables)
                    {
                        if (moveable._IsMoving)
                        {
                            MoveMoveables();
                            break;
                        }
                    }
                }
            };
        }
    }

    
// Update is called once per frame
    void Update()
    {
        var inputDirection = CompassDirection.None;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            inputDirection = CompassDirection.North;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            inputDirection = CompassDirection.South;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            inputDirection = CompassDirection.West;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            inputDirection = CompassDirection.East;
        }
        
        Move(inputDirection);
    }

    public void ProgressToNextLevel()
    {
        _currentLevelIndex++;
        SceneManager.LoadScene(_currentLevelIndex);
    }
    
    public void ResetLevel()
    {
        SceneManager.LoadScene(_currentLevelIndex);
    }
    
    public void Move(CompassDirection movementDirection)
    {
        if (_nrOfActiveAnimations > 0 || movementDirection == CompassDirection.None)
            return;

        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            if (moveable._IsControllable)
            {
                moveable.SetOrientation(movementDirection);
                moveable._IsMoving = true;
            }
        }
        
        MoveMoveables();
    }
    
    private void MoveMoveables()
    {
        // Calculate the provisional next positions of all moveables on the grid
        var nextGridPositions = new List<Vector3Int>(_moveables.Count);
        
        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            var currentPosition = moveable.transform.position;
            var currentGridPosition = _gridLayout.WorldToCell(currentPosition);
            
            if (moveable._IsMoving)
            {
                var nextGridPosition = currentGridPosition + CompassDirectionUtil.CompassDirectionToIntVector(moveable._Orientation);
                nextGridPositions.Add(nextGridPosition);
            }
            else
            {
                nextGridPositions.Add(currentGridPosition);
            }
        }
        
        
        
        // Check for each moveable if their next position is valid. 
        bool nextGridPositionsChanged;
        
        do
        {
            nextGridPositionsChanged = false;
            
            
            // Check if there are any obstacles on the nextGridPosition that prevent moving
            for (int i = 0; i < _moveables.Count; i++)
            {
                var nextGridPosition = nextGridPositions[i];
                var tile = _groundTilemap.GetTile(nextGridPosition);

                if (tile == null || tile == _obstacleTile)
                {
                    var moveable = _moveables[i];
                    moveable._IsMoving = false;
                    
                    var currentPosition = moveable.transform.position;
                    var currentGridPosition = _gridLayout.WorldToCell(currentPosition);

                    nextGridPositions[i] = currentGridPosition;
                    nextGridPositionsChanged = true;
                }
            }
            
            
            // Identify moveables that have the same next position
            var overlappingMoveIndices = new HashSet<int>();
            
            for (int i = 0; i < _moveables.Count; i++)
            {
                var nextGridPosition = nextGridPositions[i];


                for (int j = i + 1; j < _moveables.Count; j++)
                {
                    var otherNextGridPosition = nextGridPositions[j];

                    if (nextGridPosition == otherNextGridPosition)
                    {
                        overlappingMoveIndices.Add(i);
                        overlappingMoveIndices.Add(j);
                        
                        // We can stop checking overlaps with i since other overlaps will be detected when we check j
                        break;
                    }
                }
            }

            
            // Moveables that have the same next position should not move
            foreach (var index in overlappingMoveIndices)
            {
                var moveable = _moveables[index];
                moveable._IsMoving = false;

                var currentPosition = moveable.transform.position;
                var currentGridPosition = _gridLayout.WorldToCell(currentPosition);

                nextGridPositions[index] = currentGridPosition;
                nextGridPositionsChanged = true;
            }
            
        } while (nextGridPositionsChanged);

        
        
        // Actually move all moveables that are still moving
        foreach (var moveable in _moveables)
        {
            if (moveable._IsMoving)
            {
                moveable._IsMoving = false;
                moveable.Move();
            }

        }
    }
}
