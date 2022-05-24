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

                GameJsonData gj = JsonMapper.ToObject<GameJsonData>(str);
                gj.result.info.Map[0].Map_Object[0].SwitchToVectorPosition();//例子
            }
        }
    }
}
