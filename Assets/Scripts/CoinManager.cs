using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinManager : NetworkBehaviour
{
    private NetworkVariable<bool> networkIsActive = new NetworkVariable<bool>(true);

    // 添加网络对象产生时的处理
    public override void OnNetworkSpawn()
    {
        //:服务器->客户端
        networkIsActive.OnValueChanged += (previousValue, newValue) =>
        {
            this.gameObject.SetActive(newValue);
        };//*当网络变量的值发生变化时调用 该函数会在所有客户端上调用 从而实现同步

        this.gameObject.SetActive(networkIsActive.Value);
    }

    public void SetActive(bool active)//给外部的函数
    {
        if (this.IsServer)
        {
            networkIsActive.Value = active;// 服务器可以直接修改网络变量

        }
        else if (this.IsClient) 
        {
            //:客户端->服务器
            SetNetworkActiveServerRpc(active);// 客户端通过RPC请求修改网络变量
        }
    }

    //:服务器->客户端
    [ServerRpc (RequireOwnership = false)]//*允许非拥有者(创建者)调用该函数
    //!此时Coin的拥有者(创建者)是服务器端
    public void SetNetworkActiveServerRpc(bool active)
    {
        networkIsActive.Value = active;
    }

    // // 添加网络对象销毁时的处理
    public override void OnNetworkDespawn()
    {
        // 取消注册网络变量变化回调
        networkIsActive.OnValueChanged -= (previousValue, newValue) =>
        {
            this.gameObject.SetActive(newValue);
        };//*当网络变量的值发生变化时调用 该函数会在所有客户端上调用 从而实现同步
        
        // 可以在这里添加其他清理逻辑，比如：
        // - 取消注册事件
        // - 释放资源
        // - 记录日志等
        Debug.Log($"Coin {gameObject.name} is being despawned");
    }

}
