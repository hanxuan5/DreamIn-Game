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
    public TMP_Text TitleText;
    public TMP_Text TimerText;
    public Button voteButton;
    public Button voteResultButton;

    internal string selectedName;
    private int countTime;

    [SerializeField]
    private int voteNum;//记录已经投票的人数
    private List<GameObject> itemList;
    Dictionary<string, int> voteData;
    
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        voteData = new Dictionary<string, int>();
    }
    private void OnEnable()
    {
        StartCountTime(0.5f);
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && voteNum == itemList.Count)
        {
            photonView.RPC("RPCShowVoteResult", RpcTarget.All);
            voteNum = 0;
        }
    }
    public void CreatePlayerItem(GameObject[] players)
    {
        itemList = new List<GameObject>();

        if (players.Length > 0)
        {
            int i = 0;
            item.SetActive(true); 
            itemList.Add(item);

            string playerName = players[i].GetComponent<PlayerScript>().playerName;
            itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text= playerName;
            voteData.Add(playerName, 0);
            i++;

            while (i < players.Length)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; 
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); 
                
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height-20, t.localPosition.z);
                a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                string name= players[i].GetComponent<PlayerScript>().playerName;
                a.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
                voteData.Add(name, 0);
                i++;
            }

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
        {//选中该玩家，改变其颜色
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
        voteButton.gameObject.SetActive(false);
    }

    void FinishVote()
    {
        photonView.RPC("RPCAddVoteNum", RpcTarget.All);
        if (selectedName != null)
            photonView.RPC("RPCAddVoteData", RpcTarget.All,selectedName);
    }
    [PunRPC]
    void RPCShowVoteResult()
    {
        GameObject maxItem = null;
        int maxNum = 0;
        foreach (var item in itemList)
        {
            GameObject nameTextObj = item.transform.Find("NameText").gameObject;
            string name = nameTextObj.GetComponent<TMP_Text>().text;
            foreach (var data in voteData)
            {
                if (data.Key == name)
                {
                    item.transform.GetChild(1).GetComponent<TMP_Text>().text = name+": " + data.Value.ToString() + " vote";
                    nameTextObj.SetActive(false);
                    if (data.Value > maxNum)
                    {
                        maxNum = data.Value;
                        maxItem = item;
                    }
                }
            }
        }
        if (maxItem != null)//高亮最大票数的人
        {
            Button btn = maxItem.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = btn.colors.highlightedColor;
            cb.highlightedColor = btn.colors.highlightedColor;
            cb.pressedColor = btn.colors.pressedColor;
            cb.disabledColor = btn.colors.disabledColor;
            cb.selectedColor = btn.colors.highlightedColor;

            maxItem.GetComponent<Button>().colors = cb;
        }

        TimerText.gameObject.SetActive(false);
        TitleText.text = "Vote Result";

        voteButton.gameObject.SetActive(false);
        voteResultButton.gameObject.SetActive(true);

        StopCountTime();
    }

    [PunRPC]
    void RPCAddVoteNum()
    {
        voteNum++;
    }

    [PunRPC]
    void RPCAddVoteData(string name)
    {
        voteData[name]++;
    }

    ///// <summary>
    ///// 使用photon传输流同步数据
    ///// </summary>
    ///// <param name="stream"></param>
    ///// <param name="info"></param>
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    //if (stream.IsWriting == true)
    //    //{
    //    //    stream.SendNext(voteData);
    //    //}
    //    //else
    //    //{
    //    //    voteData = (Dictionary<string,int>)stream.ReceiveNext();
    //    //}
    //}

    #region 投票板计时
    IEnumerator IECountTime;
    void StartCountTime(float t)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            countTime = (int)(t * 60);
            IECountTime = CountTime();
            StartCoroutine(IECountTime);
        }
    }

    void StopCountTime()
    {
        if (PhotonNetwork.IsMasterClient)
            StopCoroutine(IECountTime);
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
