using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RPCGameObject : MonoBehaviour
{
    //所有需要生成在GameCanvas上的游戏物体必须挂载此类以防止比例问题
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
