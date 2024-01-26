using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainVCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    void FixedUpdate()
    {
        if (target != null)
        {
            //  float clampedX = Mathf.Clamp(target.position.x, minPosition.x, maxPosition.x);
            // float clampedY = Mathf.Clamp(target.position.y, minPosition.y, maxPosition.y);
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        }
    }

}
