using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;


namespace Player
{
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerTag : MonoBehaviour
    {
        public bool EnableGizmo = true;
        public Vector3 TagRange = Vector3.zero;
        public float GracePeriodDuration = 1.5f;
        public PhotonView PhotonView;
        public bool DoingTag;
        public LayerMask LineOfSightLayers;


        [SerializeField] private GameObject m_Model;
        private PlayerStatus m_PlayerStatus;
        private bool m_GracePeriod = false;
        [SerializeField] private AnimationStateController m_ASC;


        [SerializeField] private GameObject _tagHat;
        [SerializeField] private Transform m_LineOfSightStart;
        void Awake()
        {
            m_PlayerStatus = GetComponent<PlayerStatus>();
        }

        public void Tag(InputAction.CallbackContext context)
        {
            if (!PhotonView.IsMine && PhotonNetwork.IsConnected)
            {
                return;
            }

            if (m_PlayerStatus.PlayerRole == PlayerRoleEnum.Tagger && context.performed 
                && !m_GracePeriod && !m_PlayerStatus.Dummy && !DoingTag && !m_PlayerStatus.Stunned && !m_PlayerStatus.Busy)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonView.RPC("RPC_StartTag", RpcTarget.All);
                }
                else
                {
                    RPC_StartTag();
                }
            }
        }

        [PunRPC]
        private void RPC_StartTag()
        {
            DoingTag = true;
            m_PlayerStatus.Busy = true;
            m_ASC.TagAnimation();
        }


        public void DoTag()
        {
            if (PhotonNetwork.LocalPlayer == PhotonView.Owner || !PhotonNetwork.IsConnected)
            {
                // Boxcast infront
                RaycastHit[] hits = Physics.BoxCastAll(transform.position + Vector3.up, TagRange / 2, m_Model.transform.forward, Quaternion.identity, TagRange.z);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        continue;
                    }
                    PlayerTag tagComponent = hit.collider.gameObject.GetComponent<PlayerTag>();
                    if (tagComponent != null)
                    {
                        RaycastHit objectInTheWay;
                        if (!Physics.Raycast(m_LineOfSightStart.position, hit.collider.gameObject.transform.position - transform.position, out objectInTheWay, Vector3.Distance(m_LineOfSightStart.position, hit.transform.position), LineOfSightLayers))                          
                        {
                            tagComponent.GetTagged();
                            AudioManager.Instance.PlaySound(audioType.hit);
                            m_PlayerStatus.PlayerRole = PlayerRoleEnum.Runner;
                            if (!m_PlayerStatus.Dummy)
                            {
                                PlayerUI.Instance.UIRoleUpdate();
                            }
                            _tagHat.SetActive(false);

                            int tagBounty = hit.collider.gameObject.GetComponent<PlayerStatus>().Bounty.GetBounty();
                            if (tagBounty > 0)
                            {
                                m_PlayerStatus.Points += tagBounty;
                            }
                            break;
                        }
                        else
                        {
                            Debug.Log("No LOS hit " + objectInTheWay.collider.gameObject);
                        }
                    }
                }
            }
        }

        [PunRPC]
        public void RPC_GetTagged()
        {
            if (PhotonView.IsMine)
            {
                m_PlayerStatus.PlayerRole = PlayerRoleEnum.Tagger;
                if (!m_PlayerStatus.Dummy)
                {
                    PlayerUI.Instance.UIRoleUpdate();
                }
                StartCoroutine(GracePeriod());
                _tagHat.SetActive(true);
                int bounty = m_PlayerStatus.Bounty.GetBounty();
                if (bounty > 0)
                {
                    m_PlayerStatus.Points -= bounty;
                    m_PlayerStatus.Bounty.SetBounty(0);
                    m_PlayerStatus.Bounty.BountyCooldown = GameConstants.k_BOUNTYCOOLDOWN;
                }
            }
            m_PlayerStatus.GetStunned();
        }

        public void GetTagged()
        {
            if (m_PlayerStatus.PlayerRole == PlayerRoleEnum.Runner)
            {
                if (PhotonNetwork.IsConnected)
                {
                    PhotonView.RPC("RPC_GetTagged", RpcTarget.All);
                }
                else
                {
                    RPC_GetTagged();
                }
            }
        }

        IEnumerator GracePeriod()
        {
            m_GracePeriod = true;
            yield return new WaitForSeconds(GracePeriodDuration);
            m_GracePeriod = false;
        }

#if (UNITY_EDITOR)
        private void OnDrawGizmos()
        {
            if (EnableGizmo)
            {
                Vector3 center = transform.position + transform.forward;
                Gizmos.color = Color.red;
                Gizmos.DrawCube(center, TagRange);
                Gizmos.color = default;
            }
        }
#endif
    }
}
