using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;

public class ScrollIndexCallback1 : MonoBehaviour 
{
    public Image image;
	public Text text;
    public string gameID;
    public int numOfPlayer;

    private GameData gameData;
    void ScrollCellIndex (int idx) 
    {
        if (image != null)
        {
            image.color = Rainbow(idx / 50.0f);
        }
		gameObject.name = name;
	}

    // http://stackoverflow.com/questions/2288498/how-do-i-get-a-rainbow-color-gradient-in-c
    public static Color Rainbow(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        int i = (int)(progress * 6);
        float f = progress * 6.0f - i;
        float q = 1 - f;

        switch (i % 6)
        {
            case 0:
                r = 1;
                g = f;
                b = 0;
                break;
            case 1:
                r = q;
                g = 1;
                b = 0;
                break;
            case 2:
                r = 0;
                g = 1;
                b = f;
                break;
            case 3:
                r = 0;
                g = q;
                b = 1;
                break;
            case 4:
                r = f;
                g = 0;
                b = 1;
                break;
            case 5:
                r = 1;
                g = 0;
                b = q;
                break;
        }
        return new Color(r, g, b);
    }

    public void ScriptButton()
    {
        //this.transform.parent.parent.gameObject.SetActive(false);
        //TODO: Get data from backend with gameID
        StartCoroutine(GetGameData(gameID));

        //TODO: Update scene
    }
    IEnumerator GetGameData(string ID)
    {
        string url = "http://52.71.182.98/q_game/?id=";
        url += ID;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result==UnityWebRequest.Result.ProtocolError|| webRequest.result==UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
            }
            else
            {
                //保存本地
                string savePath = "Assets/Scripts/TempData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
                //读取
                StreamReader streamReader = new StreamReader(savePath);
                string str = streamReader.ReadToEnd();
                streamReader.Close();

                
                gameData = JsonMapper.ToObject<GameData>(str);
                for(int i=0;i< gameData.result.info.Map.Count;i++)
                {
                    //string addr = "Maps/InDoor/" + gameData.result.info.Map[i].background;
                    string addr =gameData.result.info.Map[i].background;
                    StartCoroutine(GetMapTexture(addr,i));//background???这名字需要修改


                    for (int j = 0; j < gameData.result.info.Map[i].Map_Object.Count; j++)
                    {
                        string objAddr = gameData.result.info.Map[i].Map_Object[j].image_link;
                        StartCoroutine(GetObjectTexture(objAddr, i, j));
                    }
                }

                StartCoroutine(WaitForDownloadCompelete());
            }
        }
        
    }
    /// <summary>
    /// 获取地图贴图
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator GetMapTexture(string addr, int i)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink=imageLink.Replace(" ", "%20");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error+"imageLink: "+imageLink);
        }
        else
        {
            gameData.result.info.Map[i].mapTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }

    /// <summary>
    /// 获取object的贴图
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator GetObjectTexture(string addr, int i,int j)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            gameData.result.info.Map[i].Map_Object[j].objTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    /// <summary>
    /// 等待所有图片下载完成
    /// </summary>
    /// <param name="gd"></param>
    /// <returns></returns>
    IEnumerator WaitForDownloadCompelete()
    {
        while(true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.result.info.Map)
            {
                if (gm.mapTexture == null) isCompelete=false;
                foreach (PlacedObject po in gm.Map_Object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }
            }
            if (isCompelete == true) break;
            yield return null;
        }
        //如果发现都下载完成了，将游戏数据传给gamemanager并执行场景更新
        GameObject.Find("GameManager").GetComponent<GameManager>().SetGameData(gameData);
        GameObject.Find("GameManager").GetComponent<GameManager>().UpdateScene();
        this.transform.parent.parent.gameObject.SetActive(false);
    }
}
