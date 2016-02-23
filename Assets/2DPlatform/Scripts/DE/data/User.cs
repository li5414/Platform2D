using UnityEngine;
using UnityEngine.Events;
using druggedcode;
using druggedcode.engine;

public class User : Singleton<User>
{
    int mGold;
    
    string[] mOwnCharacters;
    string mSelectedCharcterID;
    DEPlayer mCurrentCharacter;

    public string locationID{ get; private set; }
    public string checkPointID{ get; private set; }

	public event UnityAction<int> OnGold;

    public void SetData(string userInfo)
    {
        gold = 0;

        locationID = "1000";//town
        checkPointID = "";

        mOwnCharacters = new string[] { "100", "101" };
        mSelectedCharcterID = mOwnCharacters[0];

		print(string.Format("[User] Gold: {0}, LocationID: {1}, selectedCharacterID: {2}",gold,locationID, mSelectedCharcterID ));
    }

    public YieldInstruction Move( string locID, string chID )
    {
		locationID = locID;
        checkPointID = chID;

        return new WaitForSeconds(0f);
    }

    public DEPlayer GetCharacter()
    {
        if (mCurrentCharacter == null)
        {
            mCurrentCharacter = ResourceManager.Instance.CreatePlayer(mSelectedCharcterID);
            mCurrentCharacter.DeActive();
            
            GameObject.DontDestroyOnLoad( mCurrentCharacter );
        }

        return mCurrentCharacter;
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
