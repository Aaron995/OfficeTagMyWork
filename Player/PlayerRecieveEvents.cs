using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Networking
{    
    public class PlayerRecieveEvents : MonoBehaviour, IOnEventCallback
    {
        [SerializeField]private GameObject _tagHat;

        private PlayerStatus m_PlayerStatus;
        private PlayerPoints m_PlayerPoints;

        private void Start()
        {
            m_PlayerStatus = GetComponent<PlayerStatus>();
            m_PlayerPoints = GetComponent<PlayerPoints>();
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == GameConstants.k_GETSPAWNLOCATIONEVENTCODE)
            {
                object[] data = (object[])photonEvent.CustomData;

                transform.position = (Vector3)data[0];

                Game.GameManager.Instance.GotSpawnPos();
            }
            else if (eventCode == GameConstants.k_SENDFORCETAGGEREVENTCODE)
            {
                m_PlayerStatus.PlayerRole = PlayerRoleEnum.Tagger;
                PlayerUI.Instance.UIRoleUpdate();
                _tagHat.SetActive(true);
            }
            else if (eventCode == GameConstants.k_SENDPOINTADDEVENTCODE)
            {
                if (m_PlayerStatus.PlayerRole == PlayerRoleEnum.Runner)
                {
                    m_PlayerStatus.Points += m_PlayerPoints.PointsPerTick;
                }
            }
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

}