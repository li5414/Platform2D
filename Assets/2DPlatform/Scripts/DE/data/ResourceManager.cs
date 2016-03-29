using UnityEngine;
using druggedcode;
using druggedcode.engine;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

public class ResourceManager : Singleton<ResourceManager>
{
	DTSData<DTSCharacter> mDTSDataCharacter;
	DTSData<DTSLocation> mDTSDataLocation;

    override protected void Awake()
    {
        base.Awake();
    }

	//data
    public IEnumerator Load()
    {
        mDTSDataCharacter = LoadDTSData<DTSCharacter>( "data/character",true);
        mDTSDataLocation = LoadDTSData<DTSLocation>( "data/location",true);

		yield return new WaitForSeconds(0.1f);
    }

    DTSData<T> LoadDTSData<T>( string path, bool usePrint = false ) where T: DTS
    {
        TextAsset asset = Resources.Load<TextAsset>(path);

        if (asset == null)
        {
            Log("LoadDTSData '" + path + "' not exist.");
            return null;
        }

		DTSData<T> data = JsonConvert.DeserializeObject<DTSData<T>>(asset.text);

        if( usePrint ) Log( "DTSData '" + path + "' loaded.\n" + data.ToString() );
        return data;
    }

	public DTSLocation GetDTSLocation( string id )
	{
		return mDTSDataLocation.Get( id );
	}

	public DTSLocation GetDTSLocationByAssetName( string assetName )
	{
		foreach( DTSLocation loc in mDTSDataLocation.list )
		{
			if( loc.assetName == assetName ) return loc;
		}

		return null;
	}

	public DTSCharacter GetDTSCharacter( string id )
	{
		return mDTSDataCharacter.Get( id );
	}

    public Object Load( string path )
    {
        return Resources.Load( path );
    }

    public void Log( object msg )
    {
        Debug.Log( "[ResourceManager] " + msg );
    }
}
