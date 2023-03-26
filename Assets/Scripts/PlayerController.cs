using UnityEngine;
using Unity.Netcode;
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    private Camera mainCamera;
    private Vector3 mouseInput = Vector3.zero;

    private void Initialize()
    {
        mainCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void Update()
    {
        if (!IsOwner || !Application.isFocused) return;
        
        mouseInput.x = Input.mousePosition.x;  
        mouseInput.y = Input.mousePosition.y;  
        mouseInput.z = mainCamera.nearClipPlane;

        Vector3 mouseWorldCoordinates = mainCamera.ScreenToWorldPoint(mouseInput);
        transform.position = Vector3.MoveTowards(current: transform.position,
            target: mouseWorldCoordinates, maxDistanceDelta: Time.deltaTime * speed);

        if (mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDirection = mouseWorldCoordinates - transform.position;
            targetDirection.z = 0f;
            transform.up = targetDirection;
        }
    }

}
