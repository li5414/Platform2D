using System;
using UnityEngine;

namespace druggedcode
{
    public interface IOpenAndClose
    {
        void Open();
        void Close();
        
        bool isOpen{ get;}

        string name{ get; set;}
        GameObject gameObject{ get; }
        Transform transform{ get; }
    }


    /*
    bool _isOpen;
    public void Open()
    {
        if (_isOpen)
            return;
        
        gameObject.SetActive(true);
        
        _isOpen = true;
        
        //do someting
    }
    
    public void Close()
    {
        if (isOpen == false)
            return;
        
        _isOpen = false;
        
        //do someting;
        
        gameObject.SetActive(false);
    }
    
    public bool isOpen
    { 
        get{ return _isOpen; }
    }
    */
}

