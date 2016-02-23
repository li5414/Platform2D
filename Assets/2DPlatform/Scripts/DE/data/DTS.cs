using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using druggedcode.engine;
using Newtonsoft.Json;

public class DTSData<T> where T: DTS
{
	public MetaData meta;
	public Dictionary<string, T> dictionary;
	public List<T> list;

	public T Get(string id )
	{
		if( dictionary != null )
		{
			return GetFromDictionary( id );
		}
		else
		{
			return GetFromList( id );
		}
	}

	T GetFromDictionary( string id )
	{
		if (false == dictionary.ContainsKey(id))
		{
			return null;
		}
		return dictionary[id];
	}

	T GetFromList( string id )
	{
		int len = list.Count;
		DTS dts;
		for( int i = 0; i < len; ++i )
		{
			dts = list[i];
			if( dts.id == id ) return dts;
		}

		return dts;
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject( this, Formatting.Indented );
	}
}

public struct MetaData
{
	public string name;
	public string date;
}

public class DTS
{
	public string id;
	public string name;
	public string assetName;
}

public class DTSCharacter: DTS
{
	
    public int hp;
    public int str;
    public int agi;
    public int def;
    public int luck;
    public List<string> skills;
}

public class DTSLocation: DTS
{
	public DTSLocation( string id, string name, string assetName )
	{
		this.id = id;
		this.name = name;
		this.assetName = assetName;
	}

    public override string ToString()
    {
        return string.Format("[DTSLocation] id:{0}, name:{1}, assetName:{2}",id,name,assetName );
    }
}
