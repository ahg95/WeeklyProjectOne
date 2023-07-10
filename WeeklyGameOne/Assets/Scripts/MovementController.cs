using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField]
    private string _groundTilemapTag;

    [Header("Tiles")]
    [SerializeField]
    private TileBase _groundTile;
    [SerializeField]
    private TileBase _obstacleTile;
    [SerializeField]
    private TileBase _dangerTile;
    [SerializeField]
    private TileBase _mudTile;
    
    [Header("Events")]
    [SerializeField]
    private GameEvent _levelCompleted;
    [SerializeField]
    private GameEvent _levelFailed;

    private GridLayout _grid;
    private Tilemap _groundTilemap;
    
    private InputActions _inputActions;
    private List<Moveable> _moveables;
    private int _nrOfActiveAnimations;

    private bool _isFirstTickAfterInput;
    
    private void Awake()
    {
        _grid = FindObjectOfType<GridLayout>();
        _groundTilemap = GameObject.FindWithTag(_groundTilemapTag).GetComponent<Tilemap>();
        
        _inputActions = new InputActions();
        _inputActions.Gameplay.Movement.Enable();
        
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
                
                // If the movable has a destination, then check if it is reached or if it is in danger. If so, check if the game is won.
                if (sendingMoveable._Destination != null)
                {
                    var position = sendingMoveable.transform.position;
                    var gridPosition = _grid.WorldToCell(position);
                    var tile = _groundTilemap.GetTile(gridPosition);

                    if (tile == _dangerTile)
                    {
                        sendingMoveable._IsControllable = false;
                        sendingMoveable._IsMoving = false;
                        sendingMoveable.Sink();
                        _levelFailed.Raise();
                    }
                    else if (tile == sendingMoveable._Destination)
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
                            _levelCompleted.Raise();
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
                            Tick();
                            break;
                        }
                    }
                }
            };
        }
    }

    private void Update()
    {
        var movementInput = _inputActions.Gameplay.Movement.ReadValue<Vector2>();

        if (movementInput.y > 0)
            OnMovementInputReceived(CompassDirection.North);
        else if (movementInput.x < 0)
            OnMovementInputReceived(CompassDirection.West);
        else if (movementInput.y < 0)
            OnMovementInputReceived(CompassDirection.South);
        else if (movementInput.x > 0)
            OnMovementInputReceived(CompassDirection.East);
    }

    public void OnMovementInputReceived(CompassDirection movementDirection)
    {
        if (_nrOfActiveAnimations > 0 || movementDirection == CompassDirection.None)
            return;

        // Set movement of all controllable moveables
        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            if (moveable._IsControllable)
            {
                moveable.SetOrientation(movementDirection);
                moveable._IsMoving = true;
            }
            
        }
        
        // Check for each moveable that can be pushed if it is pushed
        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            if (!moveable._IsPushable)
                continue;
            
            var originalOrientation = moveable._Orientation;
                
            var currentPosition = moveable.transform.position;
            var currentGridPosition = _grid.WorldToCell(currentPosition);
            var northGridPosition = currentGridPosition + CompassDirectionUtil.CompassDirectionToIntVector(CompassDirection.North);
            var eastGridPosition = currentGridPosition + CompassDirectionUtil.CompassDirectionToIntVector(CompassDirection.East);
            var southGridPosition  = currentGridPosition + CompassDirectionUtil.CompassDirectionToIntVector(CompassDirection.South);
            var westGridPosition = currentGridPosition + CompassDirectionUtil.CompassDirectionToIntVector(CompassDirection.West);
            
            // Check the surrounding tiles
            for (int j = 0; j < _moveables.Count; j++)
            {
                if (j == i)
                    continue;
                var otherMoveable = _moveables[j];
                
                if (!otherMoveable._IsMoving || !otherMoveable._IsControllable || !otherMoveable._CanPush)
                    continue;

                var otherMoveablePosition = otherMoveable.transform.position;
                var otherMoveableGridPosition = _grid.WorldToCell(otherMoveablePosition);

                if ((otherMoveableGridPosition == northGridPosition &&
                     otherMoveable._Orientation == CompassDirection.South)
                    || (otherMoveableGridPosition == eastGridPosition &&
                        otherMoveable._Orientation == CompassDirection.West)
                    || (otherMoveableGridPosition == southGridPosition &&
                        otherMoveable._Orientation == CompassDirection.North)
                    || (otherMoveableGridPosition == westGridPosition &&
                        otherMoveable._Orientation == CompassDirection.East))
                {
                    if (moveable._IsMoving)
                    {
                        moveable._IsMoving = false;
                        moveable.SetOrientation(originalOrientation);
                        break;
                    }
                    else
                    {
                        moveable._IsMoving = true;
                        moveable.SetOrientation(otherMoveable._Orientation);
                    }
                }
            }
        }
            
            
        
        _isFirstTickAfterInput = true;
        
        Tick();

        _isFirstTickAfterInput = false;
    }
    
    private void Tick()
    {
        // Calculate the provisional next positions of all moveables on the grid
        var nextGridPositions = new List<Vector3Int>(_moveables.Count);
        
        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            var currentPosition = moveable.transform.position;
            var currentGridPosition = _grid.WorldToCell(currentPosition);
            
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
                    var currentGridPosition = _grid.WorldToCell(currentPosition);

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
                var currentGridPosition = _grid.WorldToCell(currentPosition);

                nextGridPositions[index] = currentGridPosition;
                nextGridPositionsChanged = true;
            }
            
        } while (nextGridPositionsChanged);
        
        // Actually move all moveables that are still moving
        for (int i = 0; i < _moveables.Count; i++)
        {
            var moveable = _moveables[i];

            if (!moveable._IsMoving)
                continue;

            if (moveable._SlidesOnMud)
            {
                var nextGridPosition = nextGridPositions[i];
                var nextTile = _groundTilemap.GetTile(nextGridPosition);
                moveable._IsMoving = nextTile == _mudTile;

                if (_isFirstTickAfterInput)
                {
                    moveable.Move();
                }
                else
                {
                    var currentPosition = moveable.transform.position;
                    var currentGridPosition = _grid.WorldToCell(currentPosition);
                    var currentTile = _groundTilemap.GetTile(currentGridPosition);

                    if (currentTile == _mudTile)
                    {
                        moveable.Slide();
                    }
                    else
                    {
                        moveable.Move();
                    }
                }
            } else
            {
                moveable._IsMoving = moveable._KeepsMomentum;
                moveable.Move();
            }

        }
    }
}
