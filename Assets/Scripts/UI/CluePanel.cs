using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class CluePanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;
    public TMP_Text DetailInfoText;
    List<GameObject> itemList = new List<GameObject>();
    public List<string> clueList = new List<string>();

    public PhotonView photonView;
    public void AddClue(string clue)
    {
        photonView.RPC("RPCAddClue", RpcTarget.All,clue);
    }
    [PunRPC]
    public void RPCAddClue(string clue)
    {
        if(itemList.Count==0)
        {
            item.SetActive(true);
            item.GetComponentInChildren<TMP_Text>().text = clue;
            item.SetActive(true);
            itemList.Add(item);
        }
        else
        {
            GameObject a = GameObject.Instantiate(item) as GameObject;
            a.transform.parent = content.transform;
            a.GetComponentInChildren<TMP_Text>().text = clue;

            RectTransform t = itemList[itemList.Count-1].GetComponent<RectTransform>();
            a.GetComponent<RectTransform>().localPosition =
             new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height - 20, t.localPosition.z);
            a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            itemList.Add(a);
        }

        clueList.Add(clue);

        content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height + 20));

    }

    public void ShowDetailClue(TMP_Text clue)
    {
        DetailInfoText.text = clue.text;
        DetailInfoText.transform.parent.parent.gameObject.SetActive(true);
    }
}
