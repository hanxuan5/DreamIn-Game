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
    public string objectInfo;
    private bool isInterable=false;
    private void Update()
    {
        if(isInterable)
        {
            if(Input.GetKeyDown(KeyCode.X))
            {
                SetInfoText(objectInfo);
                objectInfoPanel.SetActive(true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")&& collision.gameObject.GetComponent<PlayerScript>().photonView.IsMine)
        {
            if (objectInfo == "") return;
            tipText.SetActive(true);
            isInterable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (objectInfo == "") return;
        if(collision.CompareTag("Player")&& collision.gameObject.GetComponent<PlayerScript>().photonView.IsMine)
        {
            tipText.SetActive(false);
            isInterable = false;
        }
    }

    public void HideButton()
    {
        tipText.SetActive(false);
    }
    public void SetInfo(string info)
    {
        objectInfo = info;
    }    
    public void SetInfoText(string info)
    {
        objectInfoPanel.GetComponentInChildren<TMP_Text>().text = info;
        objectInfoPanel.GetComponentInChildren<TMP_Text>().text = objectInfo;
    }
}
