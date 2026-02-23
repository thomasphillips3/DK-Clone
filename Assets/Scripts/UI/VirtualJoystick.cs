using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class VirtualJoystick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Joystick Settings")]
    [SerializeField] private float movementRange = 50f;
    [SerializeField] private bool dynamicJoystick = false;
    
    [Header("Visuals")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    
    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;
    
    private Vector2 startPosition;
    private Vector2 inputVector;
    private Canvas canvas;
    private Camera mainCamera;
    private bool isActive = false;
    
    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }
    
    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            mainCamera = canvas.worldCamera;
        }
        
        startPosition = background.anchoredPosition;
        
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
        isActive = true;
        
        if (dynamicJoystick)
        {
            background.position = eventData.position;
            handle.anchoredPosition = Vector2.zero;
        }
        
        OnDrag(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isActive = false;
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        
        if (dynamicJoystick)
        {
            background.anchoredPosition = startPosition;
        }
        
        SendValueToControl(Vector2.zero);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive)
            return;
        
        Vector2 position = RectTransformUtility.WorldToScreenPoint(mainCamera, background.position);
        Vector2 radius = background.sizeDelta / 2f;
        
        inputVector = (eventData.position - position) / (radius * canvas.scaleFactor);
        
        // Clamp to unit circle
        if (inputVector.magnitude > 1f)
        {
            inputVector = inputVector.normalized;
        }
        
        // Update handle position
        handle.anchoredPosition = inputVector * movementRange;
        
        // Send input to Input System
        SendValueToControl(inputVector);
    }
    
    public Vector2 GetInputVector()
    {
        return inputVector;
    }
}

