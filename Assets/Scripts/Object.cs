using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Object : MonoBehaviour
{

    Text infoText;
    void Start()
    {
        infoText = GetComponentInChildren<Text>();
        infoText.enabled=false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            infoText.enabled = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            infoText.enabled = false;
        }
    }
}
