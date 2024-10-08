using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum MoveType
{
    Circle,
    Square
}
public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private GameObject rectangleDraw;
    private List<ISelected> soldiers = new List<ISelected>();
    private Vector2 mousePosBeginHold;
    private Vector2 rectangleDrawPosition;
    private Vector2 leftBottomPos;
    private Vector2 rightTopPos;
    private Vector2 sizeScale;
    private RaycastHit2D[] hits;
    [SerializeField] private float spaceBetweenSoldier = 1.1f;
    [SerializeField] private int countSliderIncreaseCircle = 6;
    [SerializeField] private bool canRandomPosition;
    [SerializeField] private float spaceRandomPosition = 0.1f;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetActiveSelectedArea(false);
        InputManager.Instance.SubEventInput(EventInputCategory.MouseDownLeft, () => SetMouseBeginHold());
        InputManager.Instance.SubEventInput(EventInputCategory.MouseHoldLeft, () => { DrawSelectArea(mousePosBeginHold, InputManager.Instance.mousePoistion); });
        InputManager.Instance.SubEventInput(EventInputCategory.MouseUpLeft, () => { SelectAllSoildierOnAreaSelected(); });
        InputManager.Instance.SubEventInput(EventInputCategory.MouseDownRight, () => { SwitchMoveType(); });
    }
    private void SetMouseBeginHold()
    {
        mousePosBeginHold = InputManager.Instance.mousePoistion;
    }
    public void DrawSelectArea(Vector2 mousePosBegin, Vector2 mousePosEnd)
    {
        SetActiveSelectedArea(true);
        leftBottomPos.x = Math.Min(mousePosBegin.x, mousePosEnd.x);
        leftBottomPos.y = Math.Min(mousePosBegin.y, mousePosEnd.y);
        rightTopPos.x = Math.Max(mousePosBegin.x, mousePosEnd.x);
        rightTopPos.y = Math.Max(mousePosBegin.y, mousePosEnd.y);
        sizeScale = rightTopPos - leftBottomPos;
        rectangleDraw.transform.localScale = sizeScale;
        rectangleDrawPosition.x = leftBottomPos.x + sizeScale.x / 2;
        rectangleDrawPosition.y = leftBottomPos.y + sizeScale.y / 2;
        rectangleDraw.transform.position = rectangleDrawPosition;
    }

    private Vector2 newPosSoldier;
    private Vector2 randomDirectionPos;
    private int countSoldierCurrent;
    private int index;
    private int indexCurrentCircle;
    private int radiusCircle;
    private MoveType moveType = MoveType.Circle;
    WaitForSeconds timeDelay = new WaitForSeconds(0.3f);
    public IEnumerator ArrangeSquadToCircle()
    {
        if (soldiers.Count <= 0) yield break;
        Vector2 posMouse = InputManager.Instance.mousePoistion;
        soldiers[0].ActionWhenSelected(posMouse);
        countSoldierCurrent = 0;
        index = 1;
        while (index < soldiers.Count)
        {
            if (index > countSoldierCurrent) countSoldierCurrent += countSliderIncreaseCircle;
            radiusCircle = countSoldierCurrent / countSliderIncreaseCircle;
            for (indexCurrentCircle = 0; indexCurrentCircle < countSoldierCurrent; indexCurrentCircle++, index++)
            {
                if (index == soldiers.Count) yield break;
                newPosSoldier = Quaternion.Euler(0, 0, 360 / countSoldierCurrent * (indexCurrentCircle + 1)) 
                    * Vector2.right * radiusCircle * spaceBetweenSoldier;
                newPosSoldier = posMouse + newPosSoldier;
                if (canRandomPosition) RandomPosition(); else randomDirectionPos = Vector2.zero;
                soldiers[index].ActionWhenSelected(newPosSoldier + randomDirectionPos);
                yield return timeDelay;
            }
        }
    }
    private int edge;
    public IEnumerator ArrangeSquadToSquare()
    {
        if (soldiers.Count <= 0) yield break;
        Vector2 posMouse = InputManager.Instance.mousePoistion;
        edge = (int)Math.Ceiling(Math.Sqrt(soldiers.Count));
        index = 0;
        for (int h = 0; h < edge; h++)
            for (int w = 0; w < edge; w++, index++)
            {
                if (index == soldiers.Count) yield break;
                newPosSoldier.x = w * spaceBetweenSoldier;
                newPosSoldier.y = h * spaceBetweenSoldier;
                newPosSoldier = posMouse - newPosSoldier + Vector2.one * edge * 0.65f / 2;
                if (canRandomPosition) RandomPosition(); else randomDirectionPos = Vector2.zero;
                soldiers[index].ActionWhenSelected(newPosSoldier + randomDirectionPos);
                yield return timeDelay;
            }      
    }
    private void RandomPosition()
    {
        randomDirectionPos.x = Random.Range(-spaceRandomPosition, spaceRandomPosition);
        randomDirectionPos.y = Random.Range(-spaceRandomPosition, spaceRandomPosition);
    }
    public void SwitchMoveType()
    {
        if (soldiers.Count <= 0) return;
        if (moveType == MoveType.Circle)
        {
            StopAllCoroutines();
            StartCoroutine(ArrangeSquadToCircle());
            moveType = MoveType.Square;
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ArrangeSquadToSquare());
            moveType = MoveType.Circle;
        }
    }
    public void RemoveAllSelectedList()
    {
        foreach (var selected in soldiers) selected.UnSelected();
        soldiers.Clear();
    }
    public void SelectAllSoildierOnAreaSelected()
    {
        RemoveAllSelectedList();
        hits = Physics2D.BoxCastAll(rectangleDraw.transform.position, sizeScale, 0, Vector3.forward);
        if (hits.Length == 0 && Vector2.Distance(mousePosBeginHold, InputManager.Instance.mousePoistion) < 0.005f)
        {
            hits = Physics2D.RaycastAll(InputManager.Instance.mousePoistion, Vector3.forward);
            foreach (var hit in hits)
            {
                if (hit.collider.transform.TryGetComponent<ISelected>(out ISelected selected))
                {
                    soldiers.Add(selected);
                    selected.Selected();
                    SetActiveSelectedArea(false);
                    return;
                }
            }
        }
        AddHitsISelected();
    }
    private void AddHitsISelected()
    {
        foreach (var hit in hits)
        {
            if (hit.collider.transform.TryGetComponent<ISelected>(out ISelected selected))
            {
                soldiers.Add(selected);
                selected.Selected();
            }
        }
        SetActiveSelectedArea(false);
    }
    public void SetActiveSelectedArea(bool status)
    {
        rectangleDraw.SetActive(status);
    }
}
