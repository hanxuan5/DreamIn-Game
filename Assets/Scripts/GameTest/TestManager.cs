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

public class TestManager: MonoBehaviourPunCallbacks
{
    public GameObject startButton;
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject playerPrefab;
    public GameObject objectPrefab;
    public GameObject mapPrefab;
    public GameObject colliderPrefab;
    public GameObject objectInfoPanel;
    public GameObject initialScene;

    public TMP_Text PlayerInfoText;
    public TMP_Text PlayerNameText;
    public TMP_Text PlayerIdentityText;
    public TMP_Text FinalText;

    public GameData gameData;
    private string gameDataID;
    private GameObject localPlayer;
    private List<GameObject> downloadObjects;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 32;

    public void Start()
    {
        localPlayer = GameObject.Instantiate(playerPrefab, canvas.transform.position, Quaternion.identity);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        downloadObjects = new List<GameObject>();

        startButton.SetActive(true);
        scriptScroll.SetActive(true);
    } 

#region Button

    public void StartButton()
    {        
        List<GameCharacter> characters =new List<GameCharacter>(gameData.result.info.character);
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        List<int> selectedIndex=new List<int>();
        for(int i=0;i<playerObj.Length;i++)
        {
            int index = Random.Range(0, characters.Count);
            while(selectedIndex.Contains(index))
                index = Random.Range(0, characters.Count);

            selectedIndex.Add(index);
            playerObj[i].GetComponent<PlayerScript>().SetPlayerData(index);
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

    [PunRPC]
    void RPCSetPlayerInfoPanel()
    {
        if(localPlayer!=null)
        {
            PlayerNameText.text = "Your name is " + localPlayer.GetComponent<PlayerScript>().GetPlayerName();
            PlayerIdentityText.text = "You are a " + localPlayer.GetComponent<PlayerScript>().GetPlayerIdentity();
            PlayerInfoText.text = localPlayer.GetComponent<PlayerScript>().GetPlayerInfo();
            PlayerInfoText.transform.parent.parent.parent.gameObject.SetActive(true);
        }
    }

    public void UpdateScene()
    {
        Destroy(initialScene);
        foreach (Transform child in colliders.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //update map
        {
            GameObject map = Instantiate(mapPrefab, new Vector2(0, 0), Quaternion.identity, canvas.transform);
            downloadObjects.Add(map);

            map.transform.SetParent(canvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            float w = gameData.result.info.Map[0].mapTexture.width;
            float h = gameData.result.info.Map[0].mapTexture.height;
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            map.transform.SetSiblingIndex(0);
            map.GetComponent<Image>().sprite =  Sprite.Create(gameData.result.info.Map[0].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
        }

        //update object
        {
            for (int i = 0; i < gameData.result.info.Map[0].Map_Object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);
                downloadObjects.Add(obj);

                obj.transform.SetParent(canvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.result.info.Map[0].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[0].Map_Object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<t_Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<t_Object>().SetInfoText(gameData.result.info.Map[0].Map_Object[i].message);
                obj.transform.localPosition = gameData.result.info.Map[0].Map_Object[i].GetPosition();
            }
        }

        //update collision
        {
            float w = gameData.result.info.Map[0].mapTexture.width / 2;
            float h = gameData.result.info.Map[0].mapTexture.height / 2;
            string[] rows = gameData.result.info.Map[0].collide_map.Split(';');
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length - 1; j++)
                {
                    GameObject obj = Instantiate(colliderPrefab, new Vector2(0, 0), Quaternion.identity, colliders.transform);
                    downloadObjects.Add(obj);
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h - i * ColliderSize - ColliderSize / 2, 0);
                }
            }
        }

        if (localPlayer != null)
           localPlayer.transform.SetSiblingIndex(localPlayer.transform.parent.childCount - 1);

        //Set Final Text
        FinalText.text = gameData.result.info.end;
    }

}