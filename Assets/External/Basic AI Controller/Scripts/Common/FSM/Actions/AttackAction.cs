using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ViridaxGameStudios.AI
{
    public class AttackAction : FSMAction
    {
        float m_DamageAngle;
        float m_AttackRange;
        float m_Damage;
        Animator animator;
        GameObject Target;
        Transform transform;
        string finishEvent;
        public AttackAction(FSMState owner, AI.AIController aiController) : base(owner, aiController)
        {

        }
        public void Init(Animator animator, string finishEvent = null)
        {
            this.animator = animator;
            this.m_DamageAngle = m_DamageAngle;
            this.m_AttackRange = m_AttackRange;
            this.m_Damage = m_Damage;
            this.Target = Target;
            this.transform = transform;
            this.finishEvent = finishEvent;
        }
        public override void OnEnter()
        {
            animator.SetBool("isAttacking", true);
        }

        public override void OnExit()
        {
            animator.SetBool("isAttacking", false);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        
    }
}

