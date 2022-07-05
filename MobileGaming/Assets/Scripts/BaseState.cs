using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{

    protected StateMachine stateMachine;

    public BaseState(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }


    //Virtual : Pose les méthodes, peuvent être changées plus tard sur d'autres scripts

    //Se joue lors du début de la state, avant la première updatelogic/physic
    public virtual void Enter() { }
    //Pour l'update
    public virtual void UpdateLogic() { }
    //Pour la fixed update
    public virtual void UpdatePhysics() { }
    //Se joue avant de passer à une autre state
    public virtual void Exit() { }


}