using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public bool NorthIsWall { get; set; }
    public bool SouthIsWall { get; set; }
    public bool EastIsWall { get; set; }
    public bool WestIsWall { get; set; }
    public float ManhattanDistanceToPlayer { get; set; }
    public int PositionToPlayer { get; set; }
    public int stateId { get; set; }

    public State(bool nIsWall, bool sIsWall, bool eIsWall, bool wIsWall, float manhattan, int position, int stateId) 
    {
        NorthIsWall = nIsWall;
        SouthIsWall = sIsWall;
        EastIsWall = eIsWall;
        WestIsWall = wIsWall;
        ManhattanDistanceToPlayer = manhattan;
        PositionToPlayer = position;
        this.stateId = stateId;
    }
}
