using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject readyButton;
    public GameObject scriptScroll;
    public GameObject playerPrefab;
    public GameObject canvas;

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
        //Instantiate(playerPrefab, canvas.transform.position, Quaternion.identity, canvas.transform);
        //PhotonNetwork.Instantiate("Player", new Vector3(1, 1, 0), Quaternion.identity, 0);
    }
}
