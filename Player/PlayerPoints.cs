using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Photon.Pun;
using Networking.RaiseEvents;

namespace Player
{
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerPoints : MonoBehaviour
    {
        public int PointsPerTick = 1;
        public float TickInterval = 1f;

        private Coroutine m_ScoreCoroutine;
        private PlayerStatus m_PlayerStatus;
        private void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (GetComponent<PhotonView>().IsMine)
                {
                    m_PlayerStatus = GetComponent<PlayerStatus>();
                    GameManager.Instance.GameStart.AddListener(GameStart);
                    GameManager.Instance.GameEnd.AddListener(GameEnd);
                }
            }
        }

        public void GameStart()
        {
            m_ScoreCoroutine = StartCoroutine(AddPoints());
        }

        IEnumerator AddPoints()
        {
            while (true)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    RaiseEvents.SendPointAddEventCode();
                }
                yield return new WaitForSeconds(TickInterval);
            }
        }

        public void GameEnd()
        {
            StopCoroutine(m_ScoreCoroutine);
        }
    }
}