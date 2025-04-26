using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject coinPrefab;
    [SerializeField]
    private int coinCount = 4;

    // Start is called before the first frame update
    void Start()
    {
        //CreatePlayer();
        //CreateCoins();

        NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
            Debug.Log("A new Client connected, id: " + id);
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
            Debug.Log("A Client disconnected, id: " + id); 
        };
        NetworkManager.Singleton.OnServerStarted += () => {
             Debug.Log("Server started"); 
        };

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePlayer()
    {
        Instantiate(playerPrefab, new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5)), Quaternion.identity);
    }

    private void CreateCoins()
    {
        for(int i = 0; i < coinCount; i++)
        {
            Instantiate(coinPrefab, new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10)), Quaternion.identity);
        }
    }

    public void OnStartServerBtnClick(){
        if(NetworkManager.Singleton.StartServer()){
            Debug.Log("Server started successfully");
        }else{
            Debug.Log("Server failed to start");
        }

    }

    public void OnStartClientBtnClick(){
        if(NetworkManager.Singleton.StartClient()){
            Debug.Log("Client started"); 
        }else{
            Debug.Log("Client failed to start");
        }
    }

    public void OnStartHostBtnClick(){
        if(NetworkManager.Singleton.StartHost()){
            Debug.Log("Host started");
        }else{
            Debug.Log("Host failed to start"); 
        }
    }

    public void OnShutDownNetworkBtnClick(){
        NetworkManager.Singleton.Shutdown();
        Debug.Log("Network shut down");
    }
}
