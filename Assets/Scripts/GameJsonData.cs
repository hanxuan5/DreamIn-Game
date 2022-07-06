using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// GameData's Format
/// </summary>
public class GameData
{
    public string status;
    public ReceiveResult result;
    private GameData gameData;
}
public class ReceiveResult
{
    public int id;
    public GameInfo info;
}

public class GameInfo
{
    public string name;
    //public string end;
    //public int length;
    public int playes_num;
    public List<GameCharacter> character; //Maximum value:11
    public List<GameMap> Map;
}

public class GameCharacter
{
    public string name;
    public int identity;
    public string background;

    public Texture2D characterTexture;
}

public class GameMap
{
    public string title;
    public int duration;
    public string end;
    public string background;

    public string collide_map;
    public List<PlacedObject> Map_Object; //The average number is 150, but most object messages are empty strings

    public Texture2D mapTexture;
}

public class PlacedObject
{
    public string image_link;
    public string message;
    public string position;

    public Texture2D objTexture;

    public Vector3 GetPosition()
    {
        if (position == null)
        {
            Debug.LogError("The object has no position information!");
            return new Vector3(0, 0, 0);
        }
        string str = position.Substring(1, position.Length-2);

        string[] pos = str.Split(',');
        Vector3 ret =  new Vector3(Convert.ToSingle(pos[0]), Convert.ToSingle(pos[1]), Convert.ToSingle(pos[2]));
        return ret;
    }
}
