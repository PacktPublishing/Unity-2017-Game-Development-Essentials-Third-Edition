// License: https://en.wikipedia.org/wiki/MIT_License
// Code created by Jeff Johnson & Digital Ruby, LLC - http://www.digitalruby.com
// Code is from the Free Parallax asset on the Unity asset store: http://u3d.as/bvv
// Code may be redistributed in source form, provided all the comments at the top here are kept intact

using UnityEngine;
using System.Collections;

public class FreeParallaxDemo : MonoBehaviour
{

    public FreeParallax parallax;
    public GameObject cloud;

    // Use this for initialization
    void Start()
    {
        if (cloud != null)
        {
            cloud.GetComponent<Rigidbody2D>().velocity = new Vector2(0.1f, 0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parallax != null)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                parallax.Speed = 15.0f;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                parallax.Speed = -15.0f;
            }
            else
            {
                parallax.Speed = 0.0f;
            }
        }
    }
}
