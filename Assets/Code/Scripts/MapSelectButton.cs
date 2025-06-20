using UnityEngine;
using UnityEngine.EventSystems;

public class MapSelectButton : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [Tooltip("Bu butonun temsil ettiği haritanın listedeki indeksi (0, 1, 2...)")]
    public int mapIndex;
    
    private MapSelectManager manager;

    void Start()
    {
        // Sahnedeki MapSelectManager'ı otomatik olarak bul
        manager = FindObjectOfType<MapSelectManager>();
    }

    // Bu buton kontrolcü/klavye ile seçildiğinde otomatik olarak çalışır
    public void OnSelect(BaseEventData eventData)
    {
        if (manager != null)
        {
           // manager.OnMapSelected(mapIndex);
        }
    }
    
    // Bu butona onay (Submit) komutu geldiğinde otomatik olarak çalışır
    public void OnSubmit(BaseEventData eventData)
    {
        if (manager != null)
        {
           // manager.OnMapSubmitted();
        }
    }
}