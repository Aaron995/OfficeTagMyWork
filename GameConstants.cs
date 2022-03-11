using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    // Movement related
    public const string k_KEYBOARDSCHEMENAME = "KeyboardMouse";
    public const string k_GAMEPADSCHEMENAME = "Gamepad";

    // Bounties related
    // The minimum points difference needed for the bounty to become active
    public const int k_MINIMUMPOINTSDIFFERENCE = 24;
    // The procent of player points taken when the bounty gets claimed always gets rounded down 
    public const float k_PROCENTOFPOINTSDIFFERENCEASBOUNTY = 30f;
    public const float k_BOUNTYCOOLDOWN = 10f;

    // Raise events related
    public const byte k_GETSPAWNLOCATIONEVENTCODE = 1;
    public const byte k_REQUESTSPAWNLOCATIONEVENTCODE = 2;
    public const byte k_SENDLEADERBOARDUPDATEEVENTCODE = 3;
    public const byte k_SENDFORCETAGGEREVENTCODE = 4;
    public const byte k_SENDSTARTGAMEEVENTCODE = 5;
    public const byte k_SENDENDGAMEEVENTCODE = 6;
    public const byte k_SENDNEWMASTERINFOEVENTCODE = 7;
    public const byte k_SENDSETUPDONEEVENTCODE = 8;
    public const byte k_SENDTIMERUPDATEEVENTCODE = 9;
    public const byte k_SENDPOINTADDEVENTCODE = 10;
}
