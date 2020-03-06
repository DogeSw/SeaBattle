using System.Collections;
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

        //for (int i = 0; i < 10; i++)
        //{
        //    var area = new Bounds(new Vector3(0, 0),
        //        new Vector3(Random.Range(0, 4), Random.Range(0, 4)));
        //    spawnAreas.Add(area);
        //}
                

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


        //for (int i = 0; i < 10; i++)
        //{
        //    var area = spawnAreas[i];
        //    CheckAndAdjustAreaAndOrient((Ship)allShips[i], ref area);
        //    Debug.Log(FormatBounds(area));
        //}
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
        var x = Random.Range((int)selectedArea.min.x, (int)selectedArea.max.x);
        var y = Random.Range((int)selectedArea.min.y, (int)selectedArea.max.y);
        ship.cellCenterPos = boundsOfCells[x, y].center;
        MarkupArea(ship);

    }

    void SelectArea(Ship ship)
    {
        var areasWorkingList = CopyList(spawnAreas);
        while (true)
        {
            var areaIndex = Random.Range(0, areasWorkingList.Count);
            selectedArea = areasWorkingList[areaIndex];
            if (!CheckAndAdjustAreaAndOrient(ship, ref selectedArea))
                areasWorkingList.Remove(selectedArea);
            else break;
        }
    }

    bool CheckAndAdjustAreaAndOrient(Ship ship, ref Bounds area)
    {
        var canStandVertically = ship.FloorsNum() <= area.size.y;
        var canStandHorizontally = ship.FloorsNum() <= area.size.x;
        float adjSize = ship.FloorsNum() - 1;
        
        if (!canStandHorizontally && !canStandVertically) return false;
        else if (canStandHorizontally && canStandVertically)
            ship.orientation = (Ship.Orientation)Random.Range(0, 2);
        else if (canStandHorizontally) ship.orientation = Ship.Orientation.Horizontal;
        else ship.orientation = Ship.Orientation.Vertical;

        Debug.Log($"initial area {FormatBounds(area)}");

        if (ship.orientation == Ship.Orientation.Horizontal)
        {
            area.Expand(new Vector3(-adjSize, 0));
            area.center = new Vector3(area.center.x - adjSize / 2, area.center.y);
        }
        else
        {
            area.Expand(new Vector3(0, -adjSize));
            area.center = new Vector3(area.center.x, area.center.y + adjSize / 2);
        }

        
        Debug.Log($"new area {FormatBounds(area)} for ship len {ship.FloorsNum()} " +
            $"for orientation {ship.orientation}");

        return true;
    }

    List<T> CopyList<T>(List<T> list)
    {
        return new List<T>(list);
    }

    void MarkupArea(Ship ship)
    {

    }

    string FormatBounds(Bounds bounds)
    {
        return $"{bounds.min} {bounds.max}";
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var area in spawnAreas)
        {
            Gizmos.DrawWireCube((Vector3)originBottomLeft + area.center, 
                area.size * cellSize);
        }
    }
}
