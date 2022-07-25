using UnityEngine;

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
        public delegate void UnitTakeDamageD(Unit targetUnit, sbyte physicalDamage, sbyte magicalDamage, Unit sourceUnit);
        public delegate void UnitKilledEvent(Unit killedUnit,bool physicalDeath,bool magicalDeath,Unit killer);
        
        public static event NoParamEvent OnAnyPlayerTurnStart;
        public static event PlayerParamEvent OnPlayerTurnStart;
        public static event PlayerParamEvent OnPlayerTurnEnd;
        public static event SingleUnitParamEvent OnUnitRespawned;
        public static event DoubleUnitParamEvent OnUnitAttack;
        public static event UnitHexParamEvent OnAnyUnitHexExit;
        public static event UnitHexParamEvent OnAnyUnitHexEnter;
        public static event UnitTakeDamageD OnUnitTakeDamage;
        public static event UnitKilledEvent OnUnitKilled;
        
        public static void InitEvents()
        {
            OnAnyPlayerTurnStart = DoNothing;
            
            OnPlayerTurnStart = DoNothing;
            
            OnPlayerTurnEnd = DoNothing;
            
            OnUnitRespawned = DoNothing;
            
            OnUnitAttack = DoNothing;
            
            OnAnyUnitHexExit = DoNothing;
            
            OnAnyUnitHexEnter = DoNothing;
            
            OnUnitTakeDamage = DoNothing;
            
            OnUnitKilled = DoNothing;
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
        
        private static void DoNothing(Unit targetUnit, sbyte physicalDamage, sbyte magicalDamage, Unit sourceUnit)
        {
            Debug.Log($"Doing nothing, params : {targetUnit}, {physicalDamage}, {magicalDamage}, {sourceUnit}");
        }
        
        private static void DoNothing(Unit killedUnit,bool physicalDeath,bool magicalDeath,Unit killer)
        {
            Debug.Log($"Doing nothing, params : {killedUnit}, {physicalDeath}, {magicalDeath}, {killer}");
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
        
        public static void UnitRespawned(Unit unit)
        {
            OnUnitRespawned?.Invoke(unit);
        }
        
        public static void UnitAttack(Unit unit1, Unit unit2)
        {
            OnUnitAttack?.Invoke(unit1, unit2);
        }
        
        public static void UnitHexEnter(Unit unit, Hex hex)
        {
            OnAnyUnitHexEnter?.Invoke(unit, hex);
        }

        public static void UnitHexExit(Unit unit, Hex hex)
        {
            OnAnyUnitHexExit?.Invoke(unit, hex);
        }
        
        public static void UnitTakeDamage(Unit targetunit, sbyte physicaldamage, sbyte magicaldamage, Unit sourceunit)
        {
            OnUnitTakeDamage?.Invoke(targetunit, physicaldamage, magicaldamage, sourceunit);
        }

        public static void UnitKilled(Unit killedunit, bool physicaldeath, bool magicaldeath, Unit killer)
        {
            OnUnitKilled?.Invoke(killedunit, physicaldeath, magicaldeath, killer);
        }

        #endregion
    }
}


