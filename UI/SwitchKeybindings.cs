using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class SwitchKeybindings : MonoBehaviour
    {
        [SerializeField] private GameObject m_KeyboardBindings;
        [SerializeField] private GameObject m_GamepadBindings;

        private void OnEnable()
        {
            HandleSwitch(FindObjectOfType<PlayerInput>());
        }

        public void HandleSwitch(PlayerInput input)
        {
            if (KeybindsMenu.Instance.Binding)
            {
                return;
            }

            if (input.currentControlScheme == GameConstants.k_KEYBOARDSCHEMENAME)
            {
                KeybindsMenu.Instance.SwitchBindingType(BindingTypeEnum.KeyboardMouse);
                m_KeyboardBindings.SetActive(true);
                m_GamepadBindings.SetActive(false);
            }
            else if (input.currentControlScheme == GameConstants.k_GAMEPADSCHEMENAME)
            {
                KeybindsMenu.Instance.SwitchBindingType(BindingTypeEnum.Gamepad);
                m_GamepadBindings.SetActive(true);
                m_KeyboardBindings.SetActive(false);
            }
        }
    }
}
