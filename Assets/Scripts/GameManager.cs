using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
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

        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            Debug.Log("A new Client connected, id: " + id);//相当于Player的OwerClientId
            //CreatePlayer();//!在客户端连接时创建玩家 该函数会在所有客户端上调用 从而实现同步

        };//*客户端连接回调
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log("A Client disconnected, id: " + id);
        };//*客户端断开连接回调
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            Debug.Log("Server started");
            CreateCoins();//!应该是服务器端创建金币
        };//*服务器端启动回调
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreatePlayer()
    {
        Vector3 spawnPos = GetValidSpawnPosition(0.5f);
        GameObject gameObject = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        gameObject.GetComponent<NetworkObject>().Spawn();//!在网络上实例化玩家 该函数会在所有客户端上调用 从而实现同步
        gameObject.GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);//!将玩家的拥有权转移到当前客户端上
    }

    private void CreateCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition(0.02f);
            GameObject ob = Instantiate(coinPrefab, spawnPos, Quaternion.identity);//只在本地实例化金币
            ob.GetComponent<NetworkObject>().Spawn();//!在网络上实例化金币 该函数会在所有客户端上调用 从而实现同步
        }
    }

    public void OnStartServerBtnClick()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Server started successfully");
        }
        else
        {
            Debug.Log("Server failed to start");
        }

    }

    public void OnStartClientBtnClick()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started");
        }
        else
        {
            Debug.Log("Client failed to start");
        }
    }

    public void OnStartHostBtnClick()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started");
        }
        else
        {
            Debug.Log("Host failed to start");
        }
    }

    public void OnShutDownNetworkBtnClick()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("Network shut down");
    }

    //生成位置验证方法
    private Vector3 GetValidSpawnPosition(float yPos)
    {
        Vector3 spawnPos;
        bool isValid = false;
        int attempts = 0;

        do
        {
            // 随机生成位置（根据你的游戏区域调整范围）
            spawnPos = new Vector3(
                Random.Range(-8f, 8f),
                yPos,  // Y轴高度
                Random.Range(-8f, 8f)
            );

            // 使用物理检测避免生成在墙体中
            isValid = !Physics.CheckSphere(spawnPos, 0.7f); // 0.7是物体半径
            attempts++;

        } while (!isValid && attempts < 100);

        return spawnPos;
    }
}
