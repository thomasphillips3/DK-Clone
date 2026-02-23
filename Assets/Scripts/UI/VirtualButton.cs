using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class VirtualButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
{
    [InputControl(layout = "Button")]
    [SerializeField]
    private string m_ControlPath;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    private bool isPressed = false;
    
    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
    
    void Start()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
        
        // Hide on non-touch devices
        #if !UNITY_ANDROID && !UNITY_IOS
        if (!Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
        #endif
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        
        if (buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }
        
        SendValueToControl(1.0f);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
        
        SendValueToControl(0.0f);
    }
    
    public bool IsPressed()
    {
        return isPressed;
    }
}

