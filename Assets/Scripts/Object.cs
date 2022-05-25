using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;


public class Object : MonoBehaviour
{
    public GameObject message;
    PhotonView photonView;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && message.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text.Length > 0)
        {
            message.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            message.SetActive(false);
        }
    }

    public void HideButton()
    {
        message.SetActive(false);
    }

    public void SetInfoText(string info)
    {
        message.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = info;
    }
}
