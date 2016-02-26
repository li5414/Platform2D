using UnityEngine;
using UnityEngine.Events;
using druggedcode;
using druggedcode.engine;
using System.Collections.Generic;

public class User : Singleton<User>
{
    public string locationID{ get; private set; }
    public string checkPointID{ get; private set; }

	public event UnityAction<int> OnGold;

	int mGold;

	string[] mCharacterIDList;
	Dictionary<string,DEPlayer> mCharacterList;
	string mSelectedCharcterID;

	DEPlayer mCurrentCharacter;

    public void SetData(string userInfo)
    {
        gold = 0;

        locationID = "1000";
        checkPointID = "";

		mCharacterIDList = new string[] { "100", "101" };
		mSelectedCharcterID = mCharacterIDList[0];

		mCharacterList = new Dictionary<string, DEPlayer>();
		foreach( string characterID in mCharacterIDList )
		{
			DTSCharacter dts = ResourceManager.Instance.GetDTSCharacter( characterID );
			DEPlayer prefab = Resources.Load<DEPlayer>("characters/player/" + dts.assetName );
			DEPlayer character = GameObject.Instantiate<DEPlayer>( prefab );
			character.name = dts.name;
			character.transform.SetParent( transform );
			character.gameObject.SetActive( false );
			mCharacterList.Add( dts.id, character );
		}

		print(string.Format("[User] Gold: {0}, LocationID: {1}, selectedCharacterID: {2}",gold,locationID, mSelectedCharcterID ));
    }

    public void Move( string locID, string chID )
    {
		locationID = locID;
        checkPointID = chID;
    }

    public DEPlayer GetCharacter()
    {
		return mCharacterList[ mSelectedCharcterID ];
    }

	public int gold
	{
		get{ return mGold; }
		set
		{
			mGold = value;
			if( OnGold != null ) OnGold( mGold );
		}
	}
}
