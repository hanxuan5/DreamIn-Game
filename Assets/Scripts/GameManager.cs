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

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject readyButton;
    public GameObject watchButton;
    public GameObject startButton;
    public GameObject finishButton;
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject objectPrefab;
    public GameObject mapPrefab;
    public GameObject colliderPrefab;
    public GameObject votePanel;
    public GameObject objectInfoPanel;
    public GameObject timer;
    public GameObject initialScene;

    public TMP_Text PlayerInfoText;
    public TMP_Text PlayerNameText;
    public TMP_Text PlayerIdentityText;
    public TMP_Text FinalText;
    public GameObject TimerText;

    private int countTime=0;
    private PhotonView GM_PhotonView;
    public GameData gameData;
    private string gameDataID;
    private GameObject localPlayer;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 32;

    public void Start()
    {
        GM_PhotonView = GetComponent<PhotonView>();

        //check whether to join the game midway
        Invoke("TestIfGameStart", 1);
    }

    /// <summary>
    /// check whether to join the game midway
    /// </summary>
    void TestIfGameStart()
    {
        GameObject testflag = GameObject.FindGameObjectWithTag("GameStartFlag");
        if (testflag != null)//if so, only watch the game after downloading game data
        {
            readyButton.SetActive(false);
            watchButton.SetActive(true);
            StartCoroutine(GetGameData(testflag.GetComponent<DataID>().dataId));
        }
        else
        {
            readyButton.SetActive(true);
            watchButton.SetActive(true);
        }
    }

    [PunRPC]
    void RPCSetPlayerInfoPanel()
    {
        if (localPlayer != null)
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

        countTime = gameData.result.info.length;

        //update map
        {
            GameObject map = Instantiate(mapPrefab, new Vector2(0, 0), Quaternion.identity, canvas.transform);

            map.transform.SetParent(canvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            float w = gameData.result.info.Map[0].mapTexture.width;
            float h = gameData.result.info.Map[0].mapTexture.height;
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            map.transform.SetSiblingIndex(0);

            map.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
        }

        //update object
        {
            for (int i = 0; i < gameData.result.info.Map[0].Map_Object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(canvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.result.info.Map[0].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[0].Map_Object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<Object>().objectInfoPanel = objectInfoPanel;
                obj.GetComponent<Object>().SetInfoText(gameData.result.info.Map[0].Map_Object[i].message);
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
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h - i * ColliderSize - ColliderSize / 2, 0);
                }
            }
        }

        //set Player
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject it in players)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);

            if (localPlayer != null)
                localPlayer.transform.SetSiblingIndex(localPlayer.transform.parent.childCount - 1);

            GameObject[] watchers = GameObject.FindGameObjectsWithTag("Watcher");
            foreach (GameObject it in watchers)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);
        }

        //Set Final Text
        FinalText.text = gameData.result.info.end;
    }

    #region Button
    public void WatchButton()
    {
        string playerName = "Player " + (int)Random.Range(1, 7);
        localPlayer = PhotonNetwork.Instantiate(playerName, canvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        localPlayer.GetComponent<PlayerScript>().SetPlayerTag("Watcher");
        localPlayer.GetComponent<PlayerScript>().SetPlayerName("Watcher");
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        readyButton.SetActive(false);
        watchButton.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            scriptScroll.SetActive(true);
        }
    }
    public void ReadyButton()
    {
        string playerName = "Player " + (int)Random.Range(1, 7);
        localPlayer = PhotonNetwork.Instantiate(playerName, canvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(0, 0);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);

        readyButton.SetActive(false);
        watchButton.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            scriptScroll.SetActive(true);
        }
    }

    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("game data has not been download!");
            return;
        }
        if(isDownloadCompelete==false)
        {
            Debug.Log("game data downloading");
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            finishButton.SetActive(true);
        }
        startButton.SetActive(false);
        timer.SetActive(true);
        
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

        GM_PhotonView.RPC("RPCSetPlayerInfoPanel", RpcTarget.All);

        //start count time
        StartCountTime(countTime);

        //instantiate GameStartFlag
        GameObject flag = PhotonNetwork.Instantiate("GameStartFlag", canvas.transform.position, Quaternion.identity, 0);
        flag.GetComponent<DataID>().SetGameDataId(gameDataID);
    }

    public void EndButton()
    {
        EndCountTime();
    }

#endregion

#region Download Data
    string GetGameDataLink(string ID)
    {
        return "https://api.dreamin.land/q_game/?id="+ID;
    }
    string GetImageLink(string addr)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");
        imageLink += ".png";
        return imageLink;
    }
    public void DownLoadGameData(string ID)
    {
        gameDataID = ID;
        GM_PhotonView.RPC("RPCDownloadGameData",RpcTarget.All,ID);
    }

    [PunRPC]
    void RPCDownloadGameData(string ID)
    {
        StartCoroutine(GetGameData(ID));
    }
    IEnumerator GetGameData(string ID)
    {
        string url = GetGameDataLink(ID);

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
                string savePath = "Assets/JsonData/GameData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif

                gameData = JsonMapper.ToObject<GameData>(webRequest.downloadHandler.text);

                int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
                if (playerCount >= gameData.result.info.character.Count)
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
                    Debug.Log("not enough player for this script!\n "+ gameData.result.info.character.Count);
                    scriptScroll.gameObject.SetActive(true);
                }
            }
        }
    }

    IEnumerator GetMapTexture(string addr, int i)
    {
        string imageLink = GetImageLink(addr);

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
        string imageLink = GetImageLink(addr);

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

#region Count Time
    private IEnumerator IECountTime;
    void StartCountTime(int t)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            countTime = t * 60;
            IECountTime = CountTime();
            StartCoroutine(IECountTime);
        }
        GM_PhotonView.RPC("RPCShowTimerText", RpcTarget.All);
    }

    void EndCountTime()
    {
        StopCoroutine(IECountTime);
        countTime = 0;
        GM_PhotonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
        GM_PhotonView.RPC("RPCShowVotePanel", RpcTarget.All);
    }

    IEnumerator CountTime()
    {
        while(true)
        {
            countTime -= 1;
            GM_PhotonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
            if(countTime==0)
            {
                GM_PhotonView.RPC("RPCShowVotePanel", RpcTarget.All);
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }
    [PunRPC]
    void RPCShowTimerText()
    {
        timer.gameObject.SetActive(true);
    }
    [PunRPC]
    void RPCSetTimerText(int t)
    {
        string s = "";
        if(t>=3600)
        {
            int hour = t / 3600;
            t = t % 3600;
            int min = t / 60;
            int sec = t % 60;
            if(min>=10)
            {
                if(sec>=10)
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
        else if(t>60)
        {
            int min = t / 60;
            int sec = t % 60;

            if (min >= 10)
            {
                if (sec >= 10)
                    s =""+ min + ":" + sec;
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

    [PunRPC]
    void RPCShowVotePanel()
    {
        if (localPlayer==null || localPlayer.tag == "Watcher") return;
        votePanel.SetActive(true);
        votePanel.GetComponent<VotePanel>().CreatePlayerItem(GameObject.FindGameObjectsWithTag("Player"));
    }
#endregion


    

}
