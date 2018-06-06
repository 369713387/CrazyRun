using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimation : StateMachineBehaviour
{
    public string parameter;
    public int count;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        animator.SetInteger(parameter, Random.Range(0, count));
    }

}
