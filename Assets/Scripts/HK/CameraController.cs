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
}
