using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Object = System.Object;
public class DataID : MonoBehaviour, IPunObservable
{
    public PhotonView photonView;
    public string dataId;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting == true)
        {
            stream.SendNext(dataId);
        }
        else
        {
            dataId = (string)stream.ReceiveNext();
        }
    }

    public void SetGameDataId(string ID)
    {
        dataId = ID;
    }
}
