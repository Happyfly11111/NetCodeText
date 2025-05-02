using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private Rigidbody rg;
    [SerializeField]
    private float moveSpeed = 10;
    [SerializeField]
    private float rotationSpeed = 180;
    [SerializeField]
    private Text nameText; // 玩家名称文本

    // 定义网络变量 //*告诉服务器端 该变量的变化情况 并将变化同步到其他客户端
    private NetworkVariable<Vector3> networkPos = new NetworkVariable<Vector3>();

    private NetworkVariable<Quaternion> networkRot = new NetworkVariable<Quaternion>();
    private NetworkVariable<int> clientId = new NetworkVariable<int>();
    private Color[] colors = new Color[4]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };



    void Start()
    {
        rg = GetComponent<Rigidbody>();

        //* IsClient 属性用于判断当前实例是否运行在客户端上
        //* IsOwner 属性用于判断当前客户端是否是该对象自己的客户端
        if (this.IsClient && this.IsOwner)
        {
            transform.position = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
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
            clientId.Value = (int)this.OwnerClientId;
            Debug.Log("Player OnNetworkSpawn as Server,id: " + this.OwnerClientId);
        }

    }

    void Update()
    {
        if (this.IsClient && this.IsOwner)//控制自己的移动
        {
            // 获取垂直和水平输入
            float v = Input.GetAxis("Vertical");
            float h = Input.GetAxis("Horizontal");

            // 获取目标位置和旋转
            Vector3 pos = GetTargetPos(v);
            Quaternion rot = GetTargetRot(h);


            UpdatePosAndRotServerRpc(pos, rot);//!上传服务器数据

            //! 本地移动到目标位置
            Move(pos);
            //! 本地旋转到目标旋转
            Rotate(rot);
        }
        else//控制其他玩家的移动同步
        {
            // 从网络变量中获取位置和旋转 
            //*从服务器端获取该变量的值 并将其同步到其他客户端
            Move(networkPos.Value);
            Rotate(networkRot.Value);
        }
    }

    [ServerRpc]   //!得加这个  
    public void UpdatePosAndRotServerRpc(Vector3 pos, Quaternion rot)//*告诉服务器端 该方法的调用情况 并将调用同步到其他客户端
    {
        //设置网络变量的值 //*告诉服务器端 该变量的变化情况 便于后面将变化同步到其他客户端
        networkPos.Value = pos;
        networkRot.Value = rot.normalized;
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
        Quaternion deltaRotation = Quaternion.Euler(0f, h * rotationSpeed * Time.deltaTime, 0f).normalized;
        // 计算目标旋转
        return rg.rotation * deltaRotation;
    }

    private void Rotate(Quaternion rot)
    {
        // 旋转刚体到目标旋转
        rg.MoveRotation(rot.normalized);
    }

    private void OnTriggerEnter(Collider other)
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
    }
}
