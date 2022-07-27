using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class t_SclectPanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;
    private ScriptsJsonData sj;

    private void Start()
    {
        StartCoroutine(GetNameAndID());
    }

    /// <summary>
    /// previous method, for old script data format
    /// </summary>
    /// <param name="scripts"></param>
    //public void CreateScriptItem(List<ScriptInfo> scripts)
    //{
    //    List<GameObject> itemList = new List<GameObject>();
    //    //Generate _ Count items in content
    //    if (scripts.Count > 0)
    //    {
    //        int i = 0;
    //        item.SetActive(true); //Active the first one
    //        itemList.Add(item);

    //        string[] scriptText = scripts[i].name.Split(',');

    //        if (scriptText.Length > 1)
    //        {
    //            itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text = scriptText[0];
    //            if (scriptText[1] == "1")
    //                itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = scriptText[1] + " Player";
    //            else
    //                itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = scriptText[1] + " Players";
    //        }
    //        else
    //            itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text = scriptText[0];

    //        itemList[i].GetComponent<t_ScriptItem>().gameID = scripts[i].id.ToString();
    //        i++;

    //        while (i < scripts.Count)
    //        {
    //            GameObject a = GameObject.Instantiate(item) as GameObject;
    //            a.transform.parent = content.transform;//Set as a Content's child
    //            itemList.Add(a);
    //            RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); 

    //            a.GetComponent<RectTransform>().localScale = t.localScale;
    //            a.GetComponent<RectTransform>().localPosition =
    //             new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height - 50, t.localPosition.z);


    //            string[] text = scripts[i].name.Split(',');
    //            if (text.Length > 1)
    //            {
    //                a.transform.GetChild(0).GetComponent<TMP_Text>().text = text[0];
    //                if (text[1] == "1")
    //                    a.transform.GetChild(1).GetComponent<TMP_Text>().text = text[1] + " Player";
    //                else
    //                    a.transform.GetChild(1).GetComponent<TMP_Text>().text = text[1] + " Players";
    //            }
    //            else
    //                a.transform.GetChild(0).GetComponent<TMP_Text>().text = text[0];

    //            a.GetComponent<t_ScriptItem>().gameID = scripts[i].id.ToString();
    //            i++;
    //        }
    //        //Update content height
    //        content.GetComponent<RectTransform>().sizeDelta =
    //          new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height + 50));
    //    }
    //    else
    //    {
    //        item.SetActive(false);
    //    }
    //}

    public void CreateScriptItem(List<ScriptInfo> infos)
    {
        List<GameObject> itemList = new List<GameObject>();
        //Generate _ Count items in content
        if (infos.Count > 0)
        {
            int i = 0;
            item.SetActive(true); //Active the first one
            itemList.Add(item);

            itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text = infos[i].name;
            int playerNum = infos[i].players_num;
            if (playerNum > 1)
                itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = playerNum + " Player";
            else
                itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = playerNum + " Player";

            itemList[i].GetComponent<t_ScriptItem>().gameID = infos[i].id.ToString();
            i++;

            while (i < infos.Count)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //Set as a Content's child
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>();

                a.GetComponent<RectTransform>().localScale = t.localScale;
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height - 50, t.localPosition.z);


                itemList[i].transform.GetChild(0).GetComponent<TMP_Text>().text = infos[i].name;
                int num = infos[i].players_num;
                if (playerNum > 1)
                    itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = num + " Player";
                else
                    itemList[i].transform.GetChild(1).GetComponent<TMP_Text>().text = num + " Player";

                itemList[i].GetComponent<t_ScriptItem>().gameID = infos[i].id.ToString();
                i++;
            }
            //Update content height
            content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height + 50));
        }
        else
        {
            item.SetActive(false);
        }
    }
    IEnumerator GetNameAndID()
    {
        string url = "https://api.dreamin.land/info_get/";//get all scripts info
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n");
            }
            else
            {
                //Save a data backup for debug
                string savePath = "Assets/JsonData/TestScriptData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));

                sj = JsonMapper.ToObject<ScriptsJsonData>(webRequest.downloadHandler.text);

                CreateScriptItem(sj.infos);
            }
        }
    }

    void PassButton()
    {

    }
    void NOPassButton()
    {

    }
}
