using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using druggedcode.engine;

public struct MetaData
{
    public string name;
    public string date;
}

public class DTSData<T> where T: struct
{
    public Dictionary<string, T> data;
    public MetaData meta;

    public T Get(string key)
    {
        if (false == data.ContainsKey(key))
        {
            return default(T);
        }
        return data[key];
    }

    public override string ToString()
    {
		return JsonUtility.ToJson( this );
        //return JsonConvert.SerializeObject( this, Formatting.Indented );
    }
}

public struct DTSCharacter
{
    public string id;
    public string prefabName;
    public int hp;
    public int str;
    public int agi;
    public int def;
    public int luck;
    public List<string> skills;
}

public struct DTSLocation
{
    public string id;
    public string name;
    public string assetName;

    public override string ToString()
    {
        return string.Format("[DTSLocation] id:{0}, name:{1}, assetName:{2}",id,name,assetName );
    }
}
