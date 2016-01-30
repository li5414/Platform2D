using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class NetworkManager : MonoBehaviour {
        
        private const string ip = "127.0.0.1";
        
        private const int port = 30000;
        
        private bool _useNat = false;
        
        public GameObject player;
        
        
        // Use this for initialization
        void Start () {
            
        }
        
        // Update is called once per frame
        void Update () {
            
        }
        
        void OnGUI()
        {
            if( Network.peerType == NetworkPeerType.Disconnected )
            {
                if( GUI.Button( new Rect(20,20,200,25),"Start Server"))
                {
                    Network.InitializeServer( 20, port, _useNat );
                }
                
                if( GUI.Button( new Rect(20,50,200,25),"Connect to Server"))
                {
                    Network.Connect(ip,port);
                }
            }else{
                if( Network.peerType == NetworkPeerType.Server)
                {
                    GUI.Label(new Rect(20,20,200,25),"Initialization Server...");
                    GUI.Label(new Rect(20,50,200,25),"Client Count = " + Network.connections.Length.ToString());
                }
                
                if( Network.peerType == NetworkPeerType.Client )
                {
                    GUI.Label( new Rect(20,20,200,25),"Connected to Server");
                }
            }
        }
        
        //  서버로부터 접속이 종료 되었을때
        //  OnDisconnectedFromServer
        //          
        //  접속 실패
        //  OnFailedToConnect
        //          
        //  Network Instantiate 로 네트워크 객체가 생성됐을때
        //  OnNetworkInstantiate
        //          
        //  새로운 플레이어가 접속 했을때
        //  OnPlayerConnected
        //          
        //  기존 플레이어가 접속 종료됐을 때
        //  OnPlayerDisconnected
        //          
        //  NetworkView가 특정 스크립트를 Observed 할 때 스크립트 내에서 Sendrate 간격으로 호출할 콜백 함수
        //  OnSerializeNetworkView
        //
        //게임 서버의 초기화가 완료 되었을 때
        void OnServerInitialized()
        {
            StartCoroutine( CreatePlayer());
        }
        
        //서버에 정상 접속 했을때
        void OnConnectedToServer()
        {
            StartCoroutine( CreatePlayer());
        }
        
        IEnumerator CreatePlayer()
        {
            Vector3 pos = new Vector3( Random.Range(-20.0f,20.0f)
                                      ,0.0f
                                      ,Random.Range(-20.0f,20.0f));
            
            Network.Instantiate( player,pos,Quaternion.identity,0);
            
            yield return null;
        }
        
        void OnPlayerDisconnected( NetworkPlayer netPlayer )
        {
            Network.RemoveRPCs( netPlayer );
            Network.DestroyPlayerObjects( netPlayer );
        }
    }
}
