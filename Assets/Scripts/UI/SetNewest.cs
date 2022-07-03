using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewest : MonoBehaviour
{
    void Start()
    {
        transform.SetSiblingIndex(transform.parent.childCount - 1);
    }

}
