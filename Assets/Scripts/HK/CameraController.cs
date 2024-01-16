using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
public float zoomSpeed = 100f;
    public float moveSpeed = 50f;
    public float rotationSpeed = 50f;
    public Vector3 cameraPosition;
    public Quaternion cameraRotation;
    private Vector3 lastMousePosition;
    void Start()
    {
        cameraPosition = transform.position;
        cameraRotation = transform.rotation;
    }
    void Update()
    {
        UseWASD();


        if(Input.GetKey(KeyCode.Space))
        {
            // 按下空格键时将摄像机位置重置为初始位置
            transform.position = cameraPosition;
            transform.rotation = cameraRotation;
        }
         if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0)&& Input.GetKey(KeyCode.R))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Translate(-delta.x * Time.deltaTime*moveSpeed, -delta.y * Time.deltaTime*moveSpeed, 0);

            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1)&& Input.GetKey(KeyCode.R))
        {
            float deltaMouseX = Input.mousePosition.x - lastMousePosition.x;

            RotateCameraHorizontally(deltaMouseX);

            lastMousePosition = Input.mousePosition;
        }
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");

        ZoomCameraWithMouseWheel(scrollValue);
    }
     void RotateCameraHorizontally(float deltaMouseX)
    {
        float rotationY = deltaMouseX * rotationSpeed * Time.deltaTime;
        //float rotationX = deltaMouseX * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationY, Space.World);
        //transform.Rotate(Vector3.right, rotationX, Space.World);
    }

    void ZoomCameraWithMouseWheel(float scrollValue)
    {
        Vector3 currentPosition = transform.position;
        currentPosition.z += scrollValue * zoomSpeed;
        transform.position = currentPosition;
    }

    public void UseWASD()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 计算移动方向
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // 将移动方向转换为相机空间
        moveDirection = Camera.main.transform.TransformDirection(moveDirection);
        //moveDirection.y = 0f; // 不允许上下移动

        // 移动相机
        transform.Translate(moveDirection * moveSpeed * 10 * Time.deltaTime, Space.World);
    }

}
