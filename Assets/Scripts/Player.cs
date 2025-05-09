using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Components;

public class Player : NetworkBehaviour
{
    private Rigidbody rg;
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float rotationSpeed = 180;
    [SerializeField]
    private Text nameText; // 玩家名称文本
    private Camera playerCamera;


    [SerializeField]
    private Text speedText; // 添加速度显示文本
    private Vector3 lastPosition;
    private float currentSpeed;



    // 定义网络变量 //*会通知 所有 客户端
    // private NetworkVariable<Vector3> networkPos = new NetworkVariable<Vector3>();

    // private NetworkVariable<Quaternion> networkRot = new NetworkVariable<Quaternion>();
    private NetworkVariable<int> clientId = new NetworkVariable<int>();
    private NetworkVariable<bool> networkplayerWalkState = new NetworkVariable<bool>();

    
    private Color[] colors = new Color[4]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };
    private bool lastWalkState = false;
    //private Animator animator;
    private Animator animator;


    void Start()
    {
        rg = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        //animator = GetComponent<Animator>();
        animator = GetComponent<Animator>();

        if (!IsOwner)
        {
            Debug.Log(nameText.text + " is not owner");
            playerCamera.gameObject.SetActive(false);
        }


        //* IsClient 属性用于判断当前实例是否运行在客户端上
        //* IsOwner 属性用于判断当前客户端是否是该对象自己的客户端
        //if (this.IsClient && this.IsOwner)//不行
        if (this.IsClient)
        {
            Debug.Log(nameText.text + " is client");
            //transform.position = new Vector3(-33f, 10f, -17f);
        }
        // 设置玩家名称文本
        nameText.text = "Player" + clientId.Value.ToString(); //!因为在OnNetworkSpawn函数后调用 已经设置了clientId的值
        // 设置颜色
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = colors[clientId.Value % colors.Length];
    }

    public override void OnNetworkSpawn()
    //*当网络对象被创建时调用 因为继承了NetworkBehaviour类
    //!在start函数前调用  
    //*OwerClientId 是一个只读属性 用于获取当前对象的所有者客户端ID ID从0开始(0,1,2...) 该属性在NetworkBehaviour类中定义
    {
        if (IsServer)//判断当前实例是否运行在服务器上
        {
            Debug.Log(nameText.text + " is server, id: " + this.OwnerClientId);
            clientId.Value = (int)this.OwnerClientId;
            Debug.Log("Player OnNetworkSpawn as Server,id: " + this.OwnerClientId);
        }
    }

    void Update()
    {
        if (this.IsClient && this.IsOwner)//控制自己的移动
        {
            Debug.Log(nameText.text+"is  client and owner, id: " + this.OwnerClientId);
            // 计算当前速度
            currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
            lastPosition = transform.position;

            // 更新速度显示
            if (speedText != null)
            {
                speedText.text = $"Speed: {currentSpeed:F2}";
            }

            
            // 获取垂直和水平输入
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            // 获取目标位置和旋转
            Vector3 pos = GetTargetPos(v);
            Quaternion rot = GetTargetRot(h);

            //UpdatePosAndRotServerRpc(pos, rot);//!上传服务器数据

            bool walkState = IsTargetWalk(v, h);
            Debug.Log(nameText.text+"WalkState:"+walkState);
            Debug.Log(nameText.text+"LastState:"+lastWalkState);
            


            if (lastWalkState != walkState)
            {
                Debug.Log(nameText.text+"播放动画");
                AniWlak(walkState);
                lastWalkState = walkState;
                UpdateWalkStateServerRpc(walkState);//!上传服务器数据
            }

            //! 本地移动到目标位置
            Move(pos);
            //! 本地旋转到目标旋转
            Rotate(rot);
        }
        else//控制其他玩家的移动同步
        {
            Debug.Log(nameText.text + " is not owner or client, id: " + this.OwnerClientId);
            // 从网络变量中获取位置和旋转 
            //*从服务器端获取该变量的值 并将其同步到其他客户端
            // if (networkPos.Value != rg.position || networkRot.Value != rg.rotation)
            // {
            //     Move(networkPos.Value);
            //     Rotate(networkRot.Value);
            //     Debug.Log(nameText.text + "      " + "同步移动: " + rg.position + " " + rg.rotation);
            // }

            if (lastWalkState != networkplayerWalkState.Value)
            {
                AniWlak(networkplayerWalkState.Value);
                lastWalkState = networkplayerWalkState.Value;
            }
        }
    }

    private void AniWlak(bool walkState)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalk", walkState);
            //animator.SetTrigger("Walk");
            Debug.Log(nameText.text+"播放动画");
        }
    }

    private bool IsTargetWalk(float v, float h)
    {
        // 判断是否在移动
        if (v != 0)
        {
            return true;
        }
        return false;
    }

    // [ServerRpc]   //*上传数据给服务器端 服务器端执行下面操作
    // public void UpdatePosAndRotServerRpc(Vector3 pos, Quaternion rot)
    // {
    //     Debug.Log(nameText.text + " UpdatePosAndRotServerRpc: " + pos + " " + rot);
    //     //设置网络变量的值 
    //     networkPos.Value = pos;
    //     networkRot.Value = rot.normalized;
    // }

    [ServerRpc]   //*上传数据给服务器端 服务器端执行下面操作
    public void UpdateWalkStateServerRpc(bool walkState){

        networkplayerWalkState.Value = walkState;

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
        rg.MovePosition(Vector3.Lerp(rg.position, pos, Time.deltaTime * 5));
    }

    private Quaternion GetTargetRot(float h)
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, h * rotationSpeed * Time.deltaTime, 0f);
        Quaternion finalRot = rg.rotation * deltaRotation;
        return finalRot;
    }

    private void Rotate(Quaternion rot)
    {
        rg.MoveRotation(Quaternion.Lerp(rg.rotation, rot, Time.deltaTime * 5));
    }

    private void OnTriggerEnter(Collider other)//!碰撞和Update都要判断是服务器端还是客户端
    {

        if (other.CompareTag("Coin"))
        {
            //! 只有是该客户端的玩家才会触发该事件
            if (this.IsOwner)
            {
                //other.gameObject.SetActive(false);//!只修改了本地的可见性 并没有修改服务器端的可见性

                other.gameObject.GetComponent<CoinManager>().SetActive(false);//调用CoinManager脚本中的SetActive方法 该方法会将网络变量的值同步到其他客户端
            }
        }
        else if (other.CompareTag("Player"))
        {
            ulong clientId = other.gameObject.GetComponent<NetworkObject>().OwnerClientId;//*NetworkObject类中也有OwnerClientId属性
            UpdatePlayerMeetServerRpc(this.OwnerClientId, clientId);//上传数据给服务器端 服务器端会将数据同步到其他客户端
            Debug.Log($"玩家相遇事件1：来自 {this.OwnerClientId} -> 目标 {clientId}");
        }
    }
    [ServerRpc(RequireOwnership = false)] //*上传数据给服务器端 服务器端执行下面操作
    public void UpdatePlayerMeetServerRpc(ulong from, ulong to)
    {
        //* 需要 指定 的客户端的信息
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { to }
            }
        };

        NotifyPlayerMeetClientRpc(from, clientRpcParams);//*分发指定的客户端
        Debug.Log($"玩家相遇事件2：来自 {from} -> 目标 {to}");
    }

    [ClientRpc] //*通知指定的客户端
    public void NotifyPlayerMeetClientRpc(ulong from, ClientRpcParams clientRpcParams = default)
    {
        if (!this.IsOwner)//判断当前实例是否运行在客户端上
        {
            Debug.Log($"玩家相遇事件3：来自 {from} -> 目标 {this.OwnerClientId}");
        }
    }

}
