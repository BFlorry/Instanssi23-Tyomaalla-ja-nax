using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeLightController : MonoBehaviour
{

    [SerializeField]
    private GameObject[] lightObjects;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject light in lightObjects)
        {
            light.TryGetComponent<Image>(out Image image);
            if (image != null){
                image.color = new Color(0, 0, 0, 1);
            }
        }
    }

    public void ChangeLight(int lightIndex, Color32 color){
        lightObjects[lightIndex].TryGetComponent<Image>(out Image image);
        if (image != null){
            image.color = color;
        }
    }
}
