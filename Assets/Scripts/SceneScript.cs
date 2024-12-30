using System;
using UnityEngine;

namespace Game
{
    public class SceneScript : MonoBehaviour
    {
        public float AngleSpeed;
        public Vector3 Axis;
        public Transform Cube;

        private void Update()
        {
            if (Cube != null)
            {
                Cube.Rotate(Axis, AngleSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}