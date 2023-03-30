using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLength : NetworkBehaviour
{
    [SerializeField] private GameObject tailPrefab;

    public NetworkVariable<ushort> length = new NetworkVariable<ushort>(value: 1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private List<GameObject> tails;
    private Transform lastTail;
    private Collider2D collider2D;

    public static event System.Action<ushort> ChangedLengthEvent;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        tails = new List<GameObject>();
        lastTail = transform;
        collider2D = GetComponent<Collider2D>();
        if (!IsServer) length.OnValueChanged += LengthChangedEvent;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        DestroyTails();
    }

    private void DestroyTails()
    {
        while (tails.Count != 0)
        {
            GameObject tail = tails[0];
            tails.RemoveAt(index: 0);
            Destroy(tail);
        }
    }

    [ContextMenu(itemName:"Add Length")]
    public void AddLength()
    {
        length.Value += 1;
        LengthChanged();
    }

    private void LengthChanged()
    {
        InstantiateTail();

        if (!IsOwner) return;
        ChangedLengthEvent?.Invoke(length.Value);
        ClientMusicPlayer.Instance.PlayAudioClip();
    }

    private void LengthChangedEvent(ushort previousValue, ushort newValue)
    {
        Debug.Log("LengthChanged Callback");
        LengthChanged();
    }

    private void InstantiateTail()
    {
        GameObject tailGameObject = Instantiate(tailPrefab, transform.position, Quaternion.identity);
        tailGameObject.GetComponent<SpriteRenderer>().sortingOrder = -length.Value;
        if (tailGameObject.TryGetComponent(out Tail tail))
        {
            tail.networkedOwner = transform;
            tail.followTransform = lastTail;
            lastTail = tailGameObject.transform;
            Physics2D.IgnoreCollision(tailGameObject.GetComponent<Collider2D>(), collider2D);

        }

        tails.Add(tailGameObject);
    }

}
