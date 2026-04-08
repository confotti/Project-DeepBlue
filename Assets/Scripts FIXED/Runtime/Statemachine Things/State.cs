using UnityEngine;

public abstract class State<T1> where T1 : MonoBehaviour
{
    protected T1 obj;
    protected StateMachine<T1> stateMachine;

    /* Animation things I dont think will be necessary, but might become relevant in the future. 
    protected float startingTime;
    protected bool isAnimationFinished;
    protected readonly string animBoolName;
    */

    public void Init(T1 obj, StateMachine<T1> stateMachine)
    {
        this.obj = obj;
        this.stateMachine = stateMachine;
        //this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        /*
        startingTime = Time.time;
        obj.Anim.SetBool(animBoolName, true);
        isAnimationFinished = false;
        */

    }
    
    public virtual void Exit()
    {
        //obj.Anim.SetBool(animBoolName, false);
    }
    
    public virtual void LogicUpdate()
    {
        
    }

    public virtual void PhysicsUpdate()
    {
        
    }

    //public void AnimationFinishTrigger() => isAnimationFinished = true;
}
