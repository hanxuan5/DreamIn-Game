using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    public GameObject LoginUI;
    public GameObject RoomUI;
    public InputField roomName;

    public override void OnConnectedToMaster()
    {
        RoomUI.SetActive(true);
    }

    public void PlayButton()
    {
        PhotonNetwork.ConnectUsingSettings();
        LoginUI.SetActive(false);
    }

    public void JoinOrCreateButton()
    {
        if (roomName.text.Length < 2)
        {
            return;
        }

        RoomUI.SetActive(false);

        RoomOptions options = new RoomOptions { MaxPlayers = 10 };
        PhotonNetwork.JoinOrCreateRoom(roomName.text, options, default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
