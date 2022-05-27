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

    public void PlayButton()
    {
        if (roomName.GetComponent<TMP_Text>().text.Length < 2)
        {
            return;
        }
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region.GetComponent<TMP_Text>().text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(roomName.GetComponent<TMP_Text>().text, options, default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
