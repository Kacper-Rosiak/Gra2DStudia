using UnityEngine;
using UnityEngine.UI;
public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    // Wykonuje się za każdym razem, gdy obiekt menu zostaje włączony (np. klawiszem ESC)
    void OnEnable()
    {
        ActivateTab(0);
    }

    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;

        }
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}
