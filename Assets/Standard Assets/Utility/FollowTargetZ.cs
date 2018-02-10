using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    public class FollowTargetZ : FollowTarget
    {
        private float dynamicFloat { get; set; }

        private void LateUpdate()
        {
            transform.position = new Vector3(target.position.x/2, offset.y+dynamicFloat, target.position.z+offset.z);
        }
    }
}
