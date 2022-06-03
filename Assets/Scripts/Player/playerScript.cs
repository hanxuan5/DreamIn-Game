using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class playerScript : MonoBehaviourPun
{
    public TMP_Text nameText;
    public GameObject nameTextObj;

    internal string playerName;
    private int playerIndex;//玩家在character数组中的编号
    private int playerIdentity;//玩家的身份
    private GameData gameData;

    private Rigidbody2D body;
    public float runSpeed = 20.0f;

    private Animator animator;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        //生成玩家名字面板并让其跟踪玩家
        GameObject t = GameObject.Instantiate(nameTextObj, transform.position + new Vector3(0, 25, 0), Quaternion.identity);
        GameObject canvas = GameObject.Find("GameCanvas");
        t.transform.SetParent(canvas.transform);
        t.transform.localScale = new Vector3(1, 1, 1);

        t.GetComponent<TextFollow>().SetTarget(gameObject);
        nameText = t.GetComponent<TMP_Text>();
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v);
        body.velocity =dir*runSpeed;

        if (v > 0)
        {
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        else if (v < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", true);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        if (h > 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", true);
        }
        else if (h < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", true);
            animator.SetBool("right", false);
        }

        if (h == 0 && v == 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
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

    ///// <summary>
    ///// 同步方法，同步投票数信息
    ///// </summary>
    ///// <param name="stream"></param>
    ///// <param name="info"></param>
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting == true)
    //    {
    //        stream.SendNext(voteNum);
    //    }
    //    else
    //    {
    //        voteNum = (int)stream.ReceiveNext();
    //    }
    //}
    //public void AddVoteNum()
    //{
    //    voteNum++;
    //}
}
