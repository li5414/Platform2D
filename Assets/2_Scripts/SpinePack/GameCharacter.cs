using UnityEngine;
using System.Collections.Generic;

public class GameCharacter : MonoBehaviour
{
    public static List<GameCharacter> All = new List<GameCharacter>();

    protected virtual void OnEnable()
    {
        Register();
    }

    protected virtual void OnDisable()
    {
        Unregister();
    }
    
    void Register()
    {
		if( All.Contains( this ) == false ) All.Add(this);
    }

    void Unregister()
    {
		if( All.Contains( this )) All.Remove(this);
    }

    public virtual void IgnoreCollision(Collider2D collider, bool ignore)
    {
        Physics2D.IgnoreCollision(GetComponentInChildren<Collider2D>(), collider, ignore);
    }
}
