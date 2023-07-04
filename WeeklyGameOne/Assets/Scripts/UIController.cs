using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private MovementController _movementController;
    
    private Button _up;
    private Button _down;
    private Button _left;
    private Button _right;
    private Button _retry;
    
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        _up = root.Q("Up") as Button;
        _down = root.Q("Down") as Button;
        _left = root.Q("Left") as Button;
        _right = root.Q("Right") as Button;
        _retry = root.Q("Reset") as Button;
        
        _up.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.Move(CompassDirection.North);
        });
        
        _down.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.Move(CompassDirection.South);
        });
        
        _left.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.Move(CompassDirection.West);
        });
        
        _right.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.Move(CompassDirection.East);
        });
        
        _retry.RegisterCallback<ClickEvent>((click) =>
        {
            _movementController.ResetLevel();
        });
    }

    private void OnDisable()
    {
        // Unregister here
    }
}
