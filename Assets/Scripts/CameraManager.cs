using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Transform target; // 要跟随的目标对象
    private Vector3 offset; // 相机与目标对象的偏移量

    void Start()
    {
        if (target != null)
        {
            // 计算并设置初始偏移量
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算相机的目标位置
        Vector3 targetPosition = target.position + offset;

        // 设置相机的位置
        transform.position = targetPosition;
    }
}
 