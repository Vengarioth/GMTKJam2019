using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Behaviours
{
    public class Room : MonoBehaviour
    {
        [SerializeField]
        private int _doors = 1;

        [SerializeField]
        private Vector2 _camera;

        [SerializeField]
        private int2 _spawn;

        [SerializeField]
        private Room _nextRoom;

        private int _doorsOpened = 0;

        public void OnFinish()
        {
            _doorsOpened += 1;

            if(_doorsOpened >= _doors)
            {
                if(_nextRoom != null)
                {
                    _nextRoom.Spawn();
                }
            }
        }

        public void Spawn()
        {
            var player = FindObjectOfType<PlayerBehaviour>();
            var camera = FindObjectOfType<Camera>();

            camera.transform.position = new Vector3(_camera.x, _camera.y, camera.transform.position.z);
            player.SetSpawn(_spawn);
            player.Respawn();
        }
    }
}
