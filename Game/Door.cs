using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace Game
{
    public class Door : MonoBehaviour, IInteractable, IPunObservable
    {
        public InteractableTypeEnum InteractableType { get { return InteractableTypeEnum.Door; } }
        public bool Interactable { get; set; } = true;
        public Transform RaycastAimPoint { get{ return m_RaycastAimPoint; }  }

        public PhotonView PhotonView;

        [SerializeField] private Transform m_RaycastAimPoint;
        [SerializeField] private GameObject m_DoorObject;
        [SerializeField] private float m_TurningSpeed = 2;

        private float m_TurnModifier = 0;
        private DoorStateEnum m_DoorState;
        private Vector3 m_NaturalRotation;
        private Vector3 m_MinusRotationTarget;
        private Vector3 m_PositiveRotationTarget;
        private Vector3 m_TargetRotation;

        private void Start()
        {
            m_NaturalRotation = transform.rotation.eulerAngles;
            m_MinusRotationTarget = transform.eulerAngles + new Vector3(0, -90, 0);
            m_PositiveRotationTarget = transform.eulerAngles + new Vector3(0, 90, 0);
            m_DoorState = DoorStateEnum.NaturalSide;
            PhotonView = GetComponent<PhotonView>();
        }

        private void FixedUpdate()
        {
            if (Quaternion.Angle(m_DoorObject.transform.rotation, Quaternion.Euler(m_TargetRotation)) < 0.1f)
            {
                m_DoorObject.transform.rotation = Quaternion.Euler(m_TargetRotation);
            }
            else
            {
                m_DoorObject.transform.Rotate(Vector3.up * m_TurnModifier, 1 * m_TurningSpeed);
            }
        }

        public void SetHighlight(bool state)
        {
        }

        public void OpenDoor(int actorNr, Vector3 playerPos)
        {
            PhotonView.TransferOwnership(actorNr);  

            switch (m_DoorState)
            {
                case DoorStateEnum.NegativeSide:
                    m_TargetRotation = m_NaturalRotation;
                    m_TurnModifier = 1;
                    m_DoorState = DoorStateEnum.NaturalSide;
                    break;
                case DoorStateEnum.NaturalSide:
                    Vector3 inverseTransform = transform.InverseTransformPoint(playerPos);

                    if (inverseTransform.z < 0)
                    {
                        m_TargetRotation = m_MinusRotationTarget;
                        m_DoorState = DoorStateEnum.NegativeSide;
                        m_TurnModifier = -1;
                    }
                    else if (inverseTransform.z > 0)
                    {
                        m_TargetRotation = m_PositiveRotationTarget;
                        m_DoorState = DoorStateEnum.PositiveSide;
                        m_TurnModifier = 1;
                    }
                    break;
                case DoorStateEnum.PositiveSide:
                    m_TargetRotation = m_NaturalRotation;
                    m_TurnModifier = -1;
                    m_DoorState = DoorStateEnum.NaturalSide;
                    break;
                default:
                    break;         
            }
        }



        public GameObject GetDoorObj()
        {
            return m_DoorObject;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(m_TurnModifier);
                stream.SendNext(m_TargetRotation);
                stream.SendNext((int)m_DoorState);
            }
            else
            {
                m_TurnModifier = (float)stream.ReceiveNext();
                m_TargetRotation = (Vector3)stream.ReceiveNext();
                m_DoorState = (DoorStateEnum)(int)stream.ReceiveNext();
            }
        }

        public enum DoorStateEnum
        {
            NegativeSide,
            NaturalSide,
            PositiveSide
        }
    }
}