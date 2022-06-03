using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;


public class Object : MonoBehaviour
{
    public GameObject tipText;
    public GameObject objectInfoPanel;
    private PhotonView photonView;
    private bool isInterable=false;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if(isInterable)
        {
            if(Input.GetKeyDown(KeyCode.X))
            {
                objectInfoPanel.SetActive(true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")&& collision.gameObject.GetComponent<playerScript>().photonView.IsMine)
        {
            tipText.SetActive(true);
            isInterable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")&& collision.gameObject.GetComponent<playerScript>().photonView.IsMine)
        {
            tipText.SetActive(false);
            isInterable = false;
        }
    }

    public void HideButton()
    {
        tipText.SetActive(false);
    }

    public void SetInfoText(string info)
    {
        objectInfoPanel.GetComponentInChildren<TMP_Text>().text = info;
        
    }
}
