using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform carTransform;
    public Vector3 offset;
    public float cameraFollowSpeed;

    void FixedUpdate()
    {
        transform.position = carTransform.position;
        Vector3 _pos = new Vector3(carTransform.position.x - offset.x, carTransform.position.y - offset.y, carTransform.position.z - offset.z);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, _pos, 20 * Time.deltaTime);
        //Quaternion _rot = Quaternion.identity;
        transform.rotation = carTransform.rotation;
    }
}
