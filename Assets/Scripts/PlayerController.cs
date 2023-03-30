using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
   [SerializeField] private float speed = 3f;

   public static event System.Action GameOverEvent;

    private Camera mainCamera;
    private Vector3 mouseInput;
    private PlayerLength _playerLength;
    private bool _canCollide = true;

    private readonly ulong[] _targetClientsArray = new ulong[1];
    private void Initialize()
    {
        mainCamera = Camera.main;
        _playerLength = GetComponent<PlayerLength>();

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
        mouseWorldCoordinates.z = 0f;

        transform.position = Vector3.MoveTowards(current: transform.position, target: mouseWorldCoordinates, maxDistanceDelta:Time.deltaTime * speed);

        if(mouseWorldCoordinates != transform.position)
        {
            Vector3 targetDirection = mouseWorldCoordinates - transform.position;
            targetDirection.z = 0f;
            transform.up = targetDirection;
        }

        float screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        float screenBottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float screenTop = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        Vector3 playerPosition = transform.position;
        float clampedX = Mathf.Clamp(playerPosition.x, screenLeft, screenRight);
        float clampedY = Mathf.Clamp(playerPosition.y, screenBottom, screenTop);
        playerPosition = new Vector3(clampedX, clampedY, playerPosition.z);
        transform.position = playerPosition;
    }

    [ServerRpc]
    private void DetermineCollisionWinnerServerRpc(PlayerData player1, PlayerData player2)
    {
        if (player1.Length > player2.Length)
        {
            WinInformationServerRpc(player1.Id, player2.Id);
        }
        else
        {
            WinInformationServerRpc(player2.Id, player1.Id);
        }
    }

    [ServerRpc]
    private void WinInformationServerRpc(ulong winner, ulong loser)
    {
        _targetClientsArray[0] = winner;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                //TargetClientIds = new ulong[] { winner }
                TargetClientIds = _targetClientsArray
            }
        };
        AtePlayerClientRpc(clientRpcParams);

        _targetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = _targetClientsArray;
        GameOverClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You Ate a Player!");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        Debug.Log("You Lose!");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Check if we are colliding with another player
        if (!col.gameObject.CompareTag("Player")) return;
        // Only the owner of the Networked Object can continue running the function
        if (!IsOwner) return;
        // Have we collided recently? If not, continue.
        if (!_canCollide) return;
        //StartCoroutine(CollisionCheckCoroutine());

        // Head-on Collision
        if (col.gameObject.TryGetComponent(out PlayerLength playerLength))
        {
            // Populate the serialized structs to send Player Data to the ServerRpc
            var player1 = new PlayerData()
            {
                Id = OwnerClientId,
                Length = _playerLength.length.Value
            };
            var player2 = new PlayerData()
            {
                Id = playerLength.OwnerClientId,
                Length = playerLength.length.Value
            };
            // Called on the server from the client to determine the winner
            DetermineCollisionWinnerServerRpc(player1, player2);
        }
        // Collision with a tail GameObject. Results in a loss.
        else if (col.gameObject.TryGetComponent(out Tail tail))
        {
            WinInformationServerRpc(tail.networkedOwner.GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        }
    }

    struct PlayerData : INetworkSerializable
    {
        public ulong Id;
        public ushort Length;

       public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T:
           IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Length);

        }
    }
}
