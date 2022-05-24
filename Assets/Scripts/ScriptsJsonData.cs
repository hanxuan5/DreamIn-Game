using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptsJsonData
{
    public string status;
    public ScriptsResult result;

    public ScriptsResult GetScriptsResult()
    {
        return result;
    }
}

public class ScriptsResult
{
    public int total;
    public List<ScriptInfo> Info;

    public int GetNum()
    {
        return total;
    }

    public ScriptInfo GetScript(int idx)
    {
        return Info[idx];
    }
}

public class ScriptInfo
{
    public int id;
    public string name;
}
