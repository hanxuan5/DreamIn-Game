using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using Photon.Pun;

public class ScrollIndexCallback1 : MonoBehaviour 
{
    public string gameID;
    public int numOfPlayer;

    private GameData gameData;
    void ScrollCellIndex (int idx) 
    {
		gameObject.name = name;
	}

    public void ScriptButton()
    {
        //Get data from backend with gameID
        GameObject.Find("GameManager").GetComponent<GameManager>().DownLoadGameData(gameID);
        //this.transform.parent.parent.gameObject.SetActive(false);
    }


}
