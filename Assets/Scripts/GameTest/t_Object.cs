using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class t_Object : MonoBehaviour
{
    public GameObject tipText;
    public GameObject objectInfoPanel;
    private bool isInterable = false;
    private void Update()
    {
        if (isInterable)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                objectInfoPanel.SetActive(true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            tipText.SetActive(true);
            isInterable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
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
