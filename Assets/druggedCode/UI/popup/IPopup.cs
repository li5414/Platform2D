using System;
using UnityEngine;
using UnityEngine.Events;

namespace druggedcode
{
    public interface IPopup : IOpenAndClose
    {
        UnityAction OnCloseStart{ get; set; }
        UnityAction OnCloseComplete{ get; set; }
    }


    /*
    bool _isOpen;
    public void Open()
    {
        if( _isOpen )
            return;
        
        _isOpen = true;
        
        //do someting
    }
    
    public void Close()
    {
        if( isOpen == false )
            return;
        
        _isOpen = false;


        if( OnCloseStart != null )
        {
            OnCloseStart();
            OnCloseStart = null;
        }
        
        //do someting;

        //somemotion
        CloseComplete();
    }
    
    void CloseComplete()
    {
        if( OnCloseComplete != null )
        {
            OnCloseComplete();
            OnCloseComplete = null;
        }
    }


    public bool isOpen
    { 
        get{ return _isOpen; }
    }
    */
}
