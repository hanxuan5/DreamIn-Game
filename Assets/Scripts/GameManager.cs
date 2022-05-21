using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GameManager : MonoBehaviourPunCallbacks
{ 
    public GameObject readyButton;
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            scriptScroll.SetActive(true);
        }
    }

    public void ReadyButton()
    {
        readyButton.SetActive(false);

        GameObject player = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
        player.GetComponent<playerScript>().SetPlayerName("Set a new name");

        GameObject obj = PhotonNetwork.Instantiate("Object", canvas.transform.position + new Vector3(100, 100, 100), Quaternion.identity, 0);
        obj.GetComponent<Object>().SetInfoText("new info");

        GameObject wall = PhotonNetwork.Instantiate("Wall", canvas.transform.position - new Vector3(100, 100, 100), Quaternion.identity, 0);
    }


}
