using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling
        public Transform target;                                    // target to aim for

        public float distanceForEngage; // For BASIC AI, when the distance player-enemy is below this threshold, the enemy will start to pursue the player
        private bool targetInSight;

        // Advanced AI variables
        public bool advancedAI;
        public enum State
        {
            ROAM,
            CHASE,
            WONDER
        }

        public State state;
        private bool alive;
        private Vector3 lastPlayerSeen;

        public GameObject[] waypoints;
        private int waypointInd = 0;
        public float roamSpeed = 0.7f;
        public float chaseWaitTime = 2f;
        private float chaseTimer;

        //Chase/Run
        public float runSpeed = .75f;

        //Wonder
        private Vector3 wonderPosition;
        private float wonderTimer = 0;
        public float WonderWait = 5;

        //Sight
        public float heightMultiplier;
        public float sightDST = 10;

        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;

            if (target != null) targetInSight = true;

            if(advancedAI)
            {
                state = State.ROAM;
                alive = true;
                heightMultiplier = 1.36f;
                StartCoroutine("ISM");
            }

        }

        IEnumerator ISM()
        {
            while (alive)
            {
                switch (state)
                {
                    case State.ROAM:
                        Roam();
                        break;
                    case State.CHASE:
                        Chase();
                        break;
                    case State.WONDER:
                        Wonder();
                        break;
                }
                yield return null;
            }
        }

        void Roam()
        {
            agent.speed = roamSpeed;
            if (Vector3.Distance(this.transform.position, waypoints[waypointInd].transform.position) >= 2)
            {
                agent.SetDestination(waypoints[waypointInd].transform.position);
                character.Move(agent.desiredVelocity, false, false);

            }
            else if (Vector3.Distance(this.transform.position, waypoints[waypointInd].transform.position) <= 2)
            {
                waypointInd += 1;
                if (waypointInd >= waypoints.Length)
                {
                    waypointInd = 0;
                }
            }
            else
            {
                character.Move(Vector3.zero, false, false);
            }
        }

        void Chase()
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer < chaseWaitTime)
            {
                agent.SetDestination(this.transform.position);
                character.Move(Vector3.zero, false, false);
            }
            if (chaseTimer >= chaseWaitTime)
            {
                agent.speed = runSpeed;
                agent.SetDestination(target.position); // localPosition);
                character.Move(agent.desiredVelocity, false, false);
            }
        }

        void Wonder()
        {
            wonderTimer += Time.deltaTime;
            agent.SetDestination(this.transform.position);
            character.Move(Vector3.zero, false, false);
            transform.LookAt(wonderPosition);
            if (wonderTimer >= WonderWait)
            {
                state = State.ROAM;
                wonderTimer = 0;
            }

        }

        void OnTriggerExit(Collider coll)
        {
            if (coll.tag == "Player")
            {
                state = State.WONDER;
                //chaseTimer = 0; // added
                wonderPosition = coll.gameObject.transform.position;
            }
        }

        private void Update() 
        {

            // AI Logic

            if (targetInSight)
            {
                if (target != null)
                {
                    if (Vector3.Distance(transform.position, target.position) < distanceForEngage)
                        agent.SetDestination(target.position);
                    else
                        agent.SetDestination(transform.position);
                }

                if (agent.remainingDistance > agent.stoppingDistance)
                    character.Move(agent.desiredVelocity, false, false);
                else
                    character.Move(Vector3.zero, false, false);
            }
            else // back idle / patroling
            {
               // character.Move(Vector3.zero, false, false);
            }
         

        }

        // This method can be called by any instance referenced in other classes
        public void SetTarget(Transform target)
        {
            targetInSight = true; // false?true:(target==null);
            this.target = target; // change the target with the passed in target object
        }
    }
}
