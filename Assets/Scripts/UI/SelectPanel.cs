using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;

public class SelectPanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;
    private ScriptsJsonData sj;

    private void Start()
    {
        StartCoroutine(GetNameAndID());
    }
    public void CreateScriptItem(List<ScriptInfo> scripts)
    {
        List<GameObject> itemList = new List<GameObject>();
        //在 Content 里生成 _count 个item
        if (scripts.Count > 0)
        {
            int i = 0;
            item.SetActive(true); //第一个item实例已经放在列表第一个位置，直接激活
            itemList.Add(item);
            itemList[i].GetComponentInChildren<Text>().text = scripts[i].name;
            itemList[i].GetComponent<ScrollIndexCallback1>().gameID = scripts[i].id.ToString();
            i++;

            while (i < scripts.Count)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //设置为 Content 的子对象
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); //获取前一个 item 的位置    
                                                                                 //当前 item 位置放在在前一个 item 下方    
                a.GetComponent<RectTransform>().localScale = t.localScale;
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height-20, t.localPosition.z);


                a.GetComponentInChildren<Text>().text = scripts[i].name;
                a.GetComponent<ScrollIndexCallback1>().gameID = scripts[i].id.ToString();
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

    IEnumerator GetNameAndID()
    {
        string url = "https://api.dreamin.land/game_name/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n");
            }
            else
            {
                sj = JsonMapper.ToObject<ScriptsJsonData>(webRequest.downloadHandler.text);
                CreateScriptItem(sj.result.Info);
            }
        }
    }
}
