using System;
using UnityEngine;

namespace Gameplay
{
    public class TBA5AnimNotiffy : MonoBehaviour
    {
        [SerializeField] private GameObject objectToActivate;

        public void SetObjectToActivate()
        {
            objectToActivate.SetActive(true);
        }

        private void OnDisable()
        {
            objectToActivate.SetActive(false);
        }
    }
}