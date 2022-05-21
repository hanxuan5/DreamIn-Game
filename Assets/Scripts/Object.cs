using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class Object : MonoBehaviour
{
    public Text infoText;
    PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        HideInfoText();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            ShowInfoText();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            HideInfoText();
        }
    }

    public void ShowInfoText()
    {
        infoText.gameObject.SetActive(true);
    }

    public void HideInfoText()
    {
        infoText.gameObject.SetActive(false);
    }

    public void SetInfoText(string info)
    {
        if(!photonView)
            photonView = GetComponent<PhotonView>();

        photonView.RPC("RPCSetInfoText", RpcTarget.All, info);
    }

    [PunRPC]
    void RPCSetInfoText(string info)
    {
        infoText.text = info;
    }
}
