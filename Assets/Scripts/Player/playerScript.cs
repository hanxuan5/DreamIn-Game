using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class playerScript : MonoBehaviourPun
{
    public TMP_Text nameText;
    internal string playerName;

    private int playerIndex;
    private int playerIdentity;
    private GameData gameData;

    Rigidbody2D body;
    public float runSpeed = 20.0f;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v);
        dir *= runSpeed;

        body.velocity = dir;
    }
    /// <summary>
    /// 根据信息配置player
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="index"></param>
    public void SetPlayerData( int index)
    {
        photonView.RPC("RPCSetPlayerData", RpcTarget.All,  index);
    }
    [PunRPC]
    public void RPCSetPlayerData(int index)
    {
        gameData = GameObject.Find("GameManager").GetComponent<GameManager>().gameData;

        playerIndex = index;
        SetPlayerName(gameData.result.info.character[playerIndex].name);

        playerIdentity = gameData.result.info.character[playerIndex].identity;
        //TODO::设置人物贴图
    }
    public void SetPlayerName(string name)
    {
        photonView.RPC("RPCSetPlayerName", RpcTarget.All, name);
    }
    [PunRPC]
    void RPCSetPlayerName(string name)
    {
        playerName = name;
        nameText.text = name;
    }

    public void SetPlayerTag(string tag)
    {
        photonView.RPC("RPCSetPlayerTag", RpcTarget.All, tag);
    }

    [PunRPC]
    void RPCSetPlayerTag(string tag)
    {
        gameObject.tag = tag;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public string GetPlayerIdentity()
    {
        switch (playerIdentity)
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
    public string GetPlayerInfo()
    {
        return gameData.result.info.character[playerIndex].background;
    }
}
