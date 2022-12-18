using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class selectableState : MonoBehaviour , ISelectHandler,IDeselectHandler
{
    public bool isSelected = false;
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }
    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(deselect(0.2f));
    }

    

IEnumerator deselect(float delay)
{
    yield return new WaitForSeconds(delay);
    isSelected = false;
}
private void Start()
    {
        
    }
}
