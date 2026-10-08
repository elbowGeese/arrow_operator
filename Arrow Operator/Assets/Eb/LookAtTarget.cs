using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private Camera minimapCam;

    void Start()
    {
        minimapCam = GameObject.FindWithTag("MinimapCam").GetComponent<Camera>();
        if(target == null) { target = minimapCam.transform; }
    }

    void Update()
    {
        Vector3 targetLook = new Vector3(target.position.x +  offset.x, target.position.y + offset.y, target.position.z + offset.z);
        transform.LookAt(targetLook);
    }
}
