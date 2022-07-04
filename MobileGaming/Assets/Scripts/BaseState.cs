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


    //Virtual : Pose les m�thodes, peuvent �tre chang�es plus tard sur d'autres scripts

    //Se joue lors du d�but de la state, avant la premi�re updatelogic/physic
    public virtual void Enter() { }
    //Pour l'update
    public virtual void UpdateLogic() { }
    //Pour la fixed update
    public virtual void UpdatePhysics() { }
    //Se joue avant de passer � une autre state
    public virtual void Exit() { }


}