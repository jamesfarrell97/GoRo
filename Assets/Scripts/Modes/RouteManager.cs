using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class RouteManager : MonoBehaviour
    {
        public static RouteManager Instance;

        [SerializeField] public Route[] listOfStraightRoutes;
        [SerializeField] public Route[] listOfLoopedRoutes;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }
}
