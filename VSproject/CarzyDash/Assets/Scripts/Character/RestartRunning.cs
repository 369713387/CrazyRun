using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartRunning : StateMachineBehaviour {

    static int s_DeadHash = Animator.StringToHash("Dead");

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //如果转换为死亡状态后，不能重新开始
        if (animator.GetBool(s_DeadHash))
            return;

        TrackManager.instance.StartMove();
    }
}
