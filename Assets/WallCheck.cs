using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DefaultNamespace
{
    public class WallCheck : MonoBehaviour
    {
        [SerializeField] private bool inside;
        [SerializeField] private Transform point;
        [SerializeField] private NavMeshAgent agent;

        public NavMeshPath path;

        private void OnEnable()
        {
            path = new NavMeshPath();
        }

        private void Update()
        {
            Move();
        }
        private void Move()
        {
            agent.CalculatePath(point.position, path);
            
            if (!inside) return;

            var transform1 = transform;
            transform1.localPosition = new Vector3(0, 0, 2);
            
            agent.Warp(transform1.position);
        }
    }
}
