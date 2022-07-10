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
    public GameObject currentMap;

    public TMP_Text PlayerInfoText;
    public TMP_Text PlayerNameText;
    public TMP_Text PlayerIdentityText;
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
        PlayerIdentityText.text = "You are a " + GetPlayerIdentity(character.identity);
        PlayerInfoText.text = character.background;
    }
    public string GetPlayerIdentity(int identity)
    {
        switch (identity)
        {
            case 0:
                return "Detective";
            case 1:
                return "Murderer";
            case 2:
                return "Suspect";
            default:
                Debug.LogError("wrong identity info!");
                break;
        }
        return "";
    }
    public void UpdateScene()
    {
        //this is game test scene, we don't need to change the character texture, just update the player info panel
        {
            if (localPlayer != null)
                localPlayer.transform.SetSiblingIndex(localPlayer.transform.parent.childCount - 1);

            //Set PlayerInfoPanel
            infoCharacterIndex = 0;
            SetPlayerInfoPanel(gameData.result.info.character[infoCharacterIndex]);
        }

        //Set Map
        {
            mapIndex = 0;
            UpdateMap(mapIndex);
            mapIndex++;
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

            float w = gameData.result.info.Map[index].mapTexture.width;
            float h = gameData.result.info.Map[index].mapTexture.height;

            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            map.transform.SetSiblingIndex(0);
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[index].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));

            currentMap = map;
        }

        //update map object
        {
            for (int i = 0; i < gameData.result.info.Map[index].Map_Object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(gameCanvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.result.info.Map[index].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[index].Map_Object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[index].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<t_Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<t_Object>().SetInfo(gameData.result.info.Map[index].Map_Object[i].message);
                obj.transform.localPosition = gameData.result.info.Map[index].Map_Object[i].GetPosition();

                obj.transform.SetParent(objects.transform);
            }
        }

        //update collision
        {
            float w = gameData.result.info.Map[index].mapTexture.width / 2;
            float h = gameData.result.info.Map[index].mapTexture.height / 2;
            string[] rows = gameData.result.info.Map[index].collide_map.Split(';');
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
        countTime = gameData.result.info.Map[index].duration;
        SetTimerText(countTime);

        //Set Final Text
        EndText.text = gameData.result.info.Map[index].end;
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
        //if this is the last map, show 
        if (mapIndex == gameData.result.info.Map.Count)
        {
            Debug.Log("This is the last map");
        }
        else
        {
            EndText.transform.parent.parent.gameObject.SetActive(true);
            UpdateMap(mapIndex);
            mapIndex++;

            GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in playerObj)
            {
                player.transform.localPosition = Vector2.zero;
            }
        }
    }

    public void NextButton()
    {        
        List<GameCharacter> characters =new List<GameCharacter>(gameData.result.info.character);
        infoCharacterIndex++;
        if(infoCharacterIndex>=characters.Count)
            infoCharacterIndex = 0;

        SetPlayerInfoPanel(characters[infoCharacterIndex]); 
    }
    public void PrevButton()
    {
        List<GameCharacter> characters = new List<GameCharacter>(gameData.result.info.character);
        infoCharacterIndex--;
        if (infoCharacterIndex <0)
            infoCharacterIndex = characters.Count-1;

        SetPlayerInfoPanel(characters[infoCharacterIndex]);
    }
    #endregion

    #region Download Data

    /// <summary>
    /// temporary method,just for testing new data format
    /// </summary>
    void TestGameData()
    {
        //test new data format
        string testPath = "Assets/JsonData/TestJson.json";
        gameData = JsonMapper.ToObject<GameData>(File.ReadAllText(testPath));

        int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
        if (playerCount >= gameData.result.info.playes_num)
        {
            for (int i = 0; i < gameData.result.info.Map.Count; i++)
            {
                string addr = gameData.result.info.Map[i].background;
                StartCoroutine(GetMapTexture(addr, i));

                for (int j = 0; j < gameData.result.info.Map[i].Map_Object.Count; j++)
                {
                    string objAddr = gameData.result.info.Map[i].Map_Object[j].image_link;
                    StartCoroutine(GetObjectTexture(objAddr, i, j));
                }
            }
            StartCoroutine(WaitForDownloadCompelete());
            scriptScroll.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("not enough player for this script!\n " + gameData.result.info.character.Count);
            scriptScroll.gameObject.SetActive(true);
        }
    }

    public void DownLoadGameData(string ID)
    {
        gameDataID = ID;
        //StartCoroutine(GetGameData(ID));
        TestGameData();
    }

    IEnumerator GetGameData(string ID)
    {
        string url = "https://api.dreamin.land/q_game/?id=";
        url += ID;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
            }
            else
            {
#if UNITY_EDITOR
                //Save a gamedata backup for debug
                string savePath = "Assets/Scripts/GameTest/TempData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif

                if(downloadObjects.Count!=0)
                {
                    foreach (var it in downloadObjects)
                    {
                        Destroy(it);
                    }
                    downloadObjects.Clear();
                }

                gameData = JsonMapper.ToObject<GameData>(webRequest.downloadHandler.text);
                for (int i = 0; i < gameData.result.info.Map.Count; i++)
                {
                    string addr = gameData.result.info.Map[i].background;
                    StartCoroutine(GetMapTexture(addr, i));

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
            gameData.result.info.Map[i].mapTexture = t;
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
            gameData.result.info.Map[i].Map_Object[j].objTexture = t;
        }
    }
    IEnumerator WaitForDownloadCompelete()
    {
        while (true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.result.info.Map)
            {
                if (gm.mapTexture == null) isCompelete = false;
                foreach (PlacedObject po in gm.Map_Object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }

            }
            if (isCompelete == true) break;
            yield return null;
        }
        Debug.Log("Download Compelete!");
        isDownloadCompelete = true;
        UpdateScene();
    }
    #endregion



}