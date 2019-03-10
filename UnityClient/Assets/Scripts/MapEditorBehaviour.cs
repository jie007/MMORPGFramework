using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Mob;
using Assets.Scripts.Navigation;
using Common;
using Common.Interactables;
using Common.Navigation;
using GeometRi;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class MapEditorBehaviour : MonoBehaviour
    {
        public string Name = "StartMap";
        public bool DebugGizmo = true;

        private NavMeshExtractor navMeshExtractor = new NavMeshExtractor();

        private void OnDrawGizmosSelected()
        {
            if (!DebugGizmo)
                return;
            
            var outline = navMeshExtractor.GetOutline();
            foreach (var s in outline)
            {
                GizmoHelper.DrawSegement(s);
            }
        }

        public MapInformation GetMap()
        {
            var interactables = GameObject.FindObjectsOfType<InteractableBehaviour>().Select(x =>
            {
                var config = x.Konfiguration;
                config.PositionX = x.transform.position.x;
                config.PositionY = x.transform.position.y;
                config.PositionZ = x.transform.position.z;
                return config;
            }).ToList();

            var mobs = GameObject.FindObjectsOfType<MobBehaviour>().Select(x =>
            {
                x.Information.WanderingSegments = new NavMeshExtractor().GetOutline(new Point3d(this.transform.position.x, 0, this.transform.position.z),
                    x.Information.Radius);
                x.Information.Initialize(DateTime.UtcNow, new NavMeshVector()
                {
                    X = x.transform.position.x,
                    Y = x.transform.position.y,
                    Z = x.transform.position.z
                });
                return x.Information;
            }).ToList();

            return new MapInformation()
            {
                MapName = Name,
                Interactables = interactables,
                NavMesh = new Common.Navigation.NavMesh()
                {
                    NavMeshOutline = navMeshExtractor.GetOutline()
                },
                Mobs = mobs
            };

        }
    }
}
