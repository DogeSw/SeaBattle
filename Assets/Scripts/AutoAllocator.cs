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

        


        //for (int i = 0; i < 20; i++)
        //{
        //    var area = new Bounds(new Vector3(Random.Range(0,8), Random.Range(0,8)),
        //        new Vector3(Random.Range(1, 4), Random.Range(1, 4)));
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

            AllocateShip(ship);
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
        var x = Random.Range((int)selectedArea.min.x, (int)selectedArea.max.x);
        var y = Random.Range((int)selectedArea.min.y, (int)selectedArea.max.y);
        ship.cellCenterPos = boundsOfCells[x, y].center;
        ship.transform.position = ship.cellCenterPos;
        MarkupArea(ship, x,y);

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

    void MarkupArea(Ship ship, int x, int y)
    {
        var occupitedArea = GetOccupitedArea(ship, x, y);
        var initialSpawnAreas = CopyList(spawnAreas);

        foreach (var initialSpawnArea in initialSpawnAreas)
        {

        }


    }

    bool BoundsOverlap(Bounds initial, Bounds occupied)
    {
        if (initial == selectedArea) return true;
        var miniMaxX = Mathf.Min(initial.max.x, occupied.max.x);
        var maxMinX = Mathf.Min(initial.min.x, occupied.min.x);
        var miniMaxY = Mathf.Min(initial.max.y, occupied.max.y);
        var maxMinY = Mathf.Min(initial.min.y, occupied.min.y);
        return miniMaxX > maxMinX && miniMaxY >  maxMinY;
    }

    void SplitArea(Bounds initial, Bounds occupied)
    {
        CreateSubarea(initial, initial.max.y, occupied.max.y, false);
        CreateSubarea(initial, initial.max.x, occupied.max.x, true);
        CreateSubarea(initial, occupied.min.y, initial.min.y, false);
        CreateSubarea(initial, occupied.min.x, initial.min.x, true);
    }
    void CreateSubarea(Bounds area, float max, float min, bool isVertical)
    {
        if (max - min < 1)
        {
            return;
        }
        var subarea = new Bounds();
        if (isVertical)
        {
            subarea.center = new Vector3((max + min) / 2, area.center.y);
            subarea.size = new Vector3(max - min, area.size.y);
        }
        else
        {
            subarea.center = new Vector3(area.center.x, (max + min) / 2);
            subarea.size = new Vector3(area.size.x, max - min);
        }
        spawnAreas.Add(subarea);
        Debug.Log($"area created {FormatBounds(subarea)}");
    }

    Bounds GetOccupitedArea(Ship ship, int x, int y)
    {
        float shipExtension = ship.FloorsNum() / 2;
        float centerX = x + shipExtension, centerY = y + 0.5f;
        float areaWidth = ship.FloorsNum() + 2, areaHeigth = 3;

        if (ship.orientation == Ship.Orientation.Vertical)
        {
            areaHeigth = areaWidth;
            areaWidth = 3;
            centerX = x + 0.5f;
            centerY = y + 1 - shipExtension;


        }
         var result = new Bounds(new Vector3(centerX, centerY), new Vector3(areaWidth, areaHeigth));
        return result;
    }

    string FormatBounds(Bounds bounds)
    {
        return $"{bounds.min} {bounds.max - new Vector3(1, 1)}";
    }

    void OnDrawGizmos()
    {
        //var colors = new Color[]
        //{
        //    Color.black, Color.blue, Color.cyan, Color.gray, Color.green
        //};
        //int c = 0;
        
        //foreach (var area in spawnAreas)
        //{
        //    if (c == colors.Length) c = 0;
        //    Gizmos.color = colors[c];
        //    Gizmos.DrawWireCube((Vector3)originBottomLeft + area.center - new Vector3(2.5f, 2.5f) * cellSize, 
        //        area.size * cellSize);
        //    c++;
        //}
    }
}
