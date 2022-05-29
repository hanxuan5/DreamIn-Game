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
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject objectPrefab;
    public GameObject colliderPrefab;
    public GameObject votePanel;
    public GameObject timer;
    public GameObject initialScene;

    public TMP_Text PlayerInfoText;//玩家信息
    public TMP_Text FinalText;//最后结果的面板
    public GameObject TimerText;//显示时间

    private int countTime=0;//倒计时数据
    private PhotonView GM_PhotonView;
    public GameData gameData;
    private string gameDataID;
    private GameObject localPlayer;
    private bool isDownloadCompelete=false;
    private int ColliderSize = 32;

    public void Start()
    {
        GM_PhotonView = GetComponent<PhotonView>();

        //进入场景1s后检查是否是中途加入房间，如果是,只能观战
        Invoke("TestIfGameStart", 1);
    }

    void TestIfGameStart()
    {
        GameObject testflag = GameObject.FindGameObjectWithTag("GameStartFlag");
        if (testflag != null)
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

#region 按钮点击事件
    public void WatchButton()
    {
        localPlayer = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(100, 30);
        localPlayer.GetComponent<playerScript>().SetPlayerTag("Watcher");
        localPlayer.GetComponent<playerScript>().SetPlayerName("Watcher");
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
        localPlayer = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
        localPlayer.transform.localPosition = new Vector2(100, 30);
        Camera.main.GetComponent<CameraFollow>().SetTarget(localPlayer);//开启相机跟随
        

        readyButton.SetActive(false);
        watchButton.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            scriptScroll.SetActive(true);
        }
    }
    /// <summary>
    /// 开始游戏按钮
    /// 房主才有开始游戏按钮
    /// </summary>
    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("没选择剧本");
            return;
        }
        if(isDownloadCompelete==false)
        {
            Debug.Log("没有下载完成");
            return;
        }
        startButton.SetActive(false);
        timer.SetActive(true);

        //分配人物
        List<GameCharacter> characters =new List<GameCharacter>(gameData.result.info.character);
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<playerObj.Length;i++)
        {
            int index = Random.Range(0, characters.Count);
            characters.RemoveAt(index);
            playerObj[i].GetComponent<playerScript>().SetPlayerData( index);
        }

        GM_PhotonView.RPC("RPCSetPlayerInfoPanel", RpcTarget.All);

        //开始计时
        StartCountTime(countTime);

        //一旦游戏开始，就会生成这个物体表示游戏已经开始，之后加入的玩家只能观战
        GameObject flag = PhotonNetwork.Instantiate("GameStartFlag", canvas.transform.position, Quaternion.identity, 0);
        flag.GetComponent<DataID>().SetGameDataId(gameDataID);
    }

#endregion

    public void UpdateScene()
    {
        //删除初始场景
        Destroy(initialScene);
        foreach (Transform child in colliders.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //设置计时
        countTime = gameData.result.info.length;
        //初始化地图
        {
            GameObject map = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, canvas.transform);

            map.transform.SetParent(canvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            //设置地图的位置
            float w = gameData.result.info.Map[0].mapTexture.width;
            float h = gameData.result.info.Map[0].mapTexture.height;
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            //设置地图在UI层级的最下层
            map.transform.SetSiblingIndex(0);

            //设置sprite
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
        }

        //初始化Object
        {
            //目前map是一个数组，这是暂时方法，只获取map[0]
            for (int i = 0; i < gameData.result.info.Map[0].Map_Object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(canvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.result.info.Map[0].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[0].Map_Object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<Object>().SetInfoText(gameData.result.info.Map[0].Map_Object[i].message);
                obj.transform.localPosition = gameData.result.info.Map[0].Map_Object[i].GetPosition() - canvas.transform.position;
            }
        }

        //初始化碰撞体
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

        //设置人物在UI层级的最上层
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject it in players)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);

            GameObject[] watchers = GameObject.FindGameObjectsWithTag("Watcher");
            foreach(GameObject it in watchers)
                it.transform.SetSiblingIndex(it.transform.parent.childCount - 1);
        }

        //设置结尾
        FinalText.text = gameData.result.info.end;
    }

    #region 游戏数据下载
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
        string url = "http://52.71.182.98/q_game/?id=";
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
                //保存一份副本数据到本地
                string savePath = "Assets/Scripts/TempData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif

                gameData = JsonMapper.ToObject<GameData>(webRequest.downloadHandler.text);

                //检测人物数量是否足够，不够的话不能开启游戏    这里先设置为小于等于
                int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
                if (playerCount >= gameData.result.info.character.Count)
                {
                    //下载所需的贴图数据
                    for (int i = 0; i < gameData.result.info.Map.Count; i++)
                    {
                        string addr = gameData.result.info.Map[i].background;
                        StartCoroutine(GetMapTexture(addr, i));//background???这名字需要修改

                        //地图中的物品贴图
                        for (int j = 0; j < gameData.result.info.Map[i].Map_Object.Count; j++)
                        {
                            string objAddr = gameData.result.info.Map[i].Map_Object[j].image_link;
                            StartCoroutine(GetObjectTexture(objAddr, i, j));
                        }
                        //TODO：人物贴图下载

                    }
                    //检测下载是否完成
                    StartCoroutine(WaitForDownloadCompelete());
                    scriptScroll.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("人数不够，请重新选择剧本!\n 要求人数："+ gameData.result.info.character.Count);
                    scriptScroll.gameObject.SetActive(true);
                }
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
        imageLink = imageLink.Replace(" ", "%20");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
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
    IEnumerator GetObjectTexture(string addr, int i, int j)
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
        while (true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.result.info.Map)
            {
                //检测地图贴图
                if (gm.mapTexture == null) isCompelete = false;
                //检测地图物品贴图
                foreach (PlacedObject po in gm.Map_Object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }
                //TODO：检测人物贴图

            }
            if (isCompelete == true) break;
            yield return null;
        }
        //如果发现都下载完成了
        Debug.Log("数据下载完毕");
        isDownloadCompelete = true;
        UpdateScene();
    }
    #endregion

    #region 计时部分
    void StartCountTime(int t)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            countTime = t;
            StartCoroutine(CountTime());
        }
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

    [PunRPC]
    void RPCSetPlayerInfoPanel()
    {
        if(localPlayer!=null)
        {
            PlayerInfoText.text = localPlayer.GetComponent<playerScript>().GetPlayerInfo();
            PlayerInfoText.transform.parent.parent.parent.parent.gameObject.SetActive(true);
        }
    }

}
