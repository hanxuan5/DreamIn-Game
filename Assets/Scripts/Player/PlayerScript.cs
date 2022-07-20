using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

/// <summary>
/// Add in Player to controll animations, move, data sync
/// </summary>
public class PlayerScript : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject nameTextObj;

    internal PhotonView photonView;

    internal string playerName;
    private int playerIndex;
    private int playerIdentity;
    private GameData gameData;

    private Rigidbody2D body;
    private float runSpeed = 200.0f;

    private Animator animator;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();

        //Intiate playername Panel and let it follow the player
        GameObject t = GameObject.Instantiate(nameTextObj, transform.position + new Vector3(0, 40, 0), Quaternion.identity);
        GameObject canvas = GameObject.Find("GameCanvas");
        t.transform.SetParent(canvas.transform);
        t.transform.localScale = new Vector3(1, 1, 1);
        t.GetComponent<TextFollow>().SetTarget(gameObject);
        nameText = t.GetComponent<TMP_Text>();
    }
    /// <summary>
    /// player aniamtions control
    /// </summary>
    private void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v);
        body.velocity = dir * runSpeed;

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
    /// Configure the player according to game data
    /// </summary>
    /// <param name="characters"></param>
    /// <param name="index"></param>
    public void SetPlayerData(int index)
    {
        photonView.RPC("RPCSetPlayerData", RpcTarget.All, index);
    }
    [PunRPC]
    public void RPCSetPlayerData(int index)
    {
        gameData = GameObject.Find("GameManager").GetComponent<GameManager>().gameData;

        playerIndex = index;
        SetPlayerName(gameData.game_doc.character[playerIndex].name);

        playerIdentity = gameData.game_doc.character[playerIndex].identity;
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
        return gameData.game_doc.character[playerIndex].background;
    }
}
