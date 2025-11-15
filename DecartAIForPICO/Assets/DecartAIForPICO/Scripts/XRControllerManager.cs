using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class XRControllerManager : MonoBehaviour
{
    public static XRControllerManager Instance;

    #region XR input configuration file
    [Header("Menu key (left controller only)")]
    public InputActionReference menuBtnLeft;
    [Header("Left controller X key")]
    public InputActionReference primaryBtnLeft;
    public InputActionReference primaryBtnPressedLeft;
    public InputActionReference primaryBtnUpLeft;
    [Header("Right controller A key")]
    public InputActionReference primaryBtnRight;
    public InputActionReference primaryBtnPressedRight;
    public InputActionReference primaryBtnUpRight;
    [Header("Left controller Grip key")]
    public InputActionReference gripBtnLeft;
    public InputActionReference gripBtnPressedLeft;
    public InputActionReference gripBtnUpLeft;
    [Header("Right controller Grip key")]
    public InputActionReference gripBtnRight;
    public InputActionReference gripBtnPressedRight;
    public InputActionReference gripBtnUpRight;
    [Header("Left controller Trigger key")]
    public InputActionReference triggerBtnLeft;
    [Header("Right controller Trigger key")]
    public InputActionReference triggerBtnRight;
    [Header("Left controller Y key")]
    public InputActionReference secondaryBtnLeft;
    public InputActionReference secondaryBtnPressedLeft;
    public InputActionReference secondaryBtnUpLeft;
    [Header("Right controller B key")]
    public InputActionReference secondaryBtnRight;
    public InputActionReference secondaryBtnPressedRight;
    public InputActionReference secondaryBtnUpRight;
    #endregion

    #region XR input callback
    private Action<InputAction.CallbackContext> menuInputLeft;
    private Action<InputAction.CallbackContext> triggerInputLeft;
    private Action<InputAction.CallbackContext> triggerInputRight;
    private Action<InputAction.CallbackContext> gripInputLeft;
    private Action<InputAction.CallbackContext> gripInputPressedLeft;
    private Action<InputAction.CallbackContext> gripInputUpLeft;
    private Action<InputAction.CallbackContext> gripInputRight;
    private Action<InputAction.CallbackContext> gripInputPressedRight;
    private Action<InputAction.CallbackContext> gripInputUpRight;
    private Action<InputAction.CallbackContext> primaryInputLeft;
    private Action<InputAction.CallbackContext> primaryInputPressedLeft;
    private Action<InputAction.CallbackContext> primaryInputUpLeft;
    private Action<InputAction.CallbackContext> primaryInputRight;
    private Action<InputAction.CallbackContext> primaryInputPressedRight;
    private Action<InputAction.CallbackContext> primaryInputUpRight;
    private Action<InputAction.CallbackContext> secondaryInputLeft;
    private Action<InputAction.CallbackContext> secondaryInputPressedLeft;
    private Action<InputAction.CallbackContext> secondaryInputUpLeft;
    private Action<InputAction.CallbackContext> secondaryInputRight;
    private Action<InputAction.CallbackContext> secondaryInputPressedRight;
    private Action<InputAction.CallbackContext> secondaryInputUpRight;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    #region Bind button - For external calling
    public void BingingMenuHotKey(Action<InputAction.CallbackContext> downAction)
    {
        if(menuInputLeft != null)
        {
            UnBingingMenuInputActionLeft();
        }
        menuInputLeft = downAction;
        if (menuBtnLeft != null && menuInputLeft != null)
        {
            menuBtnLeft.action.Enable();
            menuBtnLeft.action.performed += menuInputLeft;
        }
    }

    public void BingingTriggerHotKey(bool isLeftController, Action<InputAction.CallbackContext> downAction)
    {
        if (isLeftController)
        {
            if (triggerInputLeft != null)
            {
                UnBingingTriggerInputActionLeft();
            }
            triggerInputLeft = downAction;
            if (triggerBtnLeft != null && triggerInputLeft != null)
            {
                triggerBtnLeft.action.Enable();
                triggerBtnLeft.action.performed += triggerInputLeft;
            }
        }
        else
        {
            if (triggerInputRight != null)
            {
                UnBingingTriggerInputActionRight();
            }
            triggerInputRight = downAction;
            if (triggerBtnRight != null && triggerInputRight != null)
            {
                triggerBtnRight.action.Enable();
                triggerBtnRight.action.performed += triggerInputRight;
            }
        }
    }

    public void BingingGripHotKey(bool isLeftController, Action<InputAction.CallbackContext> downAction, Action<InputAction.CallbackContext> pressedAction, Action<InputAction.CallbackContext> upAction)
    {
        if (isLeftController)
        {
            if (gripInputLeft != null ||
                gripInputPressedLeft != null ||
                gripInputUpLeft != null)
            {
                UnBingingGripInputActionLeft();
            }
            gripInputLeft = downAction;
            gripInputPressedLeft = pressedAction;
            gripInputUpLeft = upAction;
            if (gripBtnLeft != null && gripInputLeft != null)
            {
                gripBtnLeft.action.Enable();
                gripBtnLeft.action.performed += gripInputLeft;
            }
            if (gripBtnPressedLeft != null && gripInputPressedLeft != null)
            {
                gripBtnPressedLeft.action.Enable();
                gripBtnPressedLeft.action.performed += gripInputPressedLeft;
            }
            if (gripBtnUpLeft != null && gripInputUpLeft != null)
            {
                gripBtnUpLeft.action.Enable();
                gripBtnUpLeft.action.performed += gripInputUpLeft;
            }
        }
        else
        {
            if (gripInputRight != null ||
                gripInputPressedRight != null ||
                gripInputUpRight != null)
            {
                UnBingingGripInputActionRight();
            }
            gripInputRight = downAction;
            gripInputPressedRight = pressedAction;
            gripInputUpRight = upAction;
            if (gripBtnRight != null && gripInputRight != null)
            {
                gripBtnRight.action.Enable();
                gripBtnRight.action.performed += gripInputRight;
            }
            if (gripBtnPressedRight != null && gripInputPressedRight != null)
            {
                gripBtnPressedRight.action.Enable();
                gripBtnPressedRight.action.performed += gripInputPressedRight;
            }
            if (gripBtnUpRight != null && gripInputUpRight != null)
            {
                gripBtnUpRight.action.Enable();
                gripBtnUpRight.action.performed += gripInputUpRight;
            }
        }
    }

    public void BingingPrimaryHotKey(bool isLeftController, Action<InputAction.CallbackContext> downAction, Action<InputAction.CallbackContext> pressedAction, Action<InputAction.CallbackContext> upAction)
    {
        if (isLeftController)
        {
            if (primaryInputLeft != null ||
                primaryInputPressedLeft != null ||
                primaryInputUpLeft != null)
            {
                UnBingingPrimaryInputActionLeft();
            }
            primaryInputLeft = downAction;
            primaryInputPressedLeft = pressedAction;
            primaryInputUpLeft = upAction;
            if (primaryBtnLeft != null && primaryInputLeft != null)
            {
                primaryBtnLeft.action.Enable();
                primaryBtnLeft.action.performed += primaryInputLeft;
            }
            if (primaryBtnPressedLeft != null && primaryInputPressedLeft != null)
            {
                primaryBtnPressedLeft.action.Enable();
                primaryBtnPressedLeft.action.performed += primaryInputPressedLeft;
            }
            if (primaryBtnUpLeft != null && primaryInputUpLeft != null)
            {
                primaryBtnUpLeft.action.Enable();
                primaryBtnUpLeft.action.performed += primaryInputUpLeft;
            }
        }
        else
        {
            if (primaryInputRight != null ||
                primaryInputPressedRight != null ||
                primaryInputUpRight != null)
            {
                UnBingingPrimaryInputActionRight();
            }
            primaryInputRight = downAction;
            primaryInputPressedRight = pressedAction;
            primaryInputUpRight = upAction;
            if (primaryBtnRight != null && primaryInputRight != null)
            {
                primaryBtnRight.action.Enable();
                primaryBtnRight.action.performed += primaryInputRight;
            }
            if (primaryBtnPressedRight != null && primaryInputPressedRight != null)
            {
                primaryBtnPressedRight.action.Enable();
                primaryBtnPressedRight.action.performed += primaryInputPressedRight;
            }
            if (primaryBtnUpRight != null && primaryInputUpRight != null)
            {
                primaryBtnUpRight.action.Enable();
                primaryBtnUpRight.action.performed += primaryInputUpRight;
            }
        }
    }

    public void BingingSecondaryHotKey(bool isLeftController, Action<InputAction.CallbackContext> downAction, Action<InputAction.CallbackContext> pressedAction, Action<InputAction.CallbackContext> upAction)
    {
        if (isLeftController)
        {
            if (secondaryInputLeft != null ||
                secondaryInputPressedLeft != null ||
                secondaryInputUpLeft != null)
            {
                UnBingingSecondaryInputActionLeft();
            }
            secondaryInputLeft = downAction;
            secondaryInputPressedLeft = pressedAction;
            secondaryInputUpLeft = upAction;
            if (secondaryBtnLeft != null && secondaryInputLeft != null)
            {
                secondaryBtnLeft.action.Enable();
                secondaryBtnLeft.action.performed += secondaryInputLeft;
            }
            if (secondaryBtnPressedLeft != null && secondaryInputPressedLeft != null)
            {
                secondaryBtnPressedLeft.action.Enable();
                secondaryBtnPressedLeft.action.performed += secondaryInputPressedLeft;
            }
            if (secondaryBtnUpLeft != null && secondaryInputUpLeft != null)
            {
                secondaryBtnUpLeft.action.Enable();
                secondaryBtnUpLeft.action.performed += secondaryInputUpLeft;
            }
        }
        else
        {
            if (secondaryInputRight != null ||
                secondaryInputPressedRight != null ||
                secondaryInputUpRight != null)
            {
                UnBingingSecondaryInputActionRight();
            }
            secondaryInputRight = downAction;
            secondaryInputPressedRight = pressedAction;
            secondaryInputUpRight = upAction;
            if (secondaryBtnRight != null && secondaryInputRight != null)
            {
                secondaryBtnRight.action.Enable();
                secondaryBtnRight.action.performed += secondaryInputRight;
            }
            if (secondaryBtnPressedRight != null && secondaryInputPressedRight != null)
            {
                secondaryBtnPressedRight.action.Enable();
                secondaryBtnPressedRight.action.performed += secondaryInputPressedRight;
            }
            if (secondaryBtnUpRight != null && secondaryInputUpRight != null)
            {
                secondaryBtnUpRight.action.Enable();
                secondaryBtnUpRight.action.performed += secondaryInputUpRight;
            }
        }
    }
    #endregion

    #region Unbind button
    public void UnBingingGameHotKey(bool isLeftController)
    {
        if (isLeftController)
        {
            UnBingingMenuInputActionLeft();
            UnBingingTriggerInputActionLeft();
            UnBingingGripInputActionLeft();
            UnBingingSecondaryInputActionLeft();
        }
        else
        {
            UnBingingTriggerInputActionRight();
            UnBingingGripInputActionRight();
            UnBingingSecondaryInputActionRight();
        }
    }

    private void UnBingingMenuInputActionLeft()
    {
        if (menuBtnLeft != null && menuInputLeft != null)
        {
            menuBtnLeft.action.Disable();
            menuBtnLeft.action.performed -= menuInputLeft;
            menuInputLeft = null;
        }
    }

    private void UnBingingTriggerInputActionLeft()
    {
        if (triggerBtnLeft != null && triggerInputLeft != null)
        {
            triggerBtnLeft.action.Disable();
            triggerBtnLeft.action.performed -= triggerInputLeft;
            triggerInputLeft = null;
        }
    }

    private void UnBingingTriggerInputActionRight()
    {
        if (triggerBtnRight != null && triggerInputRight != null)
        {
            triggerBtnRight.action.Disable();
            triggerBtnRight.action.performed -= triggerInputRight;
            triggerInputRight = null;
        }
    }

    private void UnBingingGripInputActionLeft()
    {
        if (gripBtnLeft != null && gripInputLeft != null)
        {
            gripBtnLeft.action.Disable();
            gripBtnLeft.action.performed -= gripInputLeft;
            gripInputLeft = null;
        }
        if (gripBtnPressedLeft != null && gripInputPressedLeft != null)
        {
            gripBtnPressedLeft.action.Disable();
            gripBtnPressedLeft.action.performed -= gripInputPressedLeft;
            gripInputPressedLeft = null;
        }
        if (gripBtnUpLeft != null && gripInputUpLeft != null)
        {
            gripBtnUpLeft.action.Disable();
            gripBtnUpLeft.action.performed -= gripInputUpLeft;
            gripInputUpLeft = null;
        }
    }

    private void UnBingingGripInputActionRight()
    {
        if (gripBtnRight != null && gripInputRight != null)
        {
            gripBtnRight.action.Disable();
            gripBtnRight.action.performed -= gripInputRight;
            gripInputRight = null;
        }
        if (gripBtnPressedRight != null && gripInputPressedRight != null)
        {
            gripBtnPressedRight.action.Disable();
            gripBtnPressedRight.action.performed -= gripInputPressedRight;
            gripInputPressedRight = null;
        }
        if (gripBtnUpRight != null && gripInputUpRight != null)
        {
            gripBtnUpRight.action.Disable();
            gripBtnUpRight.action.performed -= gripInputUpRight;
            gripInputUpRight = null;
        }
    }

    private void UnBingingPrimaryInputActionLeft()
    {
        if (primaryBtnLeft != null && primaryInputLeft != null)
        {
            primaryBtnLeft.action.Disable();
            primaryBtnLeft.action.performed -= primaryInputLeft;
            primaryInputLeft = null;
        }
        if (primaryBtnPressedLeft != null && primaryInputPressedLeft != null)
        {
            primaryBtnPressedLeft.action.Disable();
            primaryBtnPressedLeft.action.performed -= primaryInputPressedLeft;
            primaryInputPressedLeft = null;
        }
        if (primaryBtnUpLeft != null && primaryInputUpLeft != null)
        {
            primaryBtnUpLeft.action.Disable();
            primaryBtnUpLeft.action.performed -= primaryInputUpLeft;
            primaryInputUpLeft = null;
        }
    }

    public void UnBingingPrimaryInputActionRight()
    {
        if (primaryBtnRight != null && primaryInputRight != null)
        {
            primaryBtnRight.action.Disable();
            primaryBtnRight.action.performed -= primaryInputRight;
            primaryInputRight = null;
        }
        if (primaryBtnPressedRight != null && primaryInputPressedRight != null)
        {
            primaryBtnPressedRight.action.Disable();
            primaryBtnPressedRight.action.performed -= primaryInputPressedRight;
            primaryInputPressedRight = null;
        }
        if (primaryBtnUpRight != null && primaryInputUpRight != null)
        {
            primaryBtnUpRight.action.Disable();
            primaryBtnUpRight.action.performed -= primaryInputUpRight;
            primaryInputUpRight = null;
        }
    }

    public void UnBingingSecondaryInputActionLeft()
    {
        if (secondaryBtnLeft != null && secondaryInputLeft != null)
        {
            secondaryBtnLeft.action.Disable();
            secondaryBtnLeft.action.performed -= secondaryInputLeft;
            secondaryInputLeft = null;
        }
        if (secondaryBtnPressedLeft != null && secondaryInputPressedLeft != null)
        {
            secondaryBtnPressedLeft.action.Disable();
            secondaryBtnPressedLeft.action.performed -= secondaryInputPressedLeft;
            secondaryInputPressedLeft = null;
        }
        if (secondaryBtnUpLeft != null && secondaryInputUpLeft != null)
        {
            secondaryBtnUpLeft.action.Disable();
            secondaryBtnUpLeft.action.performed -= secondaryInputUpLeft;
            secondaryInputUpLeft = null;
        }
    }

    public void UnBingingSecondaryInputActionRight()
    {
        if (secondaryBtnRight != null && secondaryInputRight != null)
        {
            secondaryBtnRight.action.Disable();
            secondaryBtnRight.action.performed -= secondaryInputRight;
            secondaryInputRight = null;
        }
        if (secondaryBtnPressedRight != null && secondaryInputPressedRight != null)
        {
            secondaryBtnPressedRight.action.Disable();
            secondaryBtnPressedRight.action.performed -= secondaryInputPressedRight;
            secondaryInputPressedRight = null;
        }
        if (secondaryBtnUpRight != null && secondaryInputUpRight != null)
        {
            secondaryBtnUpRight.action.Disable();
            secondaryBtnUpRight.action.performed -= secondaryInputUpRight;
            secondaryInputUpRight = null;
        }
    }
    #endregion
}
