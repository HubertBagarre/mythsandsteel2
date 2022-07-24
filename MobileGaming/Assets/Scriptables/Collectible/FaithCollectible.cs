using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Collectibles/Faith Collectible")]
public class FaithCollectible : ScriptableCollectible
{
    public int faithGained;
    
    public override void OnPickedUp(Unit unitThatPickedUp, Hex hexWithThisCollectible)
    {
        unitThatPickedUp.player.faith += faithGained;
    }
}
