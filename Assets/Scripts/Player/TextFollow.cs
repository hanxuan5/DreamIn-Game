using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TextFollow : MonoBehaviour
{
    public Transform target;

    public PhotonView photonView;

    // Axis that need to be locked (can take effect in real time)
    public bool freazeX, freazeY;

    public float smoothTime = 0.05F;
    private float xVelocity, yVelocity = 0.0F;

    private Vector3 offset;

    private Vector3 oldPosition;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }
    void Update()
    {
        if (target != null)
        {
            oldPosition = transform.position;
            if (!freazeX)
            {
                oldPosition.x = Mathf.SmoothDamp(transform.position.x, target.transform.position.x + offset.x, ref xVelocity, smoothTime);
            }

            if (!freazeY)
            {
                oldPosition.y = Mathf.SmoothDamp(transform.position.y, target.transform.position.y + offset.y, ref yVelocity, smoothTime);
            }
            transform.position = oldPosition;
        }
    }
    void FixedUpdate()
    {
        if (target != null)
        {
            oldPosition = transform.position;
            if (!freazeX)
            {
                oldPosition.x = Mathf.SmoothDamp(transform.position.x, target.transform.position.x + offset.x, ref xVelocity, smoothTime);
            }

            if (!freazeY)
            {
                oldPosition.y = Mathf.SmoothDamp(transform.position.y, target.transform.position.y + offset.y, ref yVelocity, smoothTime);
            }
            transform.position = oldPosition;
        }
    }

    public void SetTarget(GameObject go)
    {
        target = go.transform;
        offset = transform.position - target.transform.position;
        //photonView.RPC("RPCSetTarget", RpcTarget.All, go);
    }
    [PunRPC]
    public void RPCSetTarget(GameObject go)
    {
        target = go.transform;
        offset = transform.position - target.transform.position;
    }

    /// <summary>
    /// Reset camera location
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
