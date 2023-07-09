using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private MovementController _movementController;
    
    private Button _up;
    private Button _down;
    private Button _left;
    private Button _right;
    private Button _retry;
    private Label _winLoseText;

    private void Awake()
    {
        _movementController = FindObjectOfType<MovementController>();
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        _up = root.Q("Up") as Button;
        _down = root.Q("Down") as Button;
        _left = root.Q("Left") as Button;
        _right = root.Q("Right") as Button;
        _retry = root.Q("Reset") as Button;
        _winLoseText = root.Q("WinLoseText") as Label;
        
        _up.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.OnMovementInputReceived(CompassDirection.North);
        });
        
        _down.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.OnMovementInputReceived(CompassDirection.South);
        });
        
        _left.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.OnMovementInputReceived(CompassDirection.West);
        });
        
        _right.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.OnMovementInputReceived(CompassDirection.East);
        });
        
        _retry.RegisterCallback<ClickEvent>((click) =>
        {
            //levelLoader.ResetLevel();
        });
    }

    private void OnDisable()
    {
        // TODO: Unregister here
        
    }

    public void OnLevelCompleted()
    {
        _winLoseText.text = "Good Night!";
    }
    
    public void OnLevelFailed()
    {
        _winLoseText.text = "Oops!";
    }
}
