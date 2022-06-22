using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class t_ScriptItem : MonoBehaviour
{
    public string gameID;
    public int numOfPlayer;

    private GameData gameData;
    void ScrollCellIndex(int idx)
    {
        gameObject.name = name;
    }

    public void ScriptButton()
    {
        //Get data from backend with gameID
        GameObject.Find("TestManager").GetComponent<TestManager>().DownLoadGameData(gameID);
    }
}
