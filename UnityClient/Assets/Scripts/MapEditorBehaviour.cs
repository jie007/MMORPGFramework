using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interactables;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class MapEditorBehaviour : MonoBehaviour
    {
        public string Name = "StartMap";

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

            return new MapInformation()
            {
                MapName = Name,
                Interactables = interactables
            };

        }
    }
}
