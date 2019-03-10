using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Navigation;
using Common.Mobs;
using Common.Navigation;
using GeometRi;
using UnityEngine;
using UnityEngine.AI;
using NavMesh = UnityEngine.AI.NavMesh;

namespace Assets.Scripts.Mob
{
    public class MobBehaviour : MonoBehaviour
    {
        public MobWanderInformation Information;
        public Vector3 Offset;

        private void OnDrawGizmosSelected()
        {
            if (Information.WanderingCenter == null)
            {
                Gizmos.DrawWireSphere(this.transform.position, Information.Radius);
                return;
            }
            else
            {
                Gizmos.DrawWireSphere(new Vector3(Information.WanderingCenter.X, this.transform.position.y, Information.WanderingCenter.Z), Information.Radius);
            }

            foreach (var s in Information.WanderingSegments)
            {
                Gizmos.DrawLine(new Vector3(s.A.X, this.transform.position.y, s.A.Z), new Vector3(s.B.X, this.transform.position.y, s.B.Z));
            }

            Gizmos.DrawCube(new Vector3(Information.GoalPosition.X, this.transform.position.y, Information.GoalPosition.Z), Vector3.one * 0.4f);
        }

        public void Start()
        {
            Information.WanderingSegments = new NavMeshExtractor().GetOutline(new Point3d(this.transform.position.x, 0, this.transform.position.z),
                Information.Radius);
            Information.Initialize(DateTime.UtcNow, new NavMeshVector()
            {
                X = this.transform.position.x,
                Y = this.transform.position.y,
                Z = this.transform.position.z
            });
        }

        public void Update()
        {
            Information.CalculateNextPoint(DateTime.UtcNow);
            var pos = Information.GetPosition(DateTime.UtcNow);
            NavMeshHit navHit;
            var vecPos = new Vector3((float) pos.X, (float)pos.Y, (float) pos.Z);
            
            if (NavMesh.SamplePosition(vecPos, out navHit, float.MaxValue, -1))
            {
                var newPos = navHit.position + Offset;
                this.transform.LookAt(newPos);
                this.transform.position = newPos;
            }
        }
    }
}