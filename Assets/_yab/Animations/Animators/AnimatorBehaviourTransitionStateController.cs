using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
public class AnimatorBehaviourTransitionStateController : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
       animator.SetBool(Defaults.AnimatorParameterInTransition, true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
       animator.SetBool(Defaults.AnimatorParameterInTransition, false);
    }
}
