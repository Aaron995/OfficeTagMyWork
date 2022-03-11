 using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Networking.RaiseEvents
{
    public static class RaiseEvents
    {
        public static void SendRequestSpawnLocationEventCode(int actorNr)
        {
            object[] content = new object[] { actorNr };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(GameConstants.k_REQUESTSPAWNLOCATIONEVENTCODE, content, raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendGetSpawnLocationEventCode(Vector3 spawnPos, int actorNr)
        {
            object[] content = new object[] { spawnPos };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] { actorNr} };
            PhotonNetwork.RaiseEvent(GameConstants.k_GETSPAWNLOCATIONEVENTCODE, content, raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendForceTaggerEventCode(int actorNr)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] { actorNr } };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDFORCETAGGEREVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendStartGameEventCode()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDSTARTGAMEEVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendEndGameEventCode()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDENDGAMEEVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendNewMasterInfoEventCode(object[] info, int newMasterActorNr)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] { newMasterActorNr } };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDNEWMASTERINFOEVENTCODE, info, raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendLeaderboardUpdateEventCode()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDLEADERBOARDUPDATEEVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendSetupDoneEventCode()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDSETUPDONEEVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendTimerUpdateEventCode(int duration)
        {
            object[] content = new object[] { duration };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDTIMERUPDATEEVENTCODE, content, raiseEventOptions, SendOptions.SendReliable);
        }

        public static void SendPointAddEventCode()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameConstants.k_SENDPOINTADDEVENTCODE, new object[0], raiseEventOptions, SendOptions.SendReliable);
        }
    }
}