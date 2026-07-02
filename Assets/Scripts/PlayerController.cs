using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMove : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Transform camTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 获取主相机的 Transform 引用
        if (Camera.main != null)
            camTransform = Camera.main.transform;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (camTransform != null)
        {
            // 1. 获取相机的水平方向（忽略 Y 轴高度差，防止角色钻地或飞天）
            Vector3 camForward = Vector3.Scale(camTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(camTransform.right, new Vector3(1, 0, 1)).normalized;

            // 2. 根据相机方向计算最终移动向量
            Vector3 move = (v * camForward + h * camRight).normalized;

            // 3. 应用速度，保留刚体原有的重力速度
            rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

            // 4. (可选) 让角色转向移动的方向，看起来更自然
            if (move.magnitude > 0.1f)
            {
                transform.forward = move;
            }
        }
    }
}