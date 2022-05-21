using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using UnityEngine.UI;
using UnityEngine;

public class playerScript : MonoBehaviourPun
{
    public Text nameText;
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

    public void SetPlayerName(string name)
    {
        photonView.RPC("RPCSetPlayerName", RpcTarget.All, name);
    }
    

    [PunRPC]
    void RPCSetPlayerName(string name)
    {
        nameText.text = name;
    }
}
