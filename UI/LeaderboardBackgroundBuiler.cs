using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace UI
{
    public class LeaderboardBackgroundBuiler : MonoBehaviour
    {
        [SerializeField] private GameObject m_TopObject;
        [SerializeField] private GameObject m_BottomObject;
        [SerializeField] private GameObject[] m_MiddlePieces;
        [SerializeField] private GameObject m_ParentObject;

        private GameObject[] m_BackgroundPieces;
        
        private void Start()
        {
            if (!PhotonNetwork.IsConnected)
            {
                return;
            }
            m_BackgroundPieces = new GameObject[PhotonNetwork.CurrentRoom.PlayerCount];
            m_BackgroundPieces[0] = m_TopObject;
            GenerateBackground();
        }


        private void GenerateBackground()
        {
            // We can skip the first cycle since the top object is placed properly already 
            for (int i = 1; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (i == PhotonNetwork.CurrentRoom.PlayerCount - 1)
                {
                    // Last player
                    m_BackgroundPieces[i] = m_BottomObject;
                }
                else
                {
                    // Middle piece
                    GameObject piece = Instantiate(m_MiddlePieces[Random.Range(0, m_MiddlePieces.Length)],m_ParentObject.transform);
                    piece.transform.SetSiblingIndex(i);                    
                    m_BackgroundPieces[i] = piece;
                }
            }
        }
    }
}