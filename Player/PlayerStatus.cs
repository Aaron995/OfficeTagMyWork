using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System;

namespace Player
{
    public class PlayerStatus : MonoBehaviour, IPunObservable
    {
        public PhotonView PhotonView;
        public PlayerBounty Bounty;
        public float StunDuration = 1.5f;
        public bool Stunned = false;
        public bool Dummy = false;
        public bool Busy = false;
        public PlayerRoleEnum PlayerRole = PlayerRoleEnum.Runner;
        public int Points = 0;
        public bool AbleToBeStunned = true;

        private Action<float> UnityUpdateTick;
        [SerializeField] private AnimationStateController m_ASC;
        [SerializeField] private GameObject _tagHat;
        [SerializeField] private float m_StunInvincibility = 1.1f;
        [SerializeField] private GameObject m_BountyParticles;

        private void Awake()
        {
            PhotonView = GetComponent<PhotonView>();
            Bounty = new PlayerBounty(PhotonView.Owner,this, m_BountyParticles);
            if (PhotonView.IsMine)
            {
                UnityUpdateTick += Bounty.Update;
            }
            
        }
        private void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                Dummy = true;
                Game.GameManager.Instance.GameStart.AddListener(() => Dummy = false);
                Game.GameManager.Instance.GameEnd.AddListener(() => Dummy = true);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(PlayerRole);
                stream.SendNext(Stunned);
                stream.SendNext(Dummy);
                stream.SendNext(Points);
                stream.SendNext(Bounty.GetBounty());
                stream.SendNext(AbleToBeStunned);
            }
            else
            {
                PlayerRole = (PlayerRoleEnum)stream.ReceiveNext();
                Stunned = (bool)stream.ReceiveNext();
                Dummy = (bool)stream.ReceiveNext();
                Points = (int)stream.ReceiveNext();
                Bounty.SetBounty((int)stream.ReceiveNext());
                AbleToBeStunned = (bool)stream.ReceiveNext();
                if (PlayerRole == PlayerRoleEnum.Tagger)
                {
                    _tagHat.SetActive(true);
                }
                else
                {
                    _tagHat.SetActive(false);
                }
            }
        }

        public void Pause(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (context.performed)
            {                
                PauseMenu.Instance.PauzeCheck();
                if (Game.GameManager.Instance.GameStarted)
                {
                    Dummy = PauseMenu.Instance._pauseActivity;
                }
            }
        }

        public void GetStunned()
        {
            if (AbleToBeStunned && !Stunned)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonView.RPC("RPC_Stunned", RpcTarget.All);
                }
                else
                {
                    RPC_Stunned();
                }
            }
        }

        [PunRPC]
        public void RPC_Stunned()
        {
            Stunned = true;
            AbleToBeStunned = false;
            m_ASC.StunnedAnimation();
        }

        public IEnumerator StunDelay()
        {
            yield return new WaitForSeconds(m_StunInvincibility);
            AbleToBeStunned = true;
        }


        private void Update()
        {
            UnityUpdateTick?.Invoke(Time.deltaTime);
        }
    }

    public enum PlayerRoleEnum
    {
        Tagger,
        Runner
    }
}