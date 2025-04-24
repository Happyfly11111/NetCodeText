using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rg;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float rotationSpeed;

    void Update()
    {
        // 获取垂直和水平输入
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        // 获取目标位置和旋转
        Vector3 pos = GetTargetPos(v);
        Quaternion rot = GetTargetRot(h);

        // 移动到目标位置
        Move(pos);
        // 旋转到目标旋转
        Rotate(rot);
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
