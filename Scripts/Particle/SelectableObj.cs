using UnityEngine;

public class SelectableObj : MonoBehaviour
{
    [SerializeField] private GameObject visualIndicator;

    private bool isSelected = false;

    public int indexInArray;

    public void DefineSelection(bool status)
    {
        isSelected = status;

        if (visualIndicator != null)
        {
            visualIndicator.SetActive(status);
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}
