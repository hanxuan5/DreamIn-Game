using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using TMPro;
using System.Text;
/// <summary>
/// TestManager use for GameTest Scene
/// </summary>
public class TestManager: MonoBehaviourPunCallbacks
{
    public GameObject startButton;
    public GameObject scriptScroll;
    public GameObject gameCanvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject playerPrefab;
    public GameObject objectPrefab;
    public GameObject mapPrefab;
    public GameObject colliderPrefab;
    public GameObject objectInfoPanel;
    public GameObject questionPanel;
    public GameObject currentMap;

    public TMP_Text PlayerInfoText;
    public TMP_Text PlayerNameText;
    public TMP_Text EndText;
    public TMP_Text TimerText;

    private int countTime = 0;
    public GameData gameData;
    private string gameDataID;
    private GameObject localPlayer;
    private List<GameObject> downloadObjects;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 32;
    private int infoCharacterIndex = 0;//the character who is showed in playerinfo panel
    private int mapIndex = 0;

    public void Start()
    {
        localPlayer = GameObject.Instantiate(playerPrefab, gameCanvas.transform.position, Quaternion.identity);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        downloadObjects = new List<GameObject>();

        startButton.SetActive(true);
        scriptScroll.SetActive(true);
    }

    void SetPlayerInfoPanel(GameCharacter character)
    {
        PlayerNameText.text = "Your name is " + character.name;
        PlayerInfoText.text = character.background;
    }
    public void InitializedScene()
    {
        //this is game test scene, we don't need to change the character texture, just update the player info panel
        {
            if (localPlayer != null)
                localPlayer.transform.SetSiblingIndex(localPlayer.transform.parent.childCount - 1);

            //Set PlayerInfoPanel
            infoCharacterIndex = 0;
            SetPlayerInfoPanel(gameData.character[infoCharacterIndex]);
        }

        //Set Map
        {
            mapIndex = 0;
            UpdateMap(mapIndex);
        }
    } 
    void UpdateMap(int index)
    {
        //delete previous map and object
        if (currentMap != null)
        {
            Destroy(currentMap);
            foreach (Transform child in colliders.transform)
                GameObject.Destroy(child.gameObject);

            foreach (Transform child in objects.transform)
                GameObject.Destroy(child.gameObject);
        }

        //update map
        {
            GameObject map = Instantiate(mapPrefab, new Vector2(0, 0), Quaternion.identity, gameCanvas.transform);

            map.transform.SetParent(gameCanvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            float w = gameData.map[index].mapTexture.width;
            float h = gameData.map[index].mapTexture.height;

            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            map.transform.SetSiblingIndex(0);
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.map[index].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));

            currentMap = map;
        }

