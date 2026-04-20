using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;

    private void Awake()
    {
        if (target == null)
        {
            var characterMovement = FindFirstObjectByType<CharacterMovement>();
            if (characterMovement != null)
            {
                target = characterMovement.transform;
            }
        }

        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
    }
}
