using System;
using UnityEngine;

namespace UnityStandardAssets.Water
{
    [RequireComponent(typeof(WaterBase))]
    public class GerstnerDisplace : Displace
    {
        public void Start()
        {
            enabled = false;
            enabled = true;
        }
    }
}