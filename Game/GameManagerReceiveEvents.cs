using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Networking
{
    public class GameManagerReceiveEvents : MonoBehaviour, IOnEventCallback
    {
        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            // Request for a spawn location
            if (eventCode == GameConstants.k_REQUESTSPAWNLOCATIONEVENTCODE && PhotonNetwork.IsMasterClient)
            {
                object[] data = (object[])photonEvent.CustomData;
                int actorNr = (int)data[0];
                GameManager.Instance.SendSpawnPos(actorNr);
            }
            // Starting game
            else if (eventCode == GameConstants.k_SENDSTARTGAMEEVENTCODE)
            {
                GameManager.Instance.StartGame();
            }
            // Ending game
            else if (eventCode == GameConstants.k_SENDENDGAMEEVENTCODE)
            {
                GameManager.Instance.EndGame();
            }
            else if (eventCode == GameConstants.k_SENDSETUPDONEEVENTCODE)
            {
                GameManager.Instance.PlayerDoneSetup();
            }
            // Gametimer sync
            else if (eventCode == GameConstants.k_SENDTIMERUPDATEEVENTCODE)
            {
                object[] data = (object[])photonEvent.CustomData;
                int gameDuration = (int)data[0];
                GameManager.Instance.GameDuration = gameDuration;
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