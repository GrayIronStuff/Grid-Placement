using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform follow;
        [SerializeField] private float smoothing;

        [SerializeField] private float offsetY;
        [SerializeField] private float offsetZ;

        private Vector3 _velocity;
        private void Update()
        {
            Follow();
        }

        private void Follow()
        {
            var transform1 = follow.transform.position;
            var pos = new Vector3
            {
                //Set camera's x axis follow the player's x axis.
                x = transform1.x,
                //Set camera's position z axis to player's z axis p(any amount) unit(s) down.
                y = transform1.y + offsetY,
                //Set camera's y axis follow the player's y axis.
                z = transform1.z - offsetZ
            };

            //Transform camera's position to the player's position depending on pos while smoothing it.
            transform.position = Vector3.SmoothDamp(transform.position, pos, ref _velocity, smoothing);
        }
    }
}