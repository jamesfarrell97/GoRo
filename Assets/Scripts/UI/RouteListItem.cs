using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class RouteListItem : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        private Route route;

        public void SetUp(Route _route)
        {
            route = _route;
            text.text = route.name;
        }

        public void OnClick()
        {
            EventOrganizer.Instance.SetRoute(route);
        }
    }
}
