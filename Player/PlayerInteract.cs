using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game;

namespace Player
{
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerInteract : MonoBehaviour
    {
        public bool EnableGizmo = true;
        public bool PlayingInteract = false;
        public PhotonView PhotonView;
        public Vector3 InteractRange;
        public LayerMask LineOfSightLayers;        

        [SerializeField] private GameObject m_Model;
        [SerializeField] private Transform m_LineOfSightStart;
        
        private PlayerStatus m_PlayerStatus;
        private AnimationStateController m_ASC;
        private Door m_InteractingDoor;
        

        void Start()
        {
            m_PlayerStatus = GetComponent<PlayerStatus>();
            m_ASC = GetComponentInChildren<AnimationStateController>();
        }

        public void Interact(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (context.performed && !m_PlayerStatus.Stunned && !m_PlayerStatus.Dummy && !m_PlayerStatus.Busy && !PlayingInteract)
            {
                GameObject selectedDoor = GetDoor();
                Debug.Log("Hit door " + selectedDoor);                
                if (selectedDoor != null )
                {
                    PlayingInteract = true;
                    if (PhotonNetwork.IsConnected)
                    {
                        PhotonView.RPC("RPC_InteractAnimation", RpcTarget.All);
                    }
                    else
                    {
                        RPC_InteractAnimation();
                    }

                    OpenDoor(selectedDoor);
                }
            }
        }

        public int GetThrowable()
        {
            // Box cast to get objects in our range
            RaycastHit[] hits = Physics.BoxCastAll(transform.position + Vector3.up, InteractRange / 2, m_Model.transform.forward, Quaternion.identity, InteractRange.z);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    continue;
                }
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (!interactable.Interactable)
                    {
                        continue;
                    }

                    // Only check for throwables
                    if (interactable.InteractableType == InteractableTypeEnum.Throwable)
                    {
                        // If we don't we check for line of sight
                        RaycastHit objectInTheWay;

                        Vector3 aimPoint = hit.collider.gameObject.transform.position;
                        if (interactable.RaycastAimPoint != null)
                        {
                            aimPoint = interactable.RaycastAimPoint.position;
                        }

                        if (!Physics.Raycast(m_LineOfSightStart.position, aimPoint - transform.position, out objectInTheWay, Vector3.Distance(m_LineOfSightStart.position, hit.transform.position), LineOfSightLayers))
                        {
                            return hit.collider.gameObject.GetComponent<PhotonView>().ViewID;
                        }
                        else if (objectInTheWay.collider.gameObject.GetComponentInParent<ThrowableObject>() != null)
                        {
                            return hit.collider.gameObject.GetComponent<PhotonView>().ViewID;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return -1;
        }

        public GameObject GetDoor()
        {            
            // Box cast to get objects in our range
            RaycastHit[] hits = Physics.BoxCastAll(transform.position + Vector3.up, InteractRange / 2, m_Model.transform.forward, Quaternion.identity, InteractRange.z);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    continue;
                }
                IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (!interactable.Interactable)
                    {
                        continue;
                    }

                    // Only check for doors
                     if (interactable.InteractableType == InteractableTypeEnum.Door)
                    {
                        // If we don't we check for line of sight
                        RaycastHit objectInTheWay;

                        Vector3 aimPoint = hit.collider.gameObject.transform.position;
                        if (interactable.RaycastAimPoint != null)
                        {
                            aimPoint = interactable.RaycastAimPoint.position;
                        }

                        if (!Physics.Raycast(m_LineOfSightStart.position, aimPoint - transform.position, out objectInTheWay, Vector3.Distance(m_LineOfSightStart.position, hit.transform.position), LineOfSightLayers))
                        {
                            return hit.collider.gameObject;
                        }
                        else
                        {
                            if (objectInTheWay.collider.gameObject == interactable.gameObject.GetComponent<Door>().GetDoorObj())
                            {
                                return hit.collider.gameObject;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }                   
                }
            }
            return null;
        }


        public void InteractDoor()
        {
            if (PhotonNetwork.LocalPlayer == PhotonView.Owner || !PhotonNetwork.IsConnected)
            {
                m_InteractingDoor.OpenDoor(PhotonView.OwnerActorNr, transform.position);
            }
        }

        [PunRPC]
        private void RPC_InteractAnimation()
        {           
            m_ASC.InteractDoorAnimation();
            m_PlayerStatus.Busy = true;
        }

        private void OpenDoor(GameObject door)
        {
            if (PhotonNetwork.LocalPlayer == PhotonView.Owner || !PhotonNetwork.IsConnected)
            {
                m_InteractingDoor = door.GetComponent<Door>();
            }
        }

#if (UNITY_EDITOR)
        private void OnDrawGizmos()
        {
            if (EnableGizmo)
            {
                Vector3 center = transform.position + transform.forward;
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(center, InteractRange);
                Gizmos.color = default;
            }
        }
#endif
    }
}
