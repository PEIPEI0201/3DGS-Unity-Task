using UnityEngine;

/// <summary>
/// 轻量第一人称控制器。默认“漂浮模式”：不受重力，锁定初始视高做水平移动，
/// 适配无真实地面几何的全景 3DGS 场景（记忆漫步的飘行感）。
/// 需要一个子物体 Camera 作为俯仰轴。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonWalker : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 2.5f;
    [Tooltip("梦境漂浮模式：不受重力，固定在初始高度水平移动")]
    public bool floatMode = true;
    public float gravity = -9.81f;

    [Header("视角")]
    public float mouseSensitivity = 2f;
    public float minPitch = -75f, maxPitch = 75f;
    [Tooltip("摄像机(俯仰轴)，留空则自动取子物体 Camera")]
    public Transform cameraPivot;

    CharacterController cc;
    float pitch;
    float fixedY;
    float vSpeed;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraPivot == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }
        fixedY = transform.position.y;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 视角
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // 移动
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f) * moveSpeed;

        if (floatMode)
        {
            move.y = 0f;
            cc.Move(move * Time.deltaTime);
            var p = transform.position; p.y = fixedY; transform.position = p; // 锁定视高
        }
        else
        {
            if (cc.isGrounded && vSpeed < 0f) vSpeed = -1f;
            vSpeed += gravity * Time.deltaTime;
            move.y = vSpeed;
            cc.Move(move * Time.deltaTime);
        }
    }
}
