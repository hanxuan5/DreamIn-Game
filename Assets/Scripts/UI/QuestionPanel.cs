using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class QuestionPanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;
    public TMP_Text TitleText;
    public TMP_Text QuestionText;
    public TMP_Text TimerText;
    public Button selectButton;
    public Button resultButton;

    internal string selectedAnswer;
    private int countTime;

    [SerializeField]
    private int playerSelectedNum;//Record the number of people who have voted
    private int playerNum;
    private List<GameObject> itemList;
    Dictionary<string, int> selectData;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        selectData = new Dictionary<string, int>();
    }
    private void OnEnable()
    {
        StartCountTime(0.5f);
        playerNum = GameObject.FindGameObjectsWithTag("Player").Length;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && playerSelectedNum == playerNum)
        {
            photonView.RPC("RPCShowSelectResult", RpcTarget.All);
            playerSelectedNum = 0;
        }
    }
    public void SetQuestion(string question)
    {
        QuestionText.text = question;
    }

    public void ResetPanel()
    {
        {//reset item[0]
            Button btn = itemList[0].GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = btn.colors.disabledColor;
            cb.highlightedColor = btn.colors.highlightedColor;
            cb.pressedColor = btn.colors.pressedColor;
            cb.disabledColor = btn.colors.disabledColor;
            cb.selectedColor = btn.colors.highlightedColor;
            btn.colors = cb;

            itemList[0].transform.GetChild(0).gameObject.SetActive(true);
            itemList[0].transform.GetChild(1).GetComponent<TMP_Text>().text = "";
        }
        //clear itemlist except item[0]
        for (int i = 1; i < itemList.Count; i++)
            Destroy(itemList[i]);
        itemList.Clear();

        resultButton.gameObject.SetActive(false);
        selectButton.gameObject.SetActive(true);

        selectData.Clear();
    }
    public void CreateAnswerItem(List<string> answers)
    {
        itemList = new List<GameObject>();

        if (answers.Count> 0)
        {
            int i = 0;
            item.SetActive(true);
            itemList.Add(item);

            itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text = answers[i];
            selectData.Add(answers[i], 0);
            i++;

            while (i < answers.Count)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform;
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>();

                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height - 20, t.localPosition.z);
                a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                a.transform.GetChild(0).GetComponent<TMP_Text>().text = answers[i];
                selectData.Add(answers[i], 0);
                i++;
            }

            content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height + 20));
        }
        else
        {
            item.SetActive(false);
        }
    }

    public void ChooseThisAnswer(Button btn)
    {
        {//Select the player and change the button color
            ColorBlock cb = btn.colors;
            cb.highlightedColor = btn.colors.highlightedColor;
            cb.pressedColor = btn.colors.pressedColor;
            cb.disabledColor = btn.colors.disabledColor;
            cb.selectedColor = btn.colors.highlightedColor;
            btn.colors = cb;
        }
        selectedAnswer = btn.GetComponentInChildren<TMP_Text>().text;
    }

    public void SelectButton()
    {
        FinishSelect();
        selectButton.gameObject.SetActive(false);
    }

    void FinishSelect()
    {
        photonView.RPC("RPCAddPlayerNum", RpcTarget.All);
        if (selectedAnswer != null)
            photonView.RPC("RPCAddSelectData", RpcTarget.All, selectedAnswer);
    }
    [PunRPC]
    void RPCShowSelectResult()
    {
        GameObject maxItem = null;
        int maxNum = 0;
        foreach (var item in itemList)
        {
            GameObject ansTextObj = item.transform.Find("AnswerText").gameObject;
            string ans = ansTextObj.GetComponent<TMP_Text>().text;
            foreach (var data in selectData)
            {
                if (data.Key == ans)
                {
                    item.transform.GetChild(1).GetComponent<TMP_Text>().text = ans + ": " + data.Value.ToString();
                    ansTextObj.SetActive(false);
                    if (data.Value > maxNum)
                    {
                        maxNum = data.Value;
                        maxItem = item;
                    }
                }
            }
        }
        if (maxItem != null)//Highlight the answer with the largest number of votes
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
        TitleText.text = "Select Result";

        selectButton.gameObject.SetActive(false);
        resultButton.gameObject.SetActive(true);

        StopCountTime();
    }

    [PunRPC]
    void RPCAddPlayerNum()
    {
        playerSelectedNum++;
    }

    [PunRPC]
    void RPCAddSelectData(string ans)
    {
        selectData[ans]++;
    }


    #region Voting board timing
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
                FinishSelect();
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
