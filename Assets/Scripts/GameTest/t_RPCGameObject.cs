using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class t_RPCGameObject : MonoBehaviour
{
    //所有需要生成在GameCanvas上的游戏测试物体必须挂载此类以防止比例问题
    public void Awake()
    {
        AddToCanvas();
    }

    public void AddToCanvas()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        transform.SetParent(canvas.transform);
        transform.localScale = new Vector3(1, 1, 1);
    }
}
