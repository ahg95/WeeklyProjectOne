using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Moveable : MonoBehaviour
{
    public bool _IsControllable;
    public CompassDirection _Orientation;
    
    [Header("Destination")]
    public TileBase _Destination;
    public TileBase _OccupiedDestination;
    
    [HideInInspector]
    public bool _IsMoving;
    
    [Header("Sprites")]
    [SerializeField]
    private Sprite _front;
    [SerializeField]
    private Sprite _back;
    [SerializeField]
    private Sprite _right;
    
    [Header("Other References")]
    [SerializeField]
    private SpriteRenderer _renderer;
    [SerializeField]
    private GameObject _visualsContainer;
    [SerializeField]
    private Transform _characterSpriteContainer;
    
    private const float _animationTime = 0.5f;
    private float _animationTimeLeft;

    public event EventHandler _animationStarted;
    public event EventHandler _animationFinished;

    private void Update()
    {
        if (_animationTimeLeft <= 0)
            return;
        
        var delta = Mathf.Min(Time.deltaTime, _animationTimeLeft);

        var movement = CompassDirectionUtil.CompassDirectionToVector(_Orientation) / _animationTime;

        transform.position += movement * delta;

        _animationTimeLeft -= Time.deltaTime;
        _animationTimeLeft = Mathf.Max(0, _animationTimeLeft);
        
        _characterSpriteContainer.localPosition = 0.3f * EasingFunctions.EaseOutCircle(-Mathf.Abs((_animationTimeLeft - _animationTime / 2) / (_animationTime / 2)) + 1) * Vector3.up;

        if (_animationTimeLeft <= 0)
            _animationFinished.Invoke(this, EventArgs.Empty);
            
    }

    public void Move()
    {
        _animationStarted.Invoke(this, EventArgs.Empty);

        _animationTimeLeft = _animationTime;
    }

    public void Sink()
    {
        
    }

    public void Hide()
    {
        _visualsContainer.SetActive(false);
    }

    public void Show()
    {
        _visualsContainer.SetActive(true);
    }
    
    public void SetOrientation(CompassDirection orientation)
    {
        _Orientation = orientation;

        AdjustSpriteToOrientation();
    }
    
    private void OnValidate()
    {
        if (!_renderer)
            return;
        
        AdjustSpriteToOrientation();
    }

    private void AdjustSpriteToOrientation()
    {
        switch (_Orientation)
        {
            case CompassDirection.North:
                _renderer.sprite = _back;
                _renderer.flipX = false;
                break;
            case CompassDirection.East:
                _renderer.sprite = _right;
                _renderer.flipX = false;
                break;
            case CompassDirection.South:
                _renderer.sprite = _front;
                _renderer.flipX = false;
                break;
            case CompassDirection.West:
                _renderer.sprite = _right;
                _renderer.flipX = true;
                break;
        }
    }
}
