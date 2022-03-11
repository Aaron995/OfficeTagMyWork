using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using Photon.Pun;
using System;

namespace Player
{
    public class PlayerBounty 
    {
        public Action<int> OnBountyChange;
        public PlayerStatus PlayerStatus;
        public float BountyCooldown;

        private GameObject m_BountyParticles;
        private Photon.Realtime.Player m_Owner;
        [SerializeField] private int m_Bounty;

        public PlayerBounty(Photon.Realtime.Player owner, PlayerStatus ps, GameObject bountyParticles)
        {
            m_Owner = owner;
            m_Bounty = 0;
            BountyCooldown = 0;
            PlayerStatus = ps;
            m_BountyParticles = bountyParticles;
        }

        public void Update(float deltaTime)
        {
            if (GameManager.Instance.GameStarted && PlayerStatus.PlayerRole == PlayerRoleEnum.Runner && BountyCooldown <= 0)
            {
                int localPlayerPoints = 0;
                int lastPlacePoints = int.MaxValue;
                foreach (Game.Player player in GameManager.Instance.Players)
                {
                    if (player.PV.Owner == m_Owner)
                    {
                        localPlayerPoints = player.PlayerStatus.Points;
                        if (lastPlacePoints > localPlayerPoints) 
                        {
                            lastPlacePoints = localPlayerPoints;
                        }
                    }
                    else if (lastPlacePoints > player.PlayerStatus.Points)
                    {
                        lastPlacePoints = player.PlayerStatus.Points;
                    }
                }

                int difference = localPlayerPoints - lastPlacePoints;
                if (difference >= GameConstants.k_MINIMUMPOINTSDIFFERENCE)
                {
                    int newBounty = Mathf.FloorToInt(difference / 100f * GameConstants.k_PROCENTOFPOINTSDIFFERENCEASBOUNTY);
                    if (newBounty > m_Bounty)
                    {
                        SetBounty(newBounty);
                    }
                }
            }
            else if (BountyCooldown > 0)
            {
                BountyCooldown -= deltaTime;
            }
        }

        public int GetBounty()
        {
            return m_Bounty;
        }

        public void SetBounty(int value)
        {
            if (value != m_Bounty)
            {
                m_Bounty = value;
                OnBountyChange?.Invoke(m_Bounty);
                if (m_Bounty > 0)
                {
                    m_BountyParticles.SetActive(true);
                }
                else
                {
                    m_BountyParticles.SetActive(false);
                }
            }
        }
    }
}