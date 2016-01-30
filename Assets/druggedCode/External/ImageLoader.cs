using UnityEngine;
using System.Collections;
using System;

namespace druggedcode
{
    public class ImageLoader: Singleton<ImageLoader>
    {
        //  using UnityEngine;
        //  using System.Collections;
        //  using System.Collections.Generic;
        //  using System;
        //  using Spine;
        
        
        //  client.Opened += SocketOpened;
        
        public Action<Texture2D> OnComplete;
        public Action<string> OnCompleteText;
        string _url;
        
        public ImageLoader()
        {
            Debug.Log("create");
        }
        
        public void load(string url)
        {
            _url = url;
            Debug.Log("load start");
            
            StartCoroutine("Download");
        }
        
        public void loadText(string url)
        {
            _url = url;
            StartCoroutine("Download");
        }
        
        void LoadComplete(WWW www)
        {
            Debug.Log("complete");
            //        OnComplete(www.texture);
            OnCompleteText(www.text);
            
        }
        
        IEnumerator Download()
        {
            Debug.Log("start Download: " + _url);
            WWW www = new WWW(_url);
            
            yield return www;
            
            
            LoadComplete(www);
        }
    }
}

