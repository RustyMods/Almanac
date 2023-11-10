using UnityEngine;
using UnityEngine.EventSystems;

namespace Almanac.MonoBehaviors;

public class ElementHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private void start()
    {
        Transform? image = this.transform.Find("hoverTextElement");
        image.gameObject.SetActive(false);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Transform? image = this.transform.Find("hoverTextElement");
        image.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Transform? image = this.transform.Find("hoverTextElement");
        image.gameObject.SetActive(false);
    }
}