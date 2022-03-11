using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game;

namespace Player
{
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerThrow : MonoBehaviour
    {
        public PhotonView PhotonView;
        public Transform PickupParent;
        public float ThrowingForce = 5f;
        public bool AbleToThrow = true;
        public bool DoPickup;
        public bool IsThrowing = false;

        [SerializeField] private ThrowableObject m_EquippedItem;
        [SerializeField] private GameObject m_Model;
        [SerializeField] Transform m_ThrowPos;
        private PlayerInteract m_PlayerInteract;
        private PlayerStatus m_PlayerStatus;
        private AnimationStateController m_ASC;
        void Start()
        {
            m_PlayerInteract = GetComponent<PlayerInteract>();
            m_PlayerStatus = GetComponent<PlayerStatus>();
            m_ASC = GetComponentInChildren<AnimationStateController>();
        }

        public void Throw(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (context.performed && !m_PlayerStatus.Stunned && !m_PlayerStatus.Dummy && !m_PlayerStatus.Busy)
            {
                if (m_EquippedItem != null && AbleToThrow)
                {
                    Throw();

                }
                else if (!DoPickup)
                {
                    EquipThrowable();
                }
            }
        }

        public void EquipThrowable()
        {
            int throwableID = m_PlayerInteract.GetThrowable();
            if (throwableID > 0)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonView.RPC("RPC_EquipThrowable", RpcTarget.All, throwableID);
                }
                else
                {
                    RPC_EquipThrowable(throwableID);
                }
            }
        }

        [PunRPC]
        public void RPC_EquipThrowable(int viewID)
        {
            GameObject throwable = GameManager.Instance.GetObjWithViewID(viewID);           

            if (throwable != null)
            {
                StartCoroutine(DoPickUp(throwable));
            }
            else
            {
                Debug.LogError("Throwable object not found with view ID: " + viewID);
            }          
        }

        IEnumerator DoPickUp(GameObject throwable)
        {
            m_ASC.PickUpAnimation();
            m_PlayerStatus.Busy = true;

            yield return new WaitUntil(() => DoPickup);

            m_EquippedItem = throwable.GetComponent<ThrowableObject>();
            throwable.transform.SetParent(PickupParent);
            throwable.transform.localPosition = Vector3.zero;
            m_EquippedItem.Equip(PhotonView.OwnerActorNr);
            DoPickup = false;
            m_PlayerStatus.Busy = false;
        }

        public void Throw()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonView.RPC("RPC_Throw", RpcTarget.All);
            }
            else
            {
                RPC_Throw();
            }
        }

        [PunRPC]
        public void RPC_Throw()
        {
            if (!IsThrowing)
            {
                m_ASC.ThrowAnimation();
                IsThrowing = true;
                AbleToThrow = false;
                m_PlayerStatus.Busy = true;

            }
        }
        public void DoThrow()
        {            
            m_EquippedItem.Throw(m_Model.transform.forward, ThrowingForce, m_ThrowPos.position);
            m_EquippedItem = null;
            IsThrowing = false;
            AudioManager.Instance.PlaySound(audioType.throwing);
        }
    }
}