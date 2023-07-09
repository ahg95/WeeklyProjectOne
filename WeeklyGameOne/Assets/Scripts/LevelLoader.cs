using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField]
    private string _globalLightTag;

    [SerializeField]
    private Color _nightColor;

    [Header("Scriptable Object Variables")]
    [SerializeField]
    private IntVariable _levelIndex;
    [SerializeField]
    private BoolVariable _sunriseAnimationEnabled;
    
    private Light2D _globalLight;
    private InputActions _inputActions;
    private bool _isLoading;

    private void Awake()
    {
        _globalLight = GameObject.FindWithTag(_globalLightTag).GetComponent<Light2D>();

        _inputActions = new InputActions();
        _inputActions.Gameplay.ResetLevel.Enable();
        _inputActions.Gameplay.SkipLevel.Enable();
        _inputActions.Gameplay.QuitApplication.Enable();
        
        _inputActions.Gameplay.ResetLevel.performed += (context) =>
        {
            if (context.performed && !_isLoading)
                ResetLevel();
        };

        _inputActions.Gameplay.SkipLevel.performed += (context) =>
        {
            if (context.performed && !_isLoading)
            {
                _isLoading = true;
                
                Debug.Log("Skip level");
                
                Debug.Log("Runtime value before: " + _levelIndex.RuntimeValue);
                
                _levelIndex.RuntimeValue += 1;
                
                Debug.Log("Runtime value after: " + _levelIndex.RuntimeValue);


                SceneManager.LoadScene(_levelIndex.RuntimeValue);
            }
        };

        _inputActions.Gameplay.QuitApplication.performed += (context) =>
        {
            Application.Quit();
        };
        
        _globalLight.color = _nightColor;

        StartCoroutine(SunriseAnimation());
    }

    public IEnumerator SunriseAnimation()
    {
        const float animationTime = 1;
        float animationTimeLeft = animationTime;

        while (animationTimeLeft > 0)
        {
            animationTimeLeft -= Time.deltaTime;
            _globalLight.color = Color.Lerp(Color.white, _nightColor, animationTimeLeft / animationTime);
            yield return null;
        }
    }
    
    public void LoadNextLevel()
    {
        if (_isLoading)
            return;
        
        StartCoroutine(TransitionToNextLevel());
    }
    
    private IEnumerator TransitionToNextLevel()
    {
        _isLoading = true;
        
        yield return new WaitForSeconds(2);

        const float nightAnimationTime = 2;
        float nightAnimationTimeLeft = nightAnimationTime;

        while (nightAnimationTimeLeft > 0)
        {
            nightAnimationTimeLeft -= Time.deltaTime;
            
            _globalLight.color = Color.Lerp(_nightColor, Color.white, nightAnimationTimeLeft / nightAnimationTime);
            
            yield return null;
        }
        
        Debug.Log("Transition to next level");
                
        Debug.Log("Runtime value before: " + _levelIndex.RuntimeValue);
        
        _levelIndex.RuntimeValue += 1;
        
        Debug.Log("Runtime value after: " + _levelIndex.RuntimeValue);

        SceneManager.LoadScene(_levelIndex.RuntimeValue);
    }
        
    public void ResetLevel()
    {
        if (_isLoading)
            return;

        _isLoading = true;

        var currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
