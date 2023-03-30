using UnityEngine;

public class Tail : MonoBehaviour
{
    public Transform networkedOwner;
    public Transform followTransform;

    [SerializeField] private float delayTime = 0.1f;
    [SerializeField] private float distance = 0.3f;
    [SerializeField] private float moveStep = 10f;

    private Vector3 targetPosition;

    private void Update()
    {
        targetPosition = followTransform.position - followTransform.forward * distance;
        targetPosition += (transform.position - targetPosition) * delayTime;
        targetPosition.z = 0f;

        transform.position = Vector3.Lerp(a: transform.position, b: targetPosition, Time.deltaTime * moveStep);
    }

}
