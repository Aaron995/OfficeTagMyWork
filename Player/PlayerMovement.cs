using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerMovement : MonoBehaviour, IPunObservable
    {
        [Header("Movement settings")]
        [Tooltip("Value used for building up/slowing down the movement.")]
        public float MaxMovementForce = 2f;
        [Tooltip("Value used to indicate the max amount of meter/s")]
        public float MaxMovementSpeed = 1f;
        [Tooltip("The Weight of the player, slowing down movement")]
        public float Mass = 50f;
        [Tooltip("The max distance before the player start teleporting to sync up")]
        public float DesyncDistance = 5f;

        [Header("References")]
        public PlayerInput Input;
        public Camera PlayerCam;
        public PhotonView PhotonView;
        public Rigidbody RB;
        public GameObject model;

        private PlayerStatus m_PlayerStatus;
        private AudioManager m_audioManager;

        // Used to track movement
        public Vector3 m_MovementDirection;
        public Vector3 m_LookDirection = Vector3.zero;
        public Vector3 m_Velocity { private set; get; }

        // Used to sync across networks
        private Vector3 m_NetworkPosition;
        private Quaternion m_NetworkRotation;

        private void Start()
        {
            m_PlayerStatus = GetComponent<PlayerStatus>();
            RB = GetComponent<Rigidbody>();
            if (PlayerCam == null)
            {
                PlayerCam = Camera.main;
            }            
        }

        private void FixedUpdate()
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                if (Vector3.Distance(RB.position, m_NetworkPosition) > DesyncDistance)
                {
                    RB.position = m_NetworkPosition;
                }
                RB.position = Vector3.MoveTowards(RB.position, m_NetworkPosition, Time.fixedDeltaTime);
                model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, m_NetworkRotation, Time.fixedDeltaTime * 500f);
            }
            else
            {
                Movement();

                if (Input.currentControlScheme == GameConstants.k_KEYBOARDSCHEMENAME && !m_PlayerStatus.Dummy && !m_PlayerStatus.Stunned)
                {
                    LookAtMouse();
                }
                else if (Input.currentControlScheme == GameConstants.k_GAMEPADSCHEMENAME && !m_PlayerStatus.Dummy && !m_PlayerStatus.Stunned)
                {
                    LookAtRightStick();
                }
            }
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (context.performed && !m_PlayerStatus.Stunned && !m_PlayerStatus.Dummy)
            {
                Vector2 inputs = context.ReadValue<Vector2>();
                m_MovementDirection = new Vector3(inputs.x, 0, inputs.y);
                            
            }
            else
            {
                m_MovementDirection = Vector2.zero;
            }

            if (m_MovementDirection != Vector3.zero)
            {
                AudioManager.Instance.RunningToggle(true);
            }
            else
            {
                AudioManager.Instance.RunningToggle(false);
            }
        }

        public void LookDirection(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (context.performed)
            {
                m_LookDirection = context.ReadValue<Vector2>();
            }
        }     

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                m_MovementDirection = Vector3.zero;
            }
        }

        private void Movement()
        {
            if (m_PlayerStatus.Dummy || m_PlayerStatus.Stunned)
            {
                m_MovementDirection = Vector3.zero;
            }

            m_Velocity = RB.velocity;

            // Calculate the desired velocity
            Vector3 velocityDesired = (transform.position + m_MovementDirection * MaxMovementSpeed - transform.position).normalized * MaxMovementSpeed - m_Velocity;
            // Clamp the desired velocity to the max we can move at a time and then apply mass
            velocityDesired = Vector3.ClampMagnitude(velocityDesired, MaxMovementForce);
            velocityDesired /= Mass;
            // Update our velocity with the force being added from our desired velocity
            m_Velocity = Vector3.ClampMagnitude(m_Velocity + velocityDesired, MaxMovementSpeed);
            // Round our velocity to 4 dicimals to avoid micro movement
            m_Velocity = new Vector3((float)Math.Round(m_Velocity.x, 4), (float)Math.Round(m_Velocity.y, 4), (float)Math.Round(m_Velocity.z, 4));
            // Set our RB velocity with our
            RB.velocity = m_Velocity;
        }       


        private void LookAtMouse()
        {                  
            Ray cameraRay = PlayerCam.ScreenPointToRay(m_LookDirection);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayLength;

            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                Vector3 pointToLook = cameraRay.GetPoint(rayLength);
                model.transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));                           
            }
        }

        private void LookAtRightStick()
        {
            m_LookDirection.z = m_LookDirection.y;
            Vector3 pointToLook = transform.position + m_LookDirection;
            model.transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (RB == null || !PhotonNetwork.IsConnected)
            {
                return;
            }

            if (stream.IsWriting)
            {
                stream.SendNext(RB.position);
                stream.SendNext(RB.velocity);
                stream.SendNext(model.transform.rotation);
                stream.SendNext(m_MovementDirection);
            }
            else
            {
                m_NetworkPosition = (Vector3)stream.ReceiveNext();
                RB.velocity = (Vector3)stream.ReceiveNext();
                m_NetworkRotation = (Quaternion)stream.ReceiveNext();
                m_MovementDirection = (Vector3)stream.ReceiveNext();

                if (!Game.GameManager.Instance.GameStarted)
                {
                    transform.SetPositionAndRotation(m_NetworkPosition, Quaternion.identity);
                }

                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                m_NetworkPosition += (RB.velocity * lag);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && PhotonNetwork.IsConnected)
            {
                transform.SetPositionAndRotation(m_NetworkPosition, Quaternion.identity);
            }
        }       
    }
}