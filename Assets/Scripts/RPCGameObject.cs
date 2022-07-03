using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// All game objects that need to be generated on GameCanvas must add this script to prevent scale problems
/// </summary>
public class RPCGameObject : MonoBehaviour
{
    PhotonView photonView;
    public void Awake()
    {
        photonView = GetComponent<PhotonView>();
        AddToCanvas();
    }

    public void AddToCanvas()
    {
        photonView.RPC("RPCAddToCanvas", RpcTarget.All);
    }
    [PunRPC]
    void RPCAddToCanvas()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        transform.SetParent(canvas.transform);
        transform.localScale = new Vector3(1, 1, 1);
    }
}
