using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IInteractable 
    {
        public GameObject gameObject { get; }
        public InteractableTypeEnum InteractableType { get; }
        public bool Interactable { get; }
        public Transform RaycastAimPoint { get; }

        public void SetHighlight(bool state);        
    }

    public enum InteractableTypeEnum
    {
        Throwable,
        Door
    }
}