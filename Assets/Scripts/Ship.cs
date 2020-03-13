using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : Dispatcher
{
    public enum Orientation
    {
        Horizontal, Vertical
    }

    public Orientation orientation = Orientation.Horizontal;
    public GameObject floorButtonPref;
    public Vector2 cellCenterPos;
    public bool isWithinCell = false, isPositionCorrect;


    Vector2 lastPosition;
    Canvas canvas;
    Orientation lastOrientation;
    Animator[] animators;
    bool toMove = false, WasLocatedOnse = false;
    float rotationAngle = -90;
    int floorsNum;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        canvas = GetComponentInParent<Canvas>();
        floorsNum = transform.childCount;
        animators = new Animator[floorsNum];
        if (orientation == Orientation.Vertical) rotationAngle = -rotationAngle;
        lastOrientation = orientation;

        float floorSize = 0;
        for (int i = 0; i < floorsNum; i++)
        {
            var floor = transform.GetChild(i);
            var floorPos = transform.position;
            floorSize = floor.GetComponent<SpriteRenderer>().bounds.size.x;
            if (orientation == Orientation.Horizontal) floorPos.x += i * floorSize;
            else if (orientation == Orientation.Vertical) floorPos.y -= i * floorSize;
            floor.transform.position = floorPos; // Поле игры

            var floorButtonObj = Instantiate(floorButtonPref, floor.transform); //Префабы кнопок
            floorButtonObj.transform.position = floorPos; // Кнопки для кораблей
            var buttonRectTransf = floorButtonObj.GetComponent<RectTransform>(); // Задание кнопок по флуру
            buttonRectTransf.sizeDelta = new Vector2(floorSize, floorSize); // Размер кнопок
            var buttonScript = floorButtonObj.GetComponent<Button>(); // Задавание кликабельности
            buttonScript.onClick.AddListener(OnFloorClick); // Кликабельность и привязываемость.

            var animator = floor.GetComponent<Animator>();
            animators[i] = animator;
        }
    }
    void SwitchErrorAnimation()
    {
        foreach (var animator in animators)
        {
            animator.SetBool("IsMistPlayst", !isPositionCorrect);
        }
    }

    // Update is called once per frame
    void Update()
    {
        toMove = Equals(currentShip);
        if (!toMove) return;
        var mousePos = Input.mousePosition; // позиция мыши
        var CanvasRec = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRec, mousePos, Camera.main, out Vector2 transletedMousePos); // Конвертация координат по канвасу с учётом камеры
        transletedMousePos = canvas.transform.TransformPoint(transletedMousePos); // Выше написанное
        transform.position = transletedMousePos;// позиция мыши
        GameField.CheckShipPosition(transletedMousePos, this);// Чекнуть корабельную позицию через Daun и это
        if (isWithinCell)
        {
            transform.position = cellCenterPos;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Rotate();
        }
        else if (Input.GetKeyUp(KeyCode.Escape))
        {

            if (WasLocatedOnse)
            {
                transform.position = lastPosition;
                currentShip = null;
                if (orientation != lastOrientation)
                {
                    Rotate();
                }
                GameField.RegisterShip(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        SwitchErrorAnimation();
    }

    void Rotate()
    {
        if (orientation == Orientation.Horizontal) orientation = Orientation.Vertical;
        else orientation = Orientation.Horizontal;
        transform.Rotate(new Vector3(0, 0, rotationAngle), Space.Self);
        rotationAngle = -rotationAngle;
    }
    void RememberPositionOrientation()
    {
        lastPosition = transform.position;
        lastOrientation = orientation;
    }

    void OnFloorClick()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        else if (toMove && isPositionCorrect)
        {
            RememberPositionOrientation();
            GameField.RegisterShip(this);
        }
        OnShipClick();
        if (isPositionCorrect)
        {
            WasLocatedOnse = true;
        }

    }
    public void AutoAllign()
    {
        transform.position = cellCenterPos;
        if (orientation != lastOrientation)
        {
            orientation = lastOrientation;
            Rotate();
        }
        RememberPositionOrientation();
        isPositionCorrect = isWithinCell = true;
    }
    public int FloorsNum()
    {
        return floorsNum;
    }

    public bool WAsLocatedOnse()
    {
        return WasLocatedOnse;
    }
    public override string ToString()
    {
        var result = base.ToString();
    }
}
