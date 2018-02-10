using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////
// Modified specifically for Unity 2017 Game Development Essentials codes examples
/////////////////////////////////////////////////////////////////////////////////
namespace UnityStandardAssets.Effects
{
    public class FireLight : MonoBehaviour
    {
        private float m_Rnd;
        private bool m_Burning = true;
        private Light m_Light;
        private float linearDistance;

        // Added for UGDE
        public Color startColor = new Color(1f,.45f,0f);    // Light starting color
        public Color endColor = new Color(1f,.95f,.55f);    // Light ending color
        public Vector3 mOffset = new Vector3(.5f,1.5f,.5f); // movement offset
        [Range(0f, 2f)]   public float mSpeed = 1f;         // movement speed
        [Range(0.1f, 1f)] public float mMultiplier = 0.5f;  // movement multiplier
        [Range(1f, 100f)] public float renderDistance;      // after this distance disable light and animation
        public bool startBurning;                           // should this fire light start enabled ?
        

        private void Start()
        {
            m_Rnd = Random.value* 100;
            m_Light = GetComponent<Light>();
            m_Burning = startBurning;
            m_Light.enabled = startBurning;
        }

         void Update()
        {
            linearDistance = Mathf.Abs(Vector3.Distance(transform.position, Camera.main.transform.position));
            if (linearDistance > renderDistance) Extinguish();
            if (linearDistance <= renderDistance) SetOnFire();


            if (m_Burning)
            {
                /////////////////////////////////////////////////////////////////////////////////
                // Added and modified specifically for Unity Game Development Essentials examples
                /////////////////////////////////////////////////////////////////////////////////

                // Oscillates the light color between start and end light colors over time
                m_Light.color = new Color( 
                    ( startColor.r + ((endColor.r - startColor.r)*.5f)) +
                        (Mathf.Sin(Time.time*3f) *((endColor.r - startColor.r)*.5f)), 
                    ( startColor.g + ((endColor.g - startColor.g)*.5f)) + 
                        (Mathf.Sin(Time.time*3f) *((endColor.g - startColor.g)*.5f)),
                    ( startColor.b + ((endColor.b - startColor.b)*.5f)) + 
                        (Mathf.Sin(Time.time*3f) *((endColor.b - startColor.b)*.5f))
                 );
                // Oscillates the light intensity over time
                m_Light.intensity = 2*Mathf.PerlinNoise(m_Rnd + Time.time, m_Rnd + 1 + Time.time*mMultiplier);
                // Move it around by changing the 3 coordinates for the final vector
                float x = Mathf.PerlinNoise(m_Rnd + 0 + Time.time* mSpeed, m_Rnd + 1 + Time.time* mSpeed) - mOffset.x;
                float y = Mathf.PerlinNoise(m_Rnd + 2 + Time.time* mSpeed, m_Rnd + 3 + Time.time* mSpeed) - mOffset.y;
                float z = Mathf.PerlinNoise(m_Rnd + 4 + Time.time* mSpeed, m_Rnd + 5 + Time.time* mSpeed) - mOffset.z;
                // Apply finally the movement on the transform local position
                transform.localPosition = Vector3.up + new Vector3(x, y, z) * mMultiplier;
            }
        }

        public void Extinguish()
        {
            m_Burning = false;
            m_Light.enabled = false;
        }

        public void SetOnFire()
        {
            m_Burning = true;
            m_Light.enabled = true;
        }
    }
}
