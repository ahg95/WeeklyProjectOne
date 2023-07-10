using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Moveable : MonoBehaviour
{
    [Header("Properties")]
    public bool _IsControllable;
    public bool _SlidesOnMud;
    public bool _IsPushable;
    public bool _CanPush;
    public bool _KeepsMomentum;
    
    public CompassDirection _Orientation { get; private set; }
    
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
    private SpriteRenderer _entityRenderer;
    [SerializeField]
    private SpriteRenderer _shadowRenderer;
    [SerializeField]
    private GameObject _visualsContainer;
    [SerializeField]
    private Transform _entityContainer;

    public event EventHandler _animationStarted;
    public event EventHandler _animationFinished;

    public void Move()
    {
        _animationStarted.Invoke(this, EventArgs.Empty);

        StartCoroutine(MoveAnimation());
    }

    private IEnumerator MoveAnimation()
    {
        const float animationTime = 0.5f;
        float animationTimeLeft = animationTime;

        while (animationTimeLeft > 0)
        {
            var delta = Mathf.Min(Time.deltaTime, animationTimeLeft);

            var movement = CompassDirectionUtil.CompassDirectionToVector(_Orientation);

            transform.position += delta / animationTime * movement;
            
            animationTimeLeft -= delta;
            
            _entityContainer.localPosition = 0.3f * EasingFunctions.EaseOutCircle(-Mathf.Abs((animationTimeLeft - animationTime / 2) / (animationTime / 2)) + 1) * Vector3.up;
            _shadowRenderer.color = Color.Lerp(Color.white, Color.clear, Mathf.Abs(_entityContainer.localPosition.y));
            
            yield return null;
        }
        
        _animationFinished.Invoke(this, EventArgs.Empty);
    }
    
    public void Sink()
    {
        _animationStarted.Invoke(this, EventArgs.Empty);

        StartCoroutine(SinkAnimation());
    }

    public void Slide()
    {
        _animationStarted.Invoke(this, EventArgs.Empty);
        
        StartCoroutine(SlideAnimation());
    }

    private IEnumerator SlideAnimation()
    {
        const float animationTime = 0.5f;
        float animationTimeLeft = animationTime;

        while (animationTimeLeft > 0)
        {
            var delta = Mathf.Min(Time.deltaTime, animationTimeLeft);

            var movement = CompassDirectionUtil.CompassDirectionToVector(_Orientation);

            transform.position += delta / animationTime * movement;
            
            animationTimeLeft -= delta;

            yield return null;
        }
        
        _animationFinished.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator SinkAnimation()
    {
        const float animationTime = 0.2f;
        float animationTimeLeft = animationTime;

        while (animationTimeLeft > 0)
        {
            var delta = Mathf.Min(Time.deltaTime, animationTimeLeft);

            _entityContainer.position += delta / animationTime * Vector3.down;
            _shadowRenderer.color = Color.Lerp(Color.white, Color.clear, Mathf.Abs(_entityContainer.localPosition.y));
            
            animationTimeLeft -= delta;

            yield return null;
        }
        
        Hide();
        
        _animationFinished.Invoke(this, EventArgs.Empty);
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
        if (!_entityRenderer)
            return;
        
        AdjustSpriteToOrientation();
    }

    private void AdjustSpriteToOrientation()
    {
        switch (_Orientation)
        {
            case CompassDirection.North:
                _entityRenderer.sprite = _back;
                _entityRenderer.flipX = false;
                break;
            case CompassDirection.East:
                _entityRenderer.sprite = _right;
                _entityRenderer.flipX = false;
                break;
            case CompassDirection.South:
                _entityRenderer.sprite = _front;
                _entityRenderer.flipX = false;
                break;
            case CompassDirection.West:
                _entityRenderer.sprite = _right;
                _entityRenderer.flipX = true;
                break;
        }
    }
}