        //update map object
        {
            for (int i = 0; i < gameData.map[index].map_object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(gameCanvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.map[index].map_object[i].objTexture.width;
                float h = gameData.map[index].map_object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.map[index].map_object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<t_Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<t_Object>().SetInfo(gameData.map[index].map_object[i].message);
                obj.transform.localPosition = gameData.map[index].map_object[i].GetPosition();

                obj.transform.SetParent(objects.transform);
            }
        }

        //update collision
        {
            float w = gameData.map[index].mapTexture.width / 2;
            float h = gameData.map[index].mapTexture.height / 2;
            string[] rows = gameData.map[index].collide_map.Split(';');
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length - 1; j++)
                {
                    GameObject obj = Instantiate(colliderPrefab, new Vector2(0, 0), Quaternion.identity, colliders.transform);
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h - i * ColliderSize - ColliderSize / 2, 0);

                    obj.transform.SetParent(colliders.transform);
                }
            }
        }

        //set count time
        countTime = int.Parse(gameData.map[index].duration);
        SetTimerText(countTime);

        //Set Final Text
        EndText.text = gameData.map[index].end;

    }

    void SetTimerText(int t)
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
            TimerText.text = s;
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
            TimerText.text = s;
        }
        else
        {
            TimerText.text = "" + t;
        }
    }

    #region Button
    public void NextLevel()
    {
        mapIndex++;
        //if this is the last map, show 
        if (mapIndex >= gameData.map.Count)
        {
            Debug.Log("This is the last map");
        }
        else
        {
            EndText.transform.parent.parent.gameObject.SetActive(true);
            UpdateMap(mapIndex);
            
            localPlayer.transform.position = Vector3.zero;
        }
    }

    public void NextButton()
    {        
        List<GameCharacter> characters =new List<GameCharacter>(gameData.character);
        infoCharacterIndex++;
        if(infoCharacterIndex>=characters.Count)
            infoCharacterIndex = 0;

        SetPlayerInfoPanel(characters[infoCharacterIndex]); 
    }
    public void PrevButton()
    {
        List<GameCharacter> characters = new List<GameCharacter>(gameData.character);
        infoCharacterIndex--;
        if (infoCharacterIndex <0)
            infoCharacterIndex = characters.Count-1;

        SetPlayerInfoPanel(characters[infoCharacterIndex]);
    }

    public void PassButton()
    {
        StartCoroutine(ChangeStatus(int.Parse(gameDataID),1));
    }
    public void NoPassButton()
    {
        StartCoroutine(ChangeStatus(int.Parse(gameDataID), 0));
    }

    public void QuestionButton()
    {
        questionPanel.gameObject.SetActive(true);
        questionPanel.GetComponent<t_QuestionPanel>().QuestionText.text = gameData.map[mapIndex].question;
        questionPanel.GetComponent<t_QuestionPanel>().AnswerText.text = "-----------------------------------------\n";
        for (int i = 0; i < gameData.map[mapIndex].answers.Count; i++)
        {
            questionPanel.GetComponent<t_QuestionPanel>().AnswerText.text += gameData.map[mapIndex].answers[i] + "\n";
            questionPanel.GetComponent<t_QuestionPanel>().AnswerText.text += "-----------------------------------------\n";
        }

    }

    IEnumerator ChangeStatus(int id, int status)
    {
        string url = "https://api.dreamin.land/change_status/";
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        Encoding encoding = Encoding.UTF8;

        byte[] buffer = encoding.GetBytes("{\"id\":"+id+",\"status\":"+status+"}");//³É¹¦

        webRequest.uploadHandler = new UploadHandlerRaw(buffer);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log("Change Status Success! game id: "+id+" Status:" + status);
        }
    }
    #endregion

    #region Download Data

    public void DownLoadGameData(string ID)
    {
        gameDataID = ID;
        StartCoroutine(GetGameData(ID));
    }

    IEnumerator GetGameData(string ID)
    {
        //new Data Format
        string url = "https://api.dreamin.land/get_game_doc/";
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");

        Encoding encoding = Encoding.UTF8;
        byte[] buffer = encoding.GetBytes("{\"id\":" + ID + "}");
        webRequest.uploadHandler = new UploadHandlerRaw(buffer);
        webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log("get game data succcess!");
#if UNITY_EDITOR
            //Save a gamedata backup for debug
            string savePath = "Assets/JsonData/GameData.json";
            File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif
        }

        //read and store in gameData
        ReceiveData d = JsonMapper.ToObject<ReceiveData>(webRequest.downloadHandler.text);
        gameData = JsonMapper.ToObject<GameData>(d.game_doc);
        for (int i = 0; i < gameData.map.Count; i++)
        {
            string addr = gameData.map[i].background;
            StartCoroutine(GetMapTexture(addr, i));

            for (int j = 0; j < gameData.map[i].map_object.Count; j++)
            {
                string objAddr = gameData.map[i].map_object[j].image_link;
                StartCoroutine(GetObjectTexture(objAddr, i, j));
            }
        }
        StartCoroutine(WaitForDownloadCompelete());
        scriptScroll.gameObject.SetActive(false);
    }

    IEnumerator GetMapTexture(string addr, int i)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");
        imageLink += ".png";
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            Texture2D t= ((DownloadHandlerTexture)www.downloadHandler).texture;
            t.filterMode = FilterMode.Point;
            gameData.map[i].mapTexture = t;
        }
    }

    IEnumerator GetObjectTexture(string addr, int i, int j)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");
        imageLink += ".png";

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            Texture2D t = ((DownloadHandlerTexture)www.downloadHandler).texture;
            t.filterMode = FilterMode.Point;
            gameData.map[i].map_object[j].objTexture = t;
        }
    }
    IEnumerator WaitForDownloadCompelete()
    {
        while (true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.map)
            {
                if (gm.mapTexture == null) isCompelete = false;
                foreach (PlacedObject po in gm.map_object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }

            }
            if (isCompelete == true) break;
            yield return null;
        }
        Debug.Log("Download Compelete!");
        isDownloadCompelete = true;
        InitializedScene();
    }
    #endregion



}