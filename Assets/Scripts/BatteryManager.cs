using UnityEngine;

public class BatteryManager : MonoBehaviour
{
    [Header("Batterijpercentage images:")]
    [SerializeField]private Texture2D image0, image1, image2, image3, image4; //0= leeg/0% 1 = 100-75%, 2 = 75-50%, 3 = 50-25, 4 =25,0

    private Material mat;
    public BusController busController;
    public Material targetMaterial;
    void Start()
    {
    }

    public void Update()
    {
        if (busController.batteryPercentage <= 0)
            targetMaterial.mainTexture = image0;
        else if (busController.batteryPercentage < 25f)
            targetMaterial.mainTexture = image1;
        else if (busController.batteryPercentage < 50f)
            targetMaterial.mainTexture = image2;
        else if (busController.batteryPercentage < 75f)
            targetMaterial.mainTexture = image3;
        else
            targetMaterial.mainTexture = image4;
    }

}