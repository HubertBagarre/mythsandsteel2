using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace CallbackManagement
{
    public class CallbackManager : MonoBehaviour
    {
        // Delegates
        public delegate void NoParamEvent();
        public delegate void SingleUnitParamEvent(Unit unit);
        public delegate void DoubleUnitParamEvent(Unit unit1,Unit unit2);
        public delegate void UnitHexParamEvent(Unit unit, Hex hex);
    
        public delegate void PlayerParamEvent(PlayerSM player);
        public static event NoParamEvent OnAnyPlayerTurnStart;
        public static event PlayerParamEvent OnPlayerTurnStart;
        public static event PlayerParamEvent OnPlayerTurnEnd;
        public static event UnitHexParamEvent OnAnyUnitHexExit;
        public static event UnitHexParamEvent OnAnyUnitHexEnter;
        
        public static void InitEvents()
        {
            OnAnyPlayerTurnStart = DoNothing;
            OnPlayerTurnStart = DoNothing;
            OnPlayerTurnEnd = DoNothing;
            OnAnyUnitHexExit = DoNothing;
            OnAnyUnitHexEnter = DoNothing;
        }

        private static void DoNothing()
        {
            Debug.Log($"Doing nothing, no params");
        }
        
        private static void DoNothing(Unit unit)
        {
            Debug.Log($"Doing nothing, param : {unit}");
        }
        
        private static void DoNothing(Unit unit,Hex hex)
        {
            Debug.Log($"Doing nothing, param : {unit}, {hex}");
        }
        
        private static void DoNothing(Unit unit1,Unit unit2)
        {
            Debug.Log($"Doing nothing, param : {unit1}, {unit2}");
        }
        
        private static void DoNothing(PlayerSM player)
        {
            Debug.Log($"Doing nothing, params : {player}");
        }


        #region Invokers

        public static void PlayerTurnStart(PlayerSM player)
        {
            OnPlayerTurnStart?.Invoke(player);
        }
        
        public static void PlayerTurnEnd(PlayerSM player)
        {
            OnPlayerTurnEnd?.Invoke(player);
        }
        
        public static void AnyPlayerTurnStart()
        {
            OnAnyPlayerTurnStart?.Invoke();
        }
        
        public static void UnitHexEnter(Unit unit, Hex hex)
        {
            OnAnyUnitHexEnter?.Invoke(unit, hex);
        }

        public static void UnitHexExit(Unit unit, Hex hex)
        {
            OnAnyUnitHexExit?.Invoke(unit, hex);
        }

        #endregion
    }
}


