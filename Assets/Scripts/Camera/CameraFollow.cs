using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public bool freazeX, freazeY;

    public float smoothTime = 0.3F;
    private float xVelocity, yVelocity= 0.0F;

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
    //private void LateUpdate()
    //{
    //    if (target != null)
    //    {
    //        oldPosition = transform.position;
    //        if (!freazeX)
    //        {
    //            oldPosition.x = Mathf.SmoothDamp(transform.position.x, target.transform.position.x + offset.x, ref xVelocity, smoothTime);
    //        }

    //        if (!freazeY)
    //        {
    //            oldPosition.y = Mathf.SmoothDamp(transform.position.y, target.transform.position.y + offset.y, ref yVelocity, smoothTime);
    //        }
    //        transform.position = oldPosition;
    //    }
    //}

    public void SetTarget(GameObject t)
    {
        target = t.transform;
        offset = transform.position - target.transform.position;
    }
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
