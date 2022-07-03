using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptItem : MonoBehaviour
{
    internal string gameID;
    internal int numOfPlayer;

    private GameData gameData;
    void ScrollCellIndex(int idx)
    {
        gameObject.name = name;
    }

    public void ScriptButton()
    {
        //Get data from backend with gameID
        GameObject.Find("GameManager").GetComponent<GameManager>().DownLoadGameData(gameID);
    }
}
