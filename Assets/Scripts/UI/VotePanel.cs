using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class VotePanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;

    public void CreatePlayerItem(GameObject[] players)
    {
        List<GameObject> itemList = new List<GameObject>();
        //在 Content 里生成 _count 个item
        if (players.Length > 0)
        {
            int i = 0;
            item.SetActive(true); //第一个item实例已经放在列表第一个位置，直接激活
            itemList.Add(item);
            itemList[i].GetComponentInChildren<TMP_Text>().text = players[i].GetComponent<playerScript>().playerName;
            i++;

            while (i < players.Length)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //设置为 Content 的子对象
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); //获取前一个 item 的位置    
                                                                                 //当前 item 位置放在在前一个 item 下方    
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height-20, t.localPosition.z);
                a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                a.GetComponentInChildren<TMP_Text>().text = players[i].GetComponent<playerScript>().playerName;
                i++;
            }
            //根据当前 item 个数更新 Content 高度 
            content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height+20));
        }
        else
        {
            item.SetActive(false);
        }
    }

    public void VoteThisPlayer(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.highlightedColor = btn.colors.highlightedColor;
        cb.pressedColor = btn.colors.pressedColor;
        cb.disabledColor = btn.colors.disabledColor;
        cb.selectedColor = btn.colors.highlightedColor;
        btn.colors = cb;
    }
}
