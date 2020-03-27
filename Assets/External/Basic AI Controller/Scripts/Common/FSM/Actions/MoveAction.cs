using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViridaxGameStudios.AI;
namespace ViridaxGameStudios
{
    public class MoveAction : FSMAction
    {
        private Transform transform;
        private Animator animator;
        private string finishEvent;
        private float size;
        private int moveType;
        Vector3[] path;
        Vector3 currentWaypoint;
        int targetIndex;
        bool canMove = false;
        Unit unit;
        public MoveAction(FSMState owner, AIController aiController) :base(owner, aiController)
        {

        }

        public void Init(Transform transform, Animator animator, int moveType = 0, string finishEvent = null, Unit unit = null)
        {
            this.transform = transform;
            this.animator = animator;
            this.finishEvent = finishEvent;
            size = transform.localScale.x;
            this.moveType = moveType;
            this.unit = unit;
        }
        public override void OnEnter()
        {
            switch (moveType)
            {
                case MovementTypes.MOVE_WALK:
                    animator.SetBool("isWalking", true);
                    break;
                case MovementTypes.MOVE_RUN:
                    animator.SetBool("isRunning", true);
                    break;
                default:
                    break;
            }
            if (aiController.enablePathfinding)
            {
                unit.StartFinding();
            }


            //if()
        }

        public override void OnExit()
        {
            //base.OnExit();
            switch (moveType)
            {
                case MovementTypes.MOVE_WALK:
                    animator.SetBool("isWalking", false);
                    break;
                case MovementTypes.MOVE_RUN:
                    animator.SetBool("isRunning", false);
                    break;
                default:
                    break;
            }
            if (aiController.enablePathfinding)
            {
                unit.StopFinding();
            }

        }

        public override void OnUpdate()
        {
            
            if(!aiController.enablePathfinding && aiController.target != null)
            {
                Move(aiController.target.transform);
            }
            
        }
        private void Finish()
        {
            unit.StopFinding();
            if (!string.IsNullOrEmpty(finishEvent))
            {
                GetOwner().SendEvent(finishEvent);
            }

        }
        
        private void Move(Transform Target)
        {
            //
            //Method Name : void Move()
            //Purpose     : This method moves the character to where the target position is. In most casess, the player position.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            if (Target != null)
            {
                RaycastHit hit;
                if (aiController.enableOA)
                {
                    
                    float distance = Vector3.Distance(transform.position, Target.transform.position);
                    if (Physics.Raycast(transform.position, Target.position, out hit, distance))
                    {
                        if (hit.transform != transform && hit.transform != Target.transform)
                        {
                            ObstacleAvoidance(Target);
                        }
                        else
                        {
                            FollowTarget(Target);
                        }
                    }
                    else
                    {
                        FollowTarget(Target);
                    }
                    
                }
                else
                {
                    FollowTarget(Target);
                }
                
            }
        }
        private void FollowTarget(Transform Target)
        {
            LookAt(Target.gameObject);
            //float distance = Vector3.Distance(transform.position, target.transform.position);
            transform.position += transform.forward * aiController.MovementSpeed * Time.deltaTime;
            Debug.DrawLine(transform.position, Target.position, Color.green);
        }
        protected void LookAt(GameObject Target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12 * Time.deltaTime);
        }
        protected void LookAt(Vector3 Target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Target - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12 * Time.deltaTime);
        }
        private void ObstacleAvoidance(Transform Target)
        {
            Vector3 dir = (Target.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 20))
            {
                if (hit.transform != transform && hit.transform != Target.transform)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                    dir += hit.normal * 50;
                }
            }

            Vector3 left = transform.position;
            Vector3 right = transform.position;

            left.x -= size;
            right.x += size;
            if (Physics.Raycast(left, transform.forward, out hit, 20))
            {
                if (hit.transform != transform && hit.transform != Target.transform)
                {
                    Debug.DrawLine(left, hit.point, Color.red);
                    dir += hit.normal * 50;

                }
            }

            if (Physics.Raycast(right, transform.forward, out hit, 20))
            {
                if (hit.transform != transform && hit.transform != Target.transform)
                {
                    Debug.DrawLine(right, hit.point, Color.red);
                    dir += hit.normal * 50;
                }
            }
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            transform.position += transform.forward * 5 * Time.deltaTime;
        }
        /*private void ObstacleAvoidance(Transform target)
        {
            Vector3 dir = (target.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 20))
            {
                if (hit.transform != transform)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                    dir += hit.normal * 50;
                }
            }

            Vector3 left = transform.position;
            Vector3 right = transform.position;

            left.x -= 2;
            right.x += 2;
            if (Physics.Raycast(left, transform.forward, out hit, 20))
            {
                if (hit.transform != transform)
                {
                    Debug.DrawLine(left, hit.point, Color.red);
                    dir += hit.normal * 50;

                }
            }

            if (Physics.Raycast(right, transform.forward, out hit, 20))
            {
                if (hit.transform != transform)
                {
                    Debug.DrawLine(right, hit.point, Color.red);
                    dir += hit.normal * 50;
                }
            }
            Quaternion rot = Quaternion.LookRotation(dir);
            //transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime);
            //transform.position += transform.forward * 5 * Time.deltaTime;
            Debug.Log("Whoops");

        }*/
        

    }


}
