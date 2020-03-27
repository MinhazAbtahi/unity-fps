using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ViridaxGameStudios.AI
{
    public class AIController : Character
    {
        //The main finite state machine that controls all states of this AI
        private FSM fsm;

        private FSMState idleState;
        private FSMState followState;
        private FSMState fleeState;
        private FSMState patrolState;
        private FSMState attackState;
        private FSMState guardState;
        private FSMState dameState;
        private FSMState deadState;

        private IdleAction idleAction;
        private MoveAction followMoveAction;
        private MoveAction patrolMoveAction;
        private AttackAction attackAction;
        private PatrolAction patrolAction;

        


        public GameObject target;
        public GameObject headLookTarget;
        public bool enemyFound = false;
        private float m_HitPoints;
        public bool pointReached = false;
        private GameObject patrolTarget;
        
        public bool enablePathfinding;
        public bool enableOA;
        public bool enableDebug;
        private Vector3[] path;
        private int targetIndex;
        public Vector3 currentWaypoint;

        private Unit unit;

        ObjectScanner objectScanner;

        #region Main Methods
        private void Awake()
        {
            Animator = GetComponent<Animator>();
            unit = GetComponent<Unit>();
            //Create the Finite State Machine
            fsm = new FSM("AI Controller FSM");

            //Create the states and actions
            idleState = fsm.AddState(CharacterStates.STATE_IDLE);
            followState = fsm.AddState(CharacterStates.STATE_FOLLOW);
            attackState = fsm.AddState(CharacterStates.STATE_ATTACK);
            patrolState = fsm.AddState(CharacterStates.STATE_PATROL);
            idleAction = new IdleAction(idleState, this);
            followMoveAction = new MoveAction(followState, this);
            patrolMoveAction = new MoveAction(patrolState, this);
            patrolAction = new PatrolAction(patrolState, this);
            attackAction = new AttackAction(attackState, this);

            //Add actions to the states
            idleState.AddAction(idleAction);
            followState.AddAction(followMoveAction);
            attackState.AddAction(attackAction);
            patrolState.AddAction(patrolAction);
            patrolState.AddAction(patrolMoveAction);
            //Add transitions to the states
            idleState.AddTransition("ToFollow", followState);
            followState.AddTransition("ToIdle", idleState);
            followState.AddTransition("ToAttack", attackState);
            attackState.AddTransition("ToIdle", idleState);
            attackState.AddTransition("ToFollow", followState);
            patrolState.AddTransition("ToIdle", idleState);
            patrolState.AddTransition("ToFollow", followState);

            

            //ScanForObjects(gameObject.transform.position, m_DetectionRadius);
        }
        public override void Start()
        {
            base.Start();
            
            
            m_HitPoints = HitPoints;
            objectScanner = new ObjectScanner(this, onObjectFound);

            //Initialise all actions
            idleAction.Init(Animator, "ToFollow");
            followMoveAction.Init(transform, Animator, MovementTypes.MOVE_RUN, "ToIdle", unit);
            patrolMoveAction.Init(transform, Animator, MovementTypes.MOVE_WALK, "ToIdle", unit);
            patrolAction.Init(transform, Animator, "ToIdle");
            attackAction.Init(Animator);
            fsm.Start(CharacterStates.STATE_IDLE);

        }
        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            Debug.Log("Enemy Found: " + enemyFound);
            fsm.Update();
            ProcessState();
            objectScanner.ScanForObjects(gameObject.transform.position, m_DetectionRadius);
            
        }
        #endregion
        private void OnAnimatorIK(int layerIndex)
        {
            if(enableHeadLook && headLookTarget != null)
            {
                Animator.SetLookAtPosition(headLookTarget.transform.position);
                Animator.SetLookAtWeight(headLookIntensity);
            }
            
        }
        void onObjectFound(bool foundEnemy, GameObject enemy)
        {
            enemyFound = foundEnemy;
            if(enemyFound)
            {
                target = enemy;
            }
            if(!isPatrolling && !enemyFound)
            {
                target = null;
            }
        }
        #region Override Methods
        public override void ReceiveDamage(float damage)
        {
            base.ReceiveDamage(damage);
        }
        public override void ResumeFromDamage()
        {
            base.ResumeFromDamage();
        }
        public override void CharacterDead()
        {
            base.CharacterDead();
        }
        #endregion

        #region Helper Methods



        public void Attack()
        {
            //
            //Method Name : void Attack()
            //Purpose     : This method is called by the attack animation event. Deals the required damage to all targets in range..
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            RaycastHit[] hits;
            hits = Physics.SphereCastAll(transform.position, m_AttackRange, transform.forward);
            foreach (RaycastHit hit in hits)
            {
                bool isHittable = false;
                foreach(string tag in enemyTags)
                {
                    if (tag.Equals(hit.transform.gameObject.tag))
                    {
                        isHittable = true;
                    }
                }
                if (isHittable)
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    float angle = Vector3.Angle(hit.transform.position - transform.position, transform.forward);
                    if (angle <= m_DamageAngle / 2 && distance <= m_AttackRange)
                    {
                        hit.transform.gameObject.SendMessage("ReceiveDamage", Damage);
                    }
                }
            }



        }
        
        void ProcessState()
        {
            if (target != null)
            {
                if(isPatrolling && !enemyFound)
                {
                    if(!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_PATROL))
                    {
                        fsm.ChangeToState(patrolState);
                    }
                }
                else if (enemyFound)
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    if (distance <= m_AttackRange)
                    {

                        if (!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_ATTACK))
                        {
                            fsm.ChangeToState(attackState);
                        }

                    }
                    else
                    {
                        if (!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_FOLLOW))
                        {
                            fsm.ChangeToState(followState);
                        }
                    }
                }
                else
                {
                    if(!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_IDLE))
                    {
                        fsm.ChangeToState(idleState);
                    }
                }
            }
            else
            {
                if(isPatrolling)
                {
                    if (!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_PATROL))
                    {
                        fsm.ChangeToState(patrolState);
                    }
                }
                else
                {
                    if (!fsm.GetCurrentState.Name.Equals(CharacterStates.STATE_IDLE))
                    {
                        fsm.ChangeToState(idleState);
                    }
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (path != null)
            {
                for (int i = targetIndex; i < path.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(path[i], Vector3.one);
                    if (i == targetIndex)
                    {
                        Gizmos.DrawLine(transform.position, path[i]);
                    }
                    else
                    {
                        Gizmos.DrawLine(path[i - 1], path[i]);
                    }
                }
            }
        }
        #endregion
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag.Equals("PatrolPoint") && collision.gameObject.name.Equals(target.gameObject.name))
            {
                pointReached = true;
            }
            

        }
    }

    
}

