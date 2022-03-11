using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEditor;

namespace UI
{
    public class RebindKey : MonoBehaviour
    {
        [SerializeField] private Button m_RebindButton;
        [SerializeField] private Button m_ResetButton;
        [SerializeField] private TMPro.TextMeshProUGUI m_ButtonTextField;
        [SerializeField] private string m_WaitingText = "Waiting for input!";
        [SerializeField] private string m_DupeErrorMessage = "Duplicate keybind, try again!";
        public InputActionReference InputAction;
        [HideInInspector] public int CompositeIndex = -1;

        void Start()
        {
            m_RebindButton.onClick.AddListener(StartRebind);
            m_ResetButton.onClick.AddListener(ResetBind);
        }

        private void OnEnable()
        {
            KeybindsMenu.Instance.RebindStarted += () => m_RebindButton.interactable = false; m_ResetButton.interactable = false;
            KeybindsMenu.Instance.RebindFinished += () => m_RebindButton.interactable = true; m_ResetButton.interactable = true;
            UpdateUI();
        }

        private void OnDisable()
        {
            KeybindsMenu.Instance.RebindStarted -= () => m_RebindButton.interactable = false; m_ResetButton.interactable = false;
            KeybindsMenu.Instance.RebindFinished -= () => m_RebindButton.interactable = true; m_ResetButton.interactable = true;
        }

        public void StartRebind()
        {
            m_ButtonTextField.text = m_WaitingText;
            CallbackContext callback = new CallbackContext(OnComplete, OnCancel, OnDupeRetry);
            int bindingID = (int)KeybindsMenu.Instance.BindingType;

            if (InputAction.action.bindings[0].isComposite)
            {
                bindingID = CompositeIndex;
            }

            KeybindsMenu.Instance.DoRebind(InputAction.action.name, callback, bindingID);
        }

        public void ResetBind()
        {
            KeybindsMenu.Instance.ResetBinding(InputAction.action.name, (int)KeybindsMenu.Instance.BindingType);
            UpdateUI();
        }

        private void OnComplete()
        {
            UpdateUI();
        }

        private void OnCancel()
        {
            UpdateUI();
        }

        private void OnDupeRetry()
        {
            m_ButtonTextField.text = m_DupeErrorMessage;
        }

        private void UpdateUI()
        {
            if (InputAction.action.bindings[0].isComposite)
            {
                m_ButtonTextField.text = KeybindsMenu.Instance.GetBindingName(InputAction.action.name, CompositeIndex);
            }
            else
            {
                m_ButtonTextField.text = KeybindsMenu.Instance.GetBindingName(InputAction.action.name, (int)KeybindsMenu.Instance.BindingType);
            }
        }
    }
#if(UNITY_EDITOR)
    [CustomEditor(typeof(RebindKey))]
    public class RebindKeyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RebindKey rebindKey = target as RebindKey;

            if (rebindKey.InputAction.action == null)
            {
                return;
            }

            if (rebindKey.InputAction.action.bindings[0].isComposite)
            {
                rebindKey.CompositeIndex = EditorGUILayout.IntField("Composite Index ", rebindKey.CompositeIndex);

                GUILayout.Label("For compesite ID use following : ");

                for (int i = 0; i < rebindKey.InputAction.action.bindings.Count; i++)
                {
                    GUILayout.Label(rebindKey.InputAction.action.bindings[i].ToString() + " = " + i); 
                }
            }
        }
    }
#endif
}