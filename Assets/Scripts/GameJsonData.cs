using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameData
{
    public string status;
    public ReceiveResult result;
}
public class ReceiveResult
{
    public int id;
    public GameInfo info;
}

public class GameInfo
{
    public string name;
    public string end;
    public int length;
    public List<GameCharacter> character; //最多11个
    public List<GameMap> Map;
}

public class GameCharacter
{
    public string name;
    public int identity;
    public string background;

    public Texture2D characterTexture;//人物贴图
}

public class GameMap
{
    public string background;
    public string collide_map;
    public List<PlacedObject> Map_Object; //平均在150个，但大部分object的message为空字符串

    public Texture2D mapTexture;//地图贴图
}

public class PlacedObject
{
    public string image_link;
    public string message;
    public string position;

    public Texture2D objTexture;//物体贴图

    public Vector3 GetPosition()
    {
        if (position == null)
        {
            Debug.LogError("该物体没有位置信息!");
            return new Vector3(0, 0, 0);
        }
        string str = position.Substring(0, position.Length);

        string[] pos = str.Split(new char[2] { 'f', ',' });
        return new Vector3(Convert.ToSingle(pos[0]), Convert.ToSingle(pos[2]), Convert.ToSingle(pos[4]));
    }
}
