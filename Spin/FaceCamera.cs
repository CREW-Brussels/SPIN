using UnityEngine;

namespace Brussels.Crew.Spin.Spin
{
    /// <summary>
    /// A component that allows an object to always face the camera.
    /// </summary>
    /// <remarks>
    /// Attach this script to a game object to ensure it continuously rotates to face the camera,
    /// which can be useful for effects such as billboarding or UI elements in 3D space.
    /// </remarks>
    public class FaceCamera : MonoBehaviour
    {
        /// <summary>
        /// Updates the orientation of the object so that it always faces the main camera.
        /// </summary>
        /// <remarks>
        /// This method adjusts the object's rotation every frame to ensure it looks at the camera's position.
        /// It then performs an additional rotation of 180 degrees around the up-axis to ensure the correct face is pointing towards the camera.
        /// </remarks>
        void Update()
        {
            Transform camPos = Camera.main.transform;
            transform.LookAt(camPos.position);
            transform.Rotate(Vector3.up, 180);
        }
    }

}