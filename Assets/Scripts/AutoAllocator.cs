﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAllocator : GameField
{
    List<Bounds> spawnAreas = new List<Bounds>(1);
    Dispatcher[] allShips;
    Bounds selectedArea;

    public void OnAutoAllocateButtonClick()
    {
        allShips = Dispatcher.CreateAllShips();
        var firstArea = new Bounds(new Vector3((float)Width() / 2, (float)Height() / 2),
            new Vector3(Width(), Height()));
        spawnAreas.Add(firstArea);
        ClearGameField();

        for (int i = 0; i < 10; i++)
        {
            var area = new Bounds(new Vector3(0, 0),
                new Vector3(Random.Range(0, 4), Random.Range(0, 4)));
            spawnAreas.Add(area);
        }
                

        StartCoroutine(AllocateAllShips());
    }

    void ClearGameField()
    {
        for (int i = 0; i < body.Length; i++) ClearFieldCell(i);
    }

    void ClearFieldCell(int i)
    {
        body[i % Width(), i / Height()] = CellState.Empty;
    }

    IEnumerator AllocateAllShips()
    {
        while (!AreAllShipsInitialized()) yield return null;

        foreach (Ship ship in allShips)
        {            
            //AllocateShip(ship);
        }


        for (int i = 0; i < 10; i++)
        {
            CheckArea((Ship)allShips[i], spawnAreas[i]);
        }
    }

    bool AreAllShipsInitialized()
    {
        foreach (Ship ship in allShips)
            if (ship.FloorsNum() == 0) return false;
        return true;
    }

    void AllocateShip(Ship ship)
    {
        SelectArea(ship);
    }

    void SelectArea(Ship ship)
    {
        var areasWorkingList = CopyList(spawnAreas);
        for (int i = 0; i < body.Length; i++)
        {
            var areaIndex = Random.Range(0, areasWorkingList.Count);
            selectedArea = areasWorkingList[areaIndex];

        }
    }

    bool CheckArea(Ship ship, Bounds area)
    {
        var canStandVertically = ship.FloorsNum() <= area.size.y;
        var canStandHorizontally = ship.FloorsNum() <= area.size.x;

        bool status = true;

        if (!canStandHorizontally && !canStandVertically) status = false; //return false;
        else if (canStandHorizontally && canStandVertically)
            ship.orientation = (Ship.Orientation)Random.Range(0, 2);
        else if (canStandHorizontally) ship.orientation = Ship.Orientation.Horizontal;
        else ship.orientation = Ship.Orientation.Vertical;
        
        Debug.Log($"area size {area.size} for ship len {ship.FloorsNum()} " +
            $"is appropriate = {status} for orientation {ship.orientation}");

        return status;
    }

    List<T> CopyList<T>(List<T> list)
    {
        return new List<T>(list);
    }
}
