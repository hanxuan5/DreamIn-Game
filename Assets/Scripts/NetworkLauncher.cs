using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    public GameObject roomName;
    public GameObject region;

    /// <summary>
    /// Connect to server
    /// </summary>
    public void PlayButton()
    {
        if (roomName.GetComponent<TMP_Text>().text.Length < 2)
        {
            return;
        }
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region.GetComponent<TMP_Text>().text;
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Connect to room
    /// </summary>
    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(roomName.GetComponent<TMP_Text>().text, options, default);
    }

    /// <summary>
    /// Load game level
    /// </summary>
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
