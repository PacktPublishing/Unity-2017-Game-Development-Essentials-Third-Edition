using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 12f;
		[Range(1f, 4f)] [SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.5f;

        // Additional serialized stuff (added for the book, for the player character)
        [SerializeField] AudioClip rightStepSound;
        [SerializeField] AudioClip leftStepSound;
        [SerializeField] AudioClip jumpSound;
        [SerializeField] AudioClip landSound;
        [SerializeField] AudioClip attackSound;
        [SerializeField] AudioClip beingHitSound;

        Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;
        int m_Fighting_Style;

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
        }

        private Transform rightHandRef; // right hand transform reference in the skeleton 

        void Start()
		{
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;

            m_Animator.stabilizeFeet = true;

            if (this.tag == "Player")
            {
                Debug.Log("seeking Right Hand");
                rightHandRef = null;
                rightHandRef = transform.Find("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");
                if (rightHandRef == null) Debug.LogError("missing transform");
                rightHandRef.GetComponent<Collider>().enabled = false;
            }

		}

        void FixedUpdate()
        {
            //Check Enemy Proximity, only if this instance of ThirdPersonCharacter is running on the "Player"
            if (this.tag == "Player")
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject e in enemies)
                {
                    if (e.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled)
                    {
                        float d = Vector3.Distance(transform.position, e.transform.position);
                        if (d <= 2f) this.SendMessage("ChangeHealth", (2f - d) * .001f);
                    }
                }
            }
        } 

        public void Move(Vector3 move, bool crouch, bool jump)
		{
			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired direction.
			if (move.sqrMagnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();

			move = Vector3.ProjectOnPlane(move, m_GroundNormal);
			m_TurnAmount = Mathf.Atan2(move.x, move.z);

			m_ForwardAmount = move.z;
            if (m_Fighting_Style==0) ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded)
			{
				HandleGroundedMovement(crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}

			//ScaleCapsuleForCrouching(crouch);
			PreventStandingInLowHeadroom();

			// send input and other state parameters to the animator
			UpdateAnimator(move);
		}


		void ScaleCapsuleForCrouching(bool crouch)
		{
			if (m_IsGrounded && crouch)
			{
				if (m_Crouching) return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					//m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!m_Crouching)
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
				{
					//m_Crouching = true;
				}
			}
		}

		void UpdateAnimator(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            // We don't need it for our game demo both AI enemy controller and for Hero playing character either 
            //m_Animator.SetBool("Crouch", m_Crouching);

            m_Animator.SetBool("OnGround", m_IsGrounded);
			if (!m_IsGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;

            //if (this.gameObject.layer==8) Debug.Log("JumpLeg:" + m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            if (m_IsGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}

		}

        void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			m_Rigidbody.AddForce(extraGravityForce);

			m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}
			
		void HandleGroundedMovement(bool crouch, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && !crouch && 
                    ( m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")
                    // added for the book to "quickly and dirty" manage the old man NPC
                    || m_Animator.GetCurrentAnimatorStateInfo(0).IsName("WalkToPoint") 
                    || m_Animator.GetCurrentAnimatorStateInfo(0).IsName("WalkCircle") 
                    )
                )
            {
                // jump!
                GetComponent<AudioSource>().clip = jumpSound;
                GetComponent<AudioSource>().Play();
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
				m_IsGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}

        // Player footsteps (driven from Mecanim animation events)
        public void FootStep(int isRightStep)
        {
            if(isRightStep==0)
                GetComponent<AudioSource>().clip = rightStepSound;
            else
                GetComponent<AudioSource>().clip = leftStepSound;
            GetComponent<AudioSource>().Play();
        }

        // To keep this class "universal" for the player and AI characters
        // we need to make a difference here, this technique is called method overloading
        // The method has the same name, but takes different parameters inputs
        public void Fight()
        {
            if (m_Fighting_Style == 0 && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Base.Punch") && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Base.Throw"))
            {
                m_Fighting_Style = Random.Range(1, 3);
                m_Animator.SetInteger("FightStyle", m_Fighting_Style);
            }
        }

        // Called usually by the player, can set fight style and throwable weapons prefab reference in
        public void Fight(int style)
        {
            if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Base.Punch"))
            {
                Debug.Log("ISPUNCHING");
                return;
            }
            if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Throw"))
            {
                Debug.Log("ISTHROWING");
                return;
            }
            //Debug.Log("inTransition:"+ m_Animator.IsInTransition(0)); // ininfluent

            if (m_Fighting_Style == 0 && m_Animator.GetInteger("FightStyle")==0 && 
               !m_Animator.IsInTransition(0) )
            {                
                m_Animator.SetInteger("FightStyle", style);
                if (style == 1)
                {
                    rightHandRef.GetComponent<Collider>().enabled = true;

                }
            }
            
        }

        // Throw a stone from the right hand
        public void ThrowStone()
        {
            GetComponent<AudioSource>().clip = attackSound;
            GetComponent<AudioSource>().Play();
            SendMessage("ThrowObject", rightHandRef);
            //GetComponent<StoneLauncher>().ThrowObject(rightHandRef);
        }

        // Chapter 9/10 collects artifact pieces and stones for fighting
        public void ObjectGrabbed() { m_Animator.SetBool("Collecting", false); }

        // Chapter 8 AI/PLAYER
        public void EndOfSlash() { m_Fighting_Style = 0; m_Animator.SetInteger("FightStyle", 0); rightHandRef.GetComponent<Collider>().enabled = false; }

        // Chapter 8 NPC
        public void AnimatorSetWalkCircle(bool flag)
        {
            m_Animator.SetBool("Walking", flag);
        }

		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (m_IsGrounded && Time.deltaTime > 0)
			{
				Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
			}
		}

        public bool Grounded(){ return m_IsGrounded; }

		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), ( transform.position+(Vector3.up *0.1f) )+ (Vector3.down * m_GroundCheckDistance), Color.blue);
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
			{
				m_GroundNormal = hitInfo.normal;
                // if it wasn't grounded, this frame must play the "landed" audio clip
                if (m_IsGrounded == false)
                {
                    // check component and clip presence
                    if (GetComponent<AudioSource>() != null && landSound!=null)
                    {
                        GetComponent<AudioSource>().clip = landSound;
                        GetComponent<AudioSource>().Play();
                    }
                }
                m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
                
			}
			else
			{
				m_IsGrounded = false;
				m_GroundNormal = Vector3.up;
				m_Animator.applyRootMotion = false;
			}
        }

	}
}
