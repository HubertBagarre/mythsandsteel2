using System;

[Serializable]
public class BaseUnitBuff
{
    public Unit assignedUnit;
    
    public int buffInfoId;
    
    public void AddBuff()
    {
        assignedUnit.currentBuffs.Add(this);
        
        OnBuffAdded(assignedUnit);
    }
    
    protected virtual void OnBuffAdded(Unit unit) { }


    public void RemoveBuff()
    {
        if (assignedUnit.currentBuffs.Contains(this)) assignedUnit.currentBuffs.Remove(this);
        
        OnBuffRemoved(assignedUnit);
    }
    
    protected virtual void OnBuffRemoved(Unit unit) { }
}
