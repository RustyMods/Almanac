using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AlmanacUnityLib;

public class GridView : MonoBehaviour
{
    protected RectTransform root;
    public GridLayoutGroup grid;
    public Scrollbar scrollbar;
    
    protected bool handleArrowKeys = true;
    private float lastInputTime;
    
    public float GetAvailableWidth() => root.rect.width - grid.padding.left + grid.padding.right;
    public int GetColumns() => Mathf.Max(1, Mathf.FloorToInt((GetAvailableWidth() + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x)));

    public virtual List<GridElement> GetElements() => new();

    public virtual GridElement? GetSelectedElement() => null;
    
    public virtual void Reset()
    {
        root = transform.Find("Viewport/ListRoot").GetComponent<RectTransform>();
        grid = root.GetComponent<GridLayoutGroup>();
        scrollbar = transform.Find("Scrollbar").GetComponent<Scrollbar>();
    }


}

public class GridElement : MonoBehaviour
{
    public virtual void Select() {}

    public virtual bool IsKnown() => false;

    public bool IsHidden() => !gameObject.activeInHierarchy;
        
    public void SetActive(bool enable) => gameObject.SetActive(enable);
        
    public void Destroy() => Destroy(gameObject);
}