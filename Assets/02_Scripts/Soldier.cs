using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface ISelected
{
    public void Selected();
    public void ActionWhenSelected(Vector3 pos);
    public void UnSelected();
}
public class Soldier : MonoBehaviour,ISelected
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject selectedBackground;
    public void Selected()
    {
        selectedBackground.SetActive(true);
    }
    public void UnSelected()
    {
        selectedBackground.SetActive(false);
    }
    public void ActionWhenSelected(Vector3 pos)
    {
        StartMoveToPos(pos);
    }
    private void Start()
    {
        selectedBackground.SetActive(false);
    }
    private void MoveToPos(Vector3 newPos)
    {
        transform.position = Vector3.MoveTowards(transform.position, newPos, moveSpeed*Time.deltaTime);
    }
    private IEnumerator IEMove(Vector3 newPos)
    {
        while(transform.position != newPos)
        {
            MoveToPos(newPos);
            yield return null;
        }
    }
    public void StartMoveToPos(Vector3 newPos)
    {
        StopAllCoroutines();
        StartCoroutine(IEMove(newPos));
    }
}

