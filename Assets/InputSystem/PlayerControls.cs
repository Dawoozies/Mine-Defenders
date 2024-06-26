//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/InputSystem/PlayerControls.inputactions
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

public partial class @PlayerControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""ScreenPos"",
            ""id"": ""bd891f5f-6134-4580-b0e3-76bf90423583"",
            ""actions"": [
                {
                    ""name"": ""MouseScreenPos"",
                    ""type"": ""Value"",
                    ""id"": ""d3859f38-9b15-4ad6-8257-6d9d2a54da20"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2355eae8-aa9c-42e0-b4ba-f07464b69f6d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseScreenPos"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cb7230f3-6fe8-4b7a-896f-98acd88538de"",
                    ""path"": ""<Touchscreen>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseScreenPos"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Player"",
            ""id"": ""ba636e95-1bad-4f4b-a8de-2842b799aeb1"",
            ""actions"": [
                {
                    ""name"": ""Tap"",
                    ""type"": ""PassThrough"",
                    ""id"": ""0bd3e668-45f8-4584-833d-4116073cfb1f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Press"",
                    ""type"": ""Button"",
                    ""id"": ""86cd7e6c-67db-424b-8f1a-6fa29918c1e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""89f93049-0ca2-48bc-b931-164d6d49eebc"",
                    ""path"": ""<Touchscreen>/primaryTouch/startPosition"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""410cc7bc-f766-425a-9bdf-596fbdd44e30"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Press"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3e402fe7-4c43-40e7-a702-62f4dfc01c3a"",
                    ""path"": ""<Touchscreen>/Press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Press"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // ScreenPos
        m_ScreenPos = asset.FindActionMap("ScreenPos", throwIfNotFound: true);
        m_ScreenPos_MouseScreenPos = m_ScreenPos.FindAction("MouseScreenPos", throwIfNotFound: true);
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Tap = m_Player.FindAction("Tap", throwIfNotFound: true);
        m_Player_Press = m_Player.FindAction("Press", throwIfNotFound: true);
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

    // ScreenPos
    private readonly InputActionMap m_ScreenPos;
    private List<IScreenPosActions> m_ScreenPosActionsCallbackInterfaces = new List<IScreenPosActions>();
    private readonly InputAction m_ScreenPos_MouseScreenPos;
    public struct ScreenPosActions
    {
        private @PlayerControls m_Wrapper;
        public ScreenPosActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseScreenPos => m_Wrapper.m_ScreenPos_MouseScreenPos;
        public InputActionMap Get() { return m_Wrapper.m_ScreenPos; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ScreenPosActions set) { return set.Get(); }
        public void AddCallbacks(IScreenPosActions instance)
        {
            if (instance == null || m_Wrapper.m_ScreenPosActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_ScreenPosActionsCallbackInterfaces.Add(instance);
            @MouseScreenPos.started += instance.OnMouseScreenPos;
            @MouseScreenPos.performed += instance.OnMouseScreenPos;
            @MouseScreenPos.canceled += instance.OnMouseScreenPos;
        }

        private void UnregisterCallbacks(IScreenPosActions instance)
        {
            @MouseScreenPos.started -= instance.OnMouseScreenPos;
            @MouseScreenPos.performed -= instance.OnMouseScreenPos;
            @MouseScreenPos.canceled -= instance.OnMouseScreenPos;
        }

        public void RemoveCallbacks(IScreenPosActions instance)
        {
            if (m_Wrapper.m_ScreenPosActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IScreenPosActions instance)
        {
            foreach (var item in m_Wrapper.m_ScreenPosActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_ScreenPosActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public ScreenPosActions @ScreenPos => new ScreenPosActions(this);

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_Tap;
    private readonly InputAction m_Player_Press;
    public struct PlayerActions
    {
        private @PlayerControls m_Wrapper;
        public PlayerActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Tap => m_Wrapper.m_Player_Tap;
        public InputAction @Press => m_Wrapper.m_Player_Press;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @Tap.started += instance.OnTap;
            @Tap.performed += instance.OnTap;
            @Tap.canceled += instance.OnTap;
            @Press.started += instance.OnPress;
            @Press.performed += instance.OnPress;
            @Press.canceled += instance.OnPress;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @Tap.started -= instance.OnTap;
            @Tap.performed -= instance.OnTap;
            @Tap.canceled -= instance.OnTap;
            @Press.started -= instance.OnPress;
            @Press.performed -= instance.OnPress;
            @Press.canceled -= instance.OnPress;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    public interface IScreenPosActions
    {
        void OnMouseScreenPos(InputAction.CallbackContext context);
    }
    public interface IPlayerActions
    {
        void OnTap(InputAction.CallbackContext context);
        void OnPress(InputAction.CallbackContext context);
    }
}
