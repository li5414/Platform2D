using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using druggedcode.engine;

//namespace druggedcode.engine
//{
//
//}
public class BattleField : ALocation
{
    List<EnemySpawner> mSpawners;

    override protected void Awake()
    {
        base.Awake();
        GetComponentsInChildren<EnemySpawner>( mSpawners );
    }

	void Test()
	{
		mSpawners.Clear();
	}
}
