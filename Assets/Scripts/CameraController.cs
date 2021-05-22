using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float gridEdgeClamp = 2.0f;


    Camera camera;
    Transform cameraTransform;

    public GridMap<Tile> grid;

    Vector3 mousePos = Vector3.zero;
    Vector3 lastMousePos = Vector3.zero;

    Vector3 moveInput = Vector3.zero;
    Vector3 targetPos = Vector3.zero;

    public float smoothMove = 0.1f;
    Vector3 smoothPosition = Vector3.zero;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        cameraTransform = camera.transform;

        mousePos = ScreenToWorldPoint(Input.mousePosition);
        lastMousePos = mousePos;

        targetPos = camera.transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        ApplyMovement();
    }

    void GetInputs()
    {
        moveInput = Vector3.zero;
        
        mousePos = ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            moveInput = lastMousePos - mousePos;
        }

        targetPos = targetPos + moveInput;

        lastMousePos = mousePos;
    }

    void ApplyMovement()
    {
        Vector3 cameraPos = cameraTransform.position;

        GridMap<Tile>.AABB bounds = grid.GetBounds();
        targetPos = ClampPosition(targetPos, bounds);

        Vector3 cameraMovePos = Vector3.SmoothDamp(cameraPos, targetPos, ref smoothPosition, smoothMove);
        lastMousePos += cameraMovePos - cameraPos;

        cameraTransform.position = cameraMovePos;
    }

    Vector3 ClampPosition(Vector3 position, GridMap<Tile>.AABB bounds)
    {
        return ClampPosition(position, bounds.minX, bounds.minY, bounds.maxX, bounds.maxY);
    }

    Vector3 ClampPosition(Vector3 position, float minX, float minY, float maxX, float maxY)
    {
        // Find the size of the camera in pixels
        Vector3 camSize = Vector3.zero;
        camSize.x = camera.pixelWidth;
        camSize.y = camera.pixelHeight;

        // Find the world Position of the camSize
        camSize = ScreenToWorldPoint(camSize) - cameraTransform.position;

        // Clamp the position
        position.x = Mathf.Clamp(position.x, minX - gridEdgeClamp + camSize.x, maxX + gridEdgeClamp - camSize.x);
        position.y = Mathf.Clamp(position.y, minY - gridEdgeClamp + camSize.y, maxY + gridEdgeClamp - camSize.y);

        return position;
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPos)
    {
        return camera.ScreenToWorldPoint(screenPos);
    }
}
