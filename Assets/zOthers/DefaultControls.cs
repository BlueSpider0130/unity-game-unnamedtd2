//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/zOthers/DefaultControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @DefaultControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @DefaultControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultControls"",
    ""maps"": [
        {
            ""name"": ""DesktopControls"",
            ""id"": ""6a65baf1-ab7f-42a7-a2bd-39f1c2fad1ca"",
            ""actions"": [
                {
                    ""name"": ""CurrentMousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""e388e1da-6497-4462-944f-365ba948faac"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CurrentMousePosDelta"",
                    ""type"": ""Value"",
                    ""id"": ""581938fb-fa90-432d-b565-a476996cad2d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""6dc1da5c-eba4-46f6-b080-d0d22e35d73e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""697d2744-e325-4250-a055-845aed744091"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CurrentMousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f3c503d7-5245-486c-9e81-ec383e5b76b1"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CurrentMousePosDelta"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""252ccc37-06b6-4383-9e24-59c770ea2a9d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // DesktopControls
        m_DesktopControls = asset.FindActionMap("DesktopControls", throwIfNotFound: true);
        m_DesktopControls_CurrentMousePosition = m_DesktopControls.FindAction("CurrentMousePosition", throwIfNotFound: true);
        m_DesktopControls_CurrentMousePosDelta = m_DesktopControls.FindAction("CurrentMousePosDelta", throwIfNotFound: true);
        m_DesktopControls_Interact = m_DesktopControls.FindAction("Interact", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // DesktopControls
    private readonly InputActionMap m_DesktopControls;
    private IDesktopControlsActions m_DesktopControlsActionsCallbackInterface;
    private readonly InputAction m_DesktopControls_CurrentMousePosition;
    private readonly InputAction m_DesktopControls_CurrentMousePosDelta;
    private readonly InputAction m_DesktopControls_Interact;
    public struct DesktopControlsActions
    {
        private @DefaultControls m_Wrapper;
        public DesktopControlsActions(@DefaultControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CurrentMousePosition => m_Wrapper.m_DesktopControls_CurrentMousePosition;
        public InputAction @CurrentMousePosDelta => m_Wrapper.m_DesktopControls_CurrentMousePosDelta;
        public InputAction @Interact => m_Wrapper.m_DesktopControls_Interact;
        public InputActionMap Get() { return m_Wrapper.m_DesktopControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DesktopControlsActions set) { return set.Get(); }
        public void SetCallbacks(IDesktopControlsActions instance)
        {
            if (m_Wrapper.m_DesktopControlsActionsCallbackInterface != null)
            {
                @CurrentMousePosition.started -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosition;
                @CurrentMousePosition.performed -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosition;
                @CurrentMousePosition.canceled -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosition;
                @CurrentMousePosDelta.started -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosDelta;
                @CurrentMousePosDelta.performed -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosDelta;
                @CurrentMousePosDelta.canceled -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnCurrentMousePosDelta;
                @Interact.started -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnInteract;
                @Interact.performed -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnInteract;
                @Interact.canceled -= m_Wrapper.m_DesktopControlsActionsCallbackInterface.OnInteract;
            }
            m_Wrapper.m_DesktopControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CurrentMousePosition.started += instance.OnCurrentMousePosition;
                @CurrentMousePosition.performed += instance.OnCurrentMousePosition;
                @CurrentMousePosition.canceled += instance.OnCurrentMousePosition;
                @CurrentMousePosDelta.started += instance.OnCurrentMousePosDelta;
                @CurrentMousePosDelta.performed += instance.OnCurrentMousePosDelta;
                @CurrentMousePosDelta.canceled += instance.OnCurrentMousePosDelta;
                @Interact.started += instance.OnInteract;
                @Interact.performed += instance.OnInteract;
                @Interact.canceled += instance.OnInteract;
            }
        }
    }
    public DesktopControlsActions @DesktopControls => new DesktopControlsActions(this);
    public interface IDesktopControlsActions
    {
        void OnCurrentMousePosition(InputAction.CallbackContext context);
        void OnCurrentMousePosDelta(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
    }
}