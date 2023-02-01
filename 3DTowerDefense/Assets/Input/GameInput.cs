//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/Input/GameInput.inputactions
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

namespace TowerDefenseInput
{
    public partial class @GameInput : IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @GameInput()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameInput"",
    ""maps"": [
        {
            ""name"": ""GamePlay"",
            ""id"": ""ba74c39c-9bc4-44b0-b528-c269b4537aa9"",
            ""actions"": [
                {
                    ""name"": ""CameraMove"",
                    ""type"": ""Value"",
                    ""id"": ""ff740347-9c58-471c-9bbb-12beeebe2dca"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CameraRotate"",
                    ""type"": ""Value"",
                    ""id"": ""c2654ce5-136a-4a84-9a6c-3ff1a8aaed4f"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CursorPosition"",
                    ""type"": ""Value"",
                    ""id"": ""aa0ab78f-3af8-4756-ac27-bbf968be0047"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""a2b41a4f-4c02-496d-96ad-77b6e9cd39d8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""9bdbdafb-f876-422a-a990-140594028adf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""36e24a10-e0f9-4152-9b71-f2d915461a2d"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Up"",
                    ""id"": ""ebe4f181-11b7-4a0f-aae4-707fcddd9dc1"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Down"",
                    ""id"": ""6fee32bb-f9d4-42ac-b5b4-4ec809d415d0"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Left"",
                    ""id"": ""85c462cb-0dc1-48ed-894e-e8fccc2f3708"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Right"",
                    ""id"": ""e36fc4db-7166-4a97-8958-1ac3067ad58b"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""a23d2e96-16a3-4066-91c4-d010104e4f2b"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""b155eb3c-5514-4199-951d-cf7ba7f6b8d6"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""fe29ec8f-4b6d-4e22-8005-275d670efee8"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""af8dedc9-4528-456b-b78e-49a425c30c23"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CursorPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""07bc62d0-3ec9-4ac3-851c-35506de6045e"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ba8e3eda-c260-4d81-9202-408814cff7df"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""b4cc9d02-b1b9-4df0-a4f4-a32a49462b08"",
            ""actions"": [
                {
                    ""name"": ""Resume"",
                    ""type"": ""Button"",
                    ""id"": ""a9acbcc2-e96e-4233-a627-4c9c8708f54b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f8c06e28-d08e-4926-bc72-cb89ba36ef2f"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Resume"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // GamePlay
            m_GamePlay = asset.FindActionMap("GamePlay", throwIfNotFound: true);
            m_GamePlay_CameraMove = m_GamePlay.FindAction("CameraMove", throwIfNotFound: true);
            m_GamePlay_CameraRotate = m_GamePlay.FindAction("CameraRotate", throwIfNotFound: true);
            m_GamePlay_CursorPosition = m_GamePlay.FindAction("CursorPosition", throwIfNotFound: true);
            m_GamePlay_LeftClick = m_GamePlay.FindAction("LeftClick", throwIfNotFound: true);
            m_GamePlay_Pause = m_GamePlay.FindAction("Pause", throwIfNotFound: true);
            // UI
            m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
            m_UI_Resume = m_UI.FindAction("Resume", throwIfNotFound: true);
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

        // GamePlay
        private readonly InputActionMap m_GamePlay;
        private IGamePlayActions m_GamePlayActionsCallbackInterface;
        private readonly InputAction m_GamePlay_CameraMove;
        private readonly InputAction m_GamePlay_CameraRotate;
        private readonly InputAction m_GamePlay_CursorPosition;
        private readonly InputAction m_GamePlay_LeftClick;
        private readonly InputAction m_GamePlay_Pause;
        public struct GamePlayActions
        {
            private @GameInput m_Wrapper;
            public GamePlayActions(@GameInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @CameraMove => m_Wrapper.m_GamePlay_CameraMove;
            public InputAction @CameraRotate => m_Wrapper.m_GamePlay_CameraRotate;
            public InputAction @CursorPosition => m_Wrapper.m_GamePlay_CursorPosition;
            public InputAction @LeftClick => m_Wrapper.m_GamePlay_LeftClick;
            public InputAction @Pause => m_Wrapper.m_GamePlay_Pause;
            public InputActionMap Get() { return m_Wrapper.m_GamePlay; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(GamePlayActions set) { return set.Get(); }
            public void SetCallbacks(IGamePlayActions instance)
            {
                if (m_Wrapper.m_GamePlayActionsCallbackInterface != null)
                {
                    @CameraMove.started -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraMove;
                    @CameraMove.performed -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraMove;
                    @CameraMove.canceled -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraMove;
                    @CameraRotate.started -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraRotate;
                    @CameraRotate.performed -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraRotate;
                    @CameraRotate.canceled -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCameraRotate;
                    @CursorPosition.started -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCursorPosition;
                    @CursorPosition.performed -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCursorPosition;
                    @CursorPosition.canceled -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnCursorPosition;
                    @LeftClick.started -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnLeftClick;
                    @LeftClick.performed -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnLeftClick;
                    @LeftClick.canceled -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnLeftClick;
                    @Pause.started -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnPause;
                    @Pause.performed -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnPause;
                    @Pause.canceled -= m_Wrapper.m_GamePlayActionsCallbackInterface.OnPause;
                }
                m_Wrapper.m_GamePlayActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @CameraMove.started += instance.OnCameraMove;
                    @CameraMove.performed += instance.OnCameraMove;
                    @CameraMove.canceled += instance.OnCameraMove;
                    @CameraRotate.started += instance.OnCameraRotate;
                    @CameraRotate.performed += instance.OnCameraRotate;
                    @CameraRotate.canceled += instance.OnCameraRotate;
                    @CursorPosition.started += instance.OnCursorPosition;
                    @CursorPosition.performed += instance.OnCursorPosition;
                    @CursorPosition.canceled += instance.OnCursorPosition;
                    @LeftClick.started += instance.OnLeftClick;
                    @LeftClick.performed += instance.OnLeftClick;
                    @LeftClick.canceled += instance.OnLeftClick;
                    @Pause.started += instance.OnPause;
                    @Pause.performed += instance.OnPause;
                    @Pause.canceled += instance.OnPause;
                }
            }
        }
        public GamePlayActions @GamePlay => new GamePlayActions(this);

        // UI
        private readonly InputActionMap m_UI;
        private IUIActions m_UIActionsCallbackInterface;
        private readonly InputAction m_UI_Resume;
        public struct UIActions
        {
            private @GameInput m_Wrapper;
            public UIActions(@GameInput wrapper) { m_Wrapper = wrapper; }
            public InputAction @Resume => m_Wrapper.m_UI_Resume;
            public InputActionMap Get() { return m_Wrapper.m_UI; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
            public void SetCallbacks(IUIActions instance)
            {
                if (m_Wrapper.m_UIActionsCallbackInterface != null)
                {
                    @Resume.started -= m_Wrapper.m_UIActionsCallbackInterface.OnResume;
                    @Resume.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnResume;
                    @Resume.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnResume;
                }
                m_Wrapper.m_UIActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Resume.started += instance.OnResume;
                    @Resume.performed += instance.OnResume;
                    @Resume.canceled += instance.OnResume;
                }
            }
        }
        public UIActions @UI => new UIActions(this);
        public interface IGamePlayActions
        {
            void OnCameraMove(InputAction.CallbackContext context);
            void OnCameraRotate(InputAction.CallbackContext context);
            void OnCursorPosition(InputAction.CallbackContext context);
            void OnLeftClick(InputAction.CallbackContext context);
            void OnPause(InputAction.CallbackContext context);
        }
        public interface IUIActions
        {
            void OnResume(InputAction.CallbackContext context);
        }
    }
}
