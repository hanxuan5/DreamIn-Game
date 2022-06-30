using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviourPun
{
    // Speed of movement
    public float moveSpeed = 3;

    /// <summary>
    /// Update the movement of player
    /// </summary>
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
