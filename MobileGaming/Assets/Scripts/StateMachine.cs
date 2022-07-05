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
    Une state machine est composée de deux choses:
    - Un script "SM" en monobehaviour, héritant de StateMachine.
    Ce script contiendra aussi les fonctions communes à différentes states, la quasi-totalité des variables liés à l'entité ayant le script, 
    et les réferences à d'autres scripts si besoin.
    - Un script pour chaque état de la state machine, devant hériter de BaseState (qui n'est pas en MonoBehaviour !).
    Chaque script d'état contient le comportement propre à celui-ci, et c'est aussi sur ces scripts que se feront la transition entre les états.

    Pour faire correspondre une state à une state machine il faut mettre le code suivant dans cette state:
    
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
        //on set les états
        idleState = new Player_Idle(this);
        walkState = new Player_Walk(this);
    }

    Enfin, il faut set l'état par défaut que la StateMachine aura lors de son initialisation :

        protected override BaseState GetInitialState()
    {
        //State jouée de base lors de l'apparition de l'objet
        return nomDeLaVariableStateVoulue;
    }

    ex :
    protected override BaseState GetInitialState()
    {
        //State jouée de base lors de l'apparition de l'objet
        return idleState;
    }

    Ici, le script va jouer le code présent dans la state "Idle". Pour passer d'un état à un autre, il faut insérer le code suivant
    dans l'état (dans la void UpdateLogic):

    stateMachine.ChangeState(((NomDuScriptDeSM) stateMachine).nomDeLaVariableStateVoulue);

    ex : Si je mets dans le script d'"Idle" le code suivant, la StateMachine va passer de l'état "Idle" à l'état "Walk" si la variable "movement"
    pas nulle (= le perso se déplace).
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