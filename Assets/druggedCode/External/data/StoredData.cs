using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace druggedcode
{
    public class StoredData
    {
        static public void SaveObject(string key, System.Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            
            PlayerPrefs.SetString(key, Convert.ToBase64String(ms.GetBuffer()));
        }
        
        static public System.Object LoadObject(string key)
        {
            string data = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(data))
            {
                return null;
            } else
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(data));
                
                return bf.Deserialize(ms);
            }
        }
        
        static public T LoadObject<T>(string key)
        {
            System.Object obj = LoadObject(key);
            if( obj == null )
                return default(T);
            else
                return (T)obj;
        }
        
        static public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
        
        static public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }

}
