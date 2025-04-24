using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private Rigidbody rg;
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float rotationSpeed = 180;

    // 定义网络变量 //!告诉服务器端 该变量的变化情况 并将变化同步到其他客户端
    private NetworkVariable<Vector3> networkPos = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private NetworkVariable<Quaternion> networkRot = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    void Start()
    {
        rg = GetComponent<Rigidbody>();

        // IsClient 属性用于判断当前实例是否运行在客户端上
        // IsOwner 属性用于判断当前客户端是否是该对象自己的客户端
        if (this.IsClient && this.IsOwner)
        {
            transform.position = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
        }
    }

    void Update()
    {
        if (this.IsClient && this.IsOwner)//+控制自己的移动
        {
            // 获取垂直和水平输入
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            // 获取目标位置和旋转
            Vector3 pos = GetTargetPos(v);
            Quaternion rot = GetTargetRot(h);


            UpdatePosAndRotServerRpc(pos, rot);

            // 移动到目标位置
            Move(pos);
            // 旋转到目标旋转
            Rotate(rot);
        }
        else
        {//+控制其他玩家的移动同步
            // 从网络变量中获取位置和旋转 
            // +!从服务器端获取该变量的值 并将其同步到其他客户端
            Move(networkPos.Value);
            Rotate(networkRot.Value);
        }
    }

    [ServerRpc]   //!得加这个  
    public void UpdatePosAndRotServerRpc(Vector3 pos, Quaternion rot)//!告诉服务器端 该方法的调用情况 并将调用同步到其他客户端
    {
        //设置网络变量的值 //!告诉服务器端 该变量的变化情况 并将变化同步到其他客户端
        networkPos.Value = pos;
        networkRot.Value = rot;
    }

    private Vector3 GetTargetPos(float v)
    {
        // 计算位置增量
        Vector3 delta = this.transform.forward * v * moveSpeed * Time.deltaTime;
        // 计算目标位置
        Vector3 pos = rg.position + delta;
        return pos;
    }

    private void Move(Vector3 pos)
    {
        // 移动刚体到目标位置
        rg.MovePosition(pos);
    }

    private Quaternion GetTargetRot(float h)
    {
        // 计算旋转增量
        Quaternion deltaRotation = Quaternion.Euler(0f, h * rotationSpeed * Time.deltaTime, 0f);
        // 计算目标旋转
        return rg.rotation * deltaRotation;
    }

    private void Rotate(Quaternion rot)
    {
        // 旋转刚体到目标旋转
        rg.MoveRotation(rot);
    }
}
