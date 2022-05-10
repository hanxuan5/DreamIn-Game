using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMoveScript : MonoBehaviourPun
{
    // Speed of movement
    public float moveSpeed = 3;

    //public Text nameText;
    // Start is called before the first frame update
    /*
    void Start()
    {
        if (photonView.IsMine)
        {
            nameText.text = PhotonNetwork.NickName;
        } else
        {
            nameText.text = photonView.Owner.NickName;
        }
    }
    */

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 pos = transform.position;
        pos.x += h * moveSpeed * Time.deltaTime;
        pos.y += v * moveSpeed * Time.deltaTime;
        transform.position = pos;
    }
}
