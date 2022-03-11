using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Player;

namespace Game
{
       [RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
    public class ThrowableObject : MonoBehaviour, IInteractable, IPunObservable
    {
        public InteractableTypeEnum InteractableType { get { return InteractableTypeEnum.Throwable; } }

        public bool Interactable { get { return m_interactable; } set { m_interactable = value; } } 

        public Transform RaycastAimPoint { get { return m_RaycastAimPoint; } }

        public Rigidbody RB;
        public PhotonView PhotonView;
        public Collider ObjCollider;
        
        [SerializeField] private Transform m_RaycastAimPoint;
        [SerializeField] private bool m_ReturnToStart = true;
        [SerializeField] private float m_TimeUntilReturn = 45;

        private bool m_CanStun = true;
        private bool m_UsedStun;
        private bool m_ReturningToStart;
        private bool m_interactable;
        private Vector3 m_StartPos;
        private Vector3 m_NetworkPosition;
        private Quaternion m_StartRot;
        private Quaternion m_NetworkRotation;
        private Coroutine m_IdleCoroutine;

        private void Start()
        {
            RB = GetComponent<Rigidbody>();
            RB.mass = 0.001f;
            PhotonView = GetComponent<PhotonView>();
            Interactable = true;
            m_UsedStun = true;
            m_StartPos = transform.position;
            m_StartRot = transform.rotation;
        }

        private void FixedUpdate()
        {
            if (PhotonNetwork.IsConnected && transform.parent == null && PhotonNetwork.LocalPlayer != PhotonView.Owner && !m_ReturningToStart)
            {
                RB.position = Vector3.MoveTowards(RB.position, m_NetworkPosition, Time.fixedDeltaTime);
                RB.rotation = Quaternion.RotateTowards(RB.rotation, m_NetworkRotation, Time.fixedDeltaTime * 100f);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (RB == null)
            {
                return;
            }

            if (stream.IsWriting)
            {
                stream.SendNext(RB.position);
                stream.SendNext(RB.velocity);
                stream.SendNext(RB.rotation);
                stream.SendNext(Interactable);
                stream.SendNext(m_UsedStun);
                stream.SendNext(m_ReturningToStart);
            }
            else
            {
                m_NetworkPosition = (Vector3)stream.ReceiveNext();
                RB.velocity = (Vector3)stream.ReceiveNext();
                m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                Interactable = (bool)stream.ReceiveNext();
                m_UsedStun = (bool)stream.ReceiveNext();
                m_ReturningToStart = (bool)stream.ReceiveNext();
                if (!GameManager.Instance.GameStarted)
                {
                    transform.SetPositionAndRotation(m_NetworkPosition, m_NetworkRotation);
                }
                else if (m_ReturningToStart)
                {
                    transform.SetPositionAndRotation(m_StartPos, m_StartRot);
                }
                else
                {
                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    m_NetworkPosition += (RB.velocity * lag);
                }
            }
        }

        public void SetHighlight(bool state)
        {           
        }

        public void Equip(int actorNr)
        {
            m_ReturningToStart = false;
            ObjCollider.enabled = false;
            RB.isKinematic = true;
            Interactable = false;
            PhotonView.TransferOwnership(actorNr);
            if (m_IdleCoroutine != null)
            {
                StopCoroutine(m_IdleCoroutine);
                m_IdleCoroutine = null;
            }
        }

        public void Throw(Vector3 direction, float force, Vector3 startPos)
        {
            transform.position = startPos;
            m_UsedStun = false;
            RB.isKinematic = false;
            gameObject.transform.parent = null;
            direction.y = 0;
            RB.AddForce(direction.normalized * force);
            m_CanStun = false;
            StartCoroutine(EnableCollider());
            if (m_ReturnToStart) m_IdleCoroutine = StartCoroutine(Idling());
        }

        IEnumerator EnableCollider()
        {
            ObjCollider.enabled = true;
            yield return new WaitForSeconds(0.1f);
            m_CanStun = true;
        }

        IEnumerator Idling()
        {            
            yield return new WaitForSeconds(m_TimeUntilReturn);
            m_ReturningToStart = true;
            transform.SetPositionAndRotation(m_StartPos, m_StartRot);
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (gameObject.transform.parent != null || !m_CanStun)
            {
                return;
            }

            RB.velocity = Vector3.zero;
            PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                if (!playerStatus.AbleToBeStunned)
                {
                    return;
                }
                else if (!m_UsedStun)
                {
                    playerStatus.GetStunned();
                }
            }

            if (!Interactable && transform.parent == null)
            {
                Interactable = true;
                m_UsedStun = true;
            }
        }        
    }
}