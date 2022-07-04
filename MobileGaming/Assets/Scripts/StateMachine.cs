using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected BaseState currentState;

    private void Awake()
    {
        currentState = new BaseState(this);
    }

    void Start()
    {
        currentState = GetInitialState();
        if (currentState != null)
            currentState.Enter();
    }

    void Update()
    {
        UpdateLoop();
        if (currentState != null)
            currentState.UpdateLogic();
    }

    protected virtual void UpdateLoop() { }

    void FixedUpdate()
    {
        if (currentState != null)
            currentState.UpdatePhysics();
    }

    public virtual void ChangeState(BaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }

    /*
    Une state machine est compos�e de deux choses:
    - Un script "SM" en monobehaviour, h�ritant de StateMachine.
    Ce script contiendra aussi les fonctions communes � diff�rentes states, la quasi-totalit� des variables li�s � l'entit� ayant le script, 
    et les r�ferences � d'autres scripts si besoin.
    - Un script pour chaque �tat de la state machine, devant h�riter de BaseState (qui n'est pas en MonoBehaviour !).
    Chaque script d'�tat contient le comportement propre � celui-ci, et c'est aussi sur ces scripts que se feront la transition entre les �tats.

    Pour faire correspondre une state � une state machine il faut mettre le code suivant dans cette state:
    
    NomDuScript sm;
    public NomDuScriptDeState(NomDuScriptDeSM stateMachine) : base(stateMachine)
    {
        //Rend les fonctions et les variables de PlayerSM accessibles
        sm = (NomDuScriptDeSM)stateMachine;
    }

    ex:
    public Player_Hit(PlayerSM stateMachine) : base(stateMachine)
    {
        //Rend les fonctions et les variables de PlayerSM accessibles
        sm = (PlayerSM)stateMachine;
    }


    Et dans le script du SM en question, rajouter les states et les setup dans l'Awake :

    ex:
    [HideInInspector] public Player_Idle idleState;
    [HideInInspector] public Player_Walk walkState;

     void Awake()
    {
        //on set les �tats
        idleState = new Player_Idle(this);
        walkState = new Player_Walk(this);
    }

    Enfin, il faut set l'�tat par d�faut que la StateMachine aura lors de son initialisation :

        protected override BaseState GetInitialState()
    {
        //State jou�e de base lors de l'apparition de l'objet
        return nomDeLaVariableStateVoulue;
    }

    ex :
    protected override BaseState GetInitialState()
    {
        //State jou�e de base lors de l'apparition de l'objet
        return idleState;
    }

    Ici, le script va jouer le code pr�sent dans la state "Idle". Pour passer d'un �tat � un autre, il faut ins�rer le code suivant
    dans l'�tat (dans la void UpdateLogic):

    stateMachine.ChangeState(((NomDuScriptDeSM) stateMachine).nomDeLaVariableStateVoulue);

    ex : Si je mets dans le script d'"Idle" le code suivant, la StateMachine va passer de l'�tat "Idle" � l'�tat "Walk" si la variable "movement"
    pas nulle (= le perso se d�place).
    if(sm.movement != Vector2.zero)
    {
    stateMachine.ChangeState(((PlayerSM) stateMachine).walkState);
    }




    */

    /*
    private void OnGUI()
    {
        string content = currentState != null ? currentState.name : "(no current state)";
        GUILayout.Label($"<color='orange'><size=40>{content}</size></color>");
    }
    */
}