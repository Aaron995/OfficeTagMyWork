using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Player;
using Photon.Realtime;
using Networking.RaiseEvents;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks
    {
        public static GameManager Instance;

        public List<Player> Players = new List<Player>();
        public List<Transform> SpawnLocations = new List<Transform>();
        public int GameDuration = 120;
        public bool GameStarted = false;
        public PhotonView[] PVObjs;

        [HideInInspector] public UnityEvent GameStart = new UnityEvent();
        [HideInInspector] public UnityEvent GameEnd = new UnityEvent();

        [SerializeField] private bool m_SpawnLocationDone = false;
        private int m_PlayersDoneSetup = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        #region Local Funcs
        private void Start()
        {
            if (PhotonNetwork.IsConnected)
            {
                StartCoroutine(Setup());
            }
            else
            {
                PVObjs = FindObjectsOfType<PhotonView>();
                StartGame();
            }
        }

        private IEnumerator Setup()
        {
            // Make sure we have all players in our player list
            while (Players.Count != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                yield return new WaitForEndOfFrame();
                GetPlayers();
            }

            // Wait until we have our spawn location
            yield return new WaitUntil(() => m_SpawnLocationDone == true);            


            // Find all the PhotonViews in the room
            PVObjs = FindObjectsOfType<PhotonView>();

            // Make leaderboard
            PlayerUI.Instance.MakeLabels();

            PlayerUI.Instance.SetupGameTimer();

            // Wait a second to make sure every is synced
            yield return new WaitForSeconds(1);
            // Finsh setup
            if (PhotonNetwork.IsMasterClient)
            {
                PlayerDoneSetup();
            }
            else
            {
                RaiseEvents.SendSetupDoneEventCode();
            }
        }

        public void GetPlayers()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                AddPlayer(player.gameObject);
            }
        }

        public void MasterClientStart()
        {
            RaiseEvents.SendForceTaggerEventCode(PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length)].ActorNumber);
            RaiseEvents.SendStartGameEventCode();
        }

        public void StartGame()
        {
            GameStart.Invoke();
            StartCoroutine(CountGameDuration());
            GameStarted = true;            
        }

        public IEnumerator CountGameDuration()
        {
            if (PhotonNetwork.IsConnected)
            {
                GameDuration = RoomManager.Instance.gameTime;
            }

            while (GameDuration > 0)
            {
                yield return new WaitForSeconds(1);
                if (PhotonNetwork.IsMasterClient)
                {
                    GameDuration--;
                    RaiseEvents.SendTimerUpdateEventCode(GameDuration);

                    if (Players.Count == 1)
                    {
                        RaiseEvents.SendEndGameEventCode();
                    }
                }
            }

            if (Players.Count > 1)
            {
                RaiseEvents.SendEndGameEventCode();
            }
        }

        public void EndGame()
        {
            GameEnd.Invoke();
            PhotonNetwork.Disconnect();
            Destroy(FindObjectOfType<RoomManager>().gameObject);

            EndGameTransfer.Instance.playerStats = GetPlayerStats();

            PhotonNetwork.AutomaticallySyncScene = false;
            SceneManager.LoadScene(0);
        }

        public void AddPlayer(GameObject playerObject)
        {
            foreach (Player player in Players)
            {
                if (player.PlayerObject == playerObject)
                {
                    return;
                }
            }
            Players.Add(new Player(playerObject));
        }

        public void RemovePlayer(Photon.Realtime.Player otherPlayer)
        {
            foreach (Player player in Players)
            {
                if (player.PV.ControllerActorNr == otherPlayer.ActorNumber)
                {
                    if (player.PlayerStatus.PlayerRole == PlayerRoleEnum.Tagger)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            RaiseEvents.SendForceTaggerEventCode(PhotonNetwork.PlayerList[Random.Range(0, PhotonNetwork.PlayerList.Length)].ActorNumber);
                        }
                    }

                    Players.Remove(player);
                    break;
                }
            }
        }

        public List<PlayerStats> GetPlayerStats()
        {
            List<PlayerStats> returnList = new List<PlayerStats>();
            foreach (Player player in Players)
            {
                if (player.PlayerObject != null)
                {
                    returnList.Add(new PlayerStats(player.PV.Owner.NickName, player.PlayerStatus.Points, player.PV));
                }
            }

            return returnList;
        }

        public Transform GetSpawnPos()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int randomNum = Random.Range(0, SpawnLocations.Count);
                Transform selected = SpawnLocations[randomNum];
                SpawnLocations.RemoveAt(randomNum);
                m_SpawnLocationDone = true;
                return selected;
            }
            else
            {
                RaiseEvents.SendRequestSpawnLocationEventCode(PhotonNetwork.LocalPlayer.ActorNumber);
                return null;
            }
        }

        public void GotSpawnPos()
        {
            m_SpawnLocationDone = true;
        }

        public GameObject GetObjWithViewID(int ID)
        {
            foreach (PhotonView pv in GameManager.Instance.PVObjs)
            {
                if (pv.ViewID == ID)
                {
                    return pv.gameObject;
                }
            }
            Debug.LogWarning("Obj not found with View ID " + ID);
            return null;
        }

        #endregion
        #region Networking Funcs

        public void PlayerDoneSetup()
        {
            m_PlayersDoneSetup += 1;
            if (m_PlayersDoneSetup >= PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.IsMasterClient)
            {
                MasterClientStart();
            }
        }

        public void SendSpawnPos(int actorNr)
        {
            int randomNum = Random.Range(0, SpawnLocations.Count);
            Transform selected = SpawnLocations[randomNum];
            SpawnLocations.RemoveAt(randomNum);
            RaiseEvents.SendGetSpawnLocationEventCode(selected.position, actorNr);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            RemovePlayer(otherPlayer);
        }

        #endregion
    }

#if (UNITY_EDITOR)
    [System.Serializable]
#endif
    public struct Player
    {
        public Player(GameObject playerObject)
        {
            PlayerObject = playerObject;
            PV = playerObject.GetComponent<PhotonView>();
            PlayerStatus = playerObject.GetComponent<PlayerStatus>();
        }

        public GameObject PlayerObject;
        public PhotonView PV;
        public PlayerStatus PlayerStatus; 
    }

    public struct PlayerStats
    {
        public PlayerStats(string name, int points, PhotonView pv)
        {
            NickName = name;
            Points = points;
            PV = pv;
        }

        public string NickName;
        public int Points;
        public PhotonView PV;
    }
}