using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAllocator : GameField
{
    Dispatcher[] allShips;

    public void OnAutoAllocateButtonClick()
    {
        allShips = Dispatcher.CreateAllShips();
        foreach (Ship item in allShips)
        {
            Debug.Log(item.FloorsNum());
        }
    }
}
