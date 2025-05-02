using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinManager : NetworkBehaviour
{
    private NetworkVariable<bool> networkIsActive = new NetworkVariable<bool>(true);

    public override void OnNetworkSpawn()
    {
        
        networkIsActive.OnValueChanged += (previousValue, newValue) =>
        {
            this.gameObject.SetActive(newValue);
        };//*当网络变量的值发生变化时调用 该函数会在所有客户端上调用 从而实现同步

        this.gameObject.SetActive(networkIsActive.Value);
    }

    public void SetActive(bool active)
    {
        if (this.IsServer)
        {
            networkIsActive.Value = active;

        }
        else if (this.IsClient)
        {
            SetNetworkActiveServerRpc(active);
        }
    }

    [ServerRpc (RequireOwnership = false)]//*允许非拥有者(创建者)调用该函数
    //!此时Coin的拥有者(创建者)是服务器端
    public void SetNetworkActiveServerRpc(bool active)
    {
        networkIsActive.Value = active;
    }

}
