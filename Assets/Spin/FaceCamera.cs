using UnityEngine;

namespace Brussels.Crew.Spin
{

    public class FaceCamera : MonoBehaviour
    {
        void Update()
        {
            Transform camPos = Camera.main.transform;
            transform.LookAt(camPos.position);
            transform.Rotate(Vector3.up, 180);
        }
    }

}