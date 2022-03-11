using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System;

namespace UI
{
    public class KeybindsMenu : MonoBehaviour
    {
        public static KeybindsMenu Instance;

        public InputActionAsset Actions;
        public event Action RebindStarted;
        public event Action RebindFinished;
        public int ActionMapIndex = 0;
        public bool Binding { private set; get; }

        public BindingTypeEnum BindingType = BindingTypeEnum.KeyboardMouse;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }

            LoadBindingOverrides();
        }

        private void OnDisable()
        {
            SaveBindingOverrides();
        }

        private void OnApplicationQuit()
        {
            SaveBindingOverrides();
        }

        public void SwitchBindingType(BindingTypeEnum type)
        {
            BindingType = type;
        }

        public void DoRebind(string actionName, CallbackContext callbackContext, int bindingIndex)
        {
            InputAction action = Actions.FindAction(actionName);

            // If we manage to have a valid action we can start the rebinding
            if (action != null)
            {
                Binding = true; 

                // We disable the action we are rebinding
                action.Disable();

                // Create the interactive rerebinding variable so we can start declaring some of it's fields 
                var rebind = action.PerformInteractiveRebinding(bindingIndex);

                // Add our logic for when it's completed
                rebind.OnComplete(operation =>
                {
                    rebind.Dispose();
                    action.Enable();

                    if (CheckDupesBinds(action,bindingIndex))
                    {
                        rebind.Dispose();
                        callbackContext.OnDupeRetry();
                        DoRebind(actionName, callbackContext, bindingIndex);
                        return;
                    }

                    RebindFinished?.Invoke();
                    callbackContext.OnComplete();
                    Binding = false;
                });

                // Add our logic if it gets canceled
                rebind.OnCancel(operation =>
                {
                    rebind.Dispose();
                    action.Enable();
                    callbackContext.OnCancel();
                    RebindFinished?.Invoke();
                    operation.Dispose();
                    Binding = false;
                });

                // Invoke the action for our systems to react to the rebind and start it
                RebindStarted?.Invoke();
                rebind.Start();
            }
            else
            {
                Debug.LogError("Invalid actionName! " + actionName);
            }
        }

        public string GetBindingName(string actionName, int bindingIndex)
        {
            return Actions.FindAction(actionName).GetBindingDisplayString(bindingIndex);
        }

        public void ResetBinding(string actionName, int bindingIndex)
        {
            InputAction action = Actions.FindAction(actionName);

            if (action == null || action.bindings.Count <= bindingIndex)
            {
                Debug.Log("Could not find action or binding");
                return;
            }

            if (action.bindings[bindingIndex].isComposite)
            {
                for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                    action.RemoveBindingOverride(i);
            }
            else
                action.RemoveBindingOverride(bindingIndex);            
        }

        public void ResetAllBindings()
        {
            foreach (InputAction action in Actions.actionMaps[ActionMapIndex])
            {
                action.RemoveAllBindingOverrides();
            }            
        }
        private void LoadBindingOverrides()
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                Actions.LoadBindingOverridesFromJson(rebinds);
        }

        private void SaveBindingOverrides()
        {
            var rebinds = Actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }

        private bool CheckDupesBinds(InputAction action, int index)
        {
            InputBinding newBinding = action.bindings[index];
            foreach (InputBinding binding in action.actionMap.bindings)
            {               
                if (binding == newBinding && !newBinding.isPartOfComposite)
                {
                    continue;
                }
                else if (newBinding.isPartOfComposite && newBinding.id == binding.id)
                {
                    continue;
                }


                if (binding.effectivePath == newBinding.effectivePath)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum BindingTypeEnum
    {
        KeyboardMouse = 1,
        Gamepad = 0
    }

    public struct CallbackContext
    {
        public delegate void OnCompleteDelegate();
        public delegate void OnCancelDelegate();
        public delegate void OnDupeRetryDelegate();

        public OnCompleteDelegate OnComplete;
        public OnCancelDelegate OnCancel;
        public OnDupeRetryDelegate OnDupeRetry;

        public CallbackContext(OnCompleteDelegate onComplete, OnCancelDelegate onCancel, OnDupeRetryDelegate onDupe)
        {
            OnComplete = onComplete;
            OnCancel = onCancel;
            OnDupeRetry = onDupe;
        }
    }
}