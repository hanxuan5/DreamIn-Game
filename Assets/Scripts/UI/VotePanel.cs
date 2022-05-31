using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class VotePanel : MonoBehaviour, IPunObservable
{
    public GameObject content;
    public GameObject item;
    public TMP_Text TimerText;
    public TMP_Text FinalText;

    internal string selectedName;
    private int countTime;
    private PhotonView photonView;

    Dictionary<string, int> voteData;

    

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        voteData = new Dictionary<string, int>();
    }
    private void OnEnable()
    {
        StartCountTime(0.5f);
    }
    public void CreatePlayerItem(GameObject[] players)
    {
        List<GameObject> itemList = new List<GameObject>();
        //在 Content 里生成 _count 个item
        if (players.Length > 0)
        {
            int i = 0;
            item.SetActive(true); //第一个item实例已经放在列表第一个位置，直接激活
            itemList.Add(item);

            string playerName = players[i].GetComponent<playerScript>().playerName;
            itemList[i].GetComponentInChildren<TMP_Text>().text = playerName;
            voteData.Add(playerName, 0);
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

                string name= players[i].GetComponent<playerScript>().playerName;
                a.GetComponentInChildren<TMP_Text>().text = name;
                voteData.Add(name, 0);
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
        {//选中该按钮，改变其颜色为选中状态
            ColorBlock cb = btn.colors;
            cb.highlightedColor = btn.colors.highlightedColor;
            cb.pressedColor = btn.colors.pressedColor;
            cb.disabledColor = btn.colors.disabledColor;
            cb.selectedColor = btn.colors.highlightedColor;
            btn.colors = cb;
        }
        selectedName = btn.GetComponentInChildren<TMP_Text>().text;
    }

    public void VoteButton()
    {
        FinishVote();
    }

    void FinishVote()
    {
        if (selectedName == null) return;
        voteData[selectedName]++;

        string t = "";
        foreach(var it in voteData)
        {
            t += it.Key + ": " + it.Value+"\n";
        }
        FinalText.text = t;
    }

    /// <summary>
    /// 同步方法，同步投票数信息
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting == true)
        {
            stream.SendNext(voteData);
        }
        else
        {
            voteData = (Dictionary<string,int>)stream.ReceiveNext();
        }
    }

    #region 投票扳的计时部分
    void StartCountTime(float t)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //t是分钟
            countTime = (int)(t * 60);
            StartCoroutine(CountTime());
        }
    }

    IEnumerator CountTime()
    {
        while (true)
        {
            countTime -= 1;
            photonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
            if (countTime == 0)
            {
                FinishVote();
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    [PunRPC]
    void RPCSetTimerText(int t)
    {
        string s = "";
        if (t >= 3600)
        {
            int hour = t / 3600;
            t = t % 3600;
            int min = t / 60;
            int sec = t % 60;
            if (min >= 10)
            {
                if (sec >= 10)
                    s = "" + hour + ":" + min + ":" + sec;
                else
                    s = "" + hour + ":" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "" + hour + ":0" + min + ":" + sec;
                else
                    s = "" + hour + ":0" + min + ":0" + sec;
            }
            TimerText.GetComponent<TMP_Text>().text = s;
        }
        else if (t > 60)
        {
            int min = t / 60;
            int sec = t % 60;

            if (min >= 10)
            {
                if (sec >= 10)
                    s = "" + min + ":" + sec;
                else
                    s = "" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "0" + min + ":" + sec;
                else
                    s = "0" + min + ":0" + sec;
            }
            TimerText.GetComponent<TMP_Text>().text = s;
        }
        else
        {
            TimerText.GetComponent<TMP_Text>().text = "" + t;
        }
    }
    #endregion
}
