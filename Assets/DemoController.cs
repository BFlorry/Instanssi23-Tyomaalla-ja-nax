using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    [SerializeField]
    private GameObject coneObject;

    [SerializeField]
    private GameObject crabObject;

    [SerializeField]
    private GameObject hatObject;

    [SerializeField]
    private GameObject drillObject;

    private List<GameObject> cones;

    private List<GameObject> crabs;

    private float backAndForthMultiplier = 0;
    private float rotationMultiplier = 1;
    [SerializeField]
    private float timeFactor = 0.3f;
    private DemoEffect curEffect;
    private DemoEffect curCrabEffect;

    public enum DemoEffect
    {
        CIRCLE,
        SINWAVE,
        SINCIRCLE
    }

    // Start is called before the first frame update
    void Start()
    {
        cones = new List<GameObject>();
        crabs = new List<GameObject>();
        //cones.Add(Instantiate(coneObject));
    }

    public void CreateNewCone(){
        cones.Add(Instantiate(coneObject));
    }

    public void CreateNewHat(){
        cones.Add(Instantiate(hatObject));
    }

    public void CreateNewCrab(){
        crabs.Add(Instantiate(crabObject));
    }

    public void ChangeDemoEffect(DemoEffect effect, bool forCrabs = false){
        if(forCrabs){ 
            curCrabEffect = effect; 
            return;
            }
        curEffect = effect;
    }

    public void ChangeEffectCircle(){
        ChangeDemoEffect(DemoEffect.CIRCLE);
    }
    public void ChangeEffectSinCircle(){
        ChangeDemoEffect(DemoEffect.SINCIRCLE);
    }
    public void ChangeEffectSinWave(){
        ChangeDemoEffect(DemoEffect.SINWAVE);
    }

    public void ChangeCrabEffectCircle(){
        ChangeDemoEffect(DemoEffect.CIRCLE, true);
    }
    public void ChangeCrabEffectSinCircle(){
        ChangeDemoEffect(DemoEffect.SINCIRCLE, true);
    }
    public void ChangeCrabEffectSinWave(){
        ChangeDemoEffect(DemoEffect.SINWAVE, true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)){
            cones.Add(Instantiate(coneObject));
        }

        if(Input.GetKeyDown(KeyCode.RightShift)){
            backAndForthMultiplier++;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift)){
            rotationMultiplier++;
        }

        if(Input.GetKeyDown(KeyCode.Backspace)){
            int[] ints = {0};
            PulseSize(ints, 3);
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            ExitDemo();
        }

        int coneCount = cones.Count;
        if(coneCount > 0){
            for (int i = 0; i < coneCount; i++)
            {
                float angle = (i + 1) * Mathf.PI * 2f / (coneCount);

                if(cones[i].transform.localScale.magnitude > new Vector3(1, 1, 1).magnitude){
                        cones[i].transform.localScale = Vector3.Lerp(cones[i].transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * 3f);
                }

                if(curEffect == DemoEffect.SINCIRCLE){
                
                    
                    Vector3 newPos = new Vector3(Mathf.Cos(angle * Time.realtimeSinceStartup * timeFactor) * coneCount, Mathf.Sin(angle * Time.realtimeSinceStartup * timeFactor) * coneCount, Mathf.Sin(backAndForthMultiplier * angle)) * 0.6f;
                    cones[i].transform.position = newPos;
                    cones[i].transform.Rotate(new Vector3(Mathf.Cos(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)), Mathf.Sin( angle * Time.realtimeSinceStartup * timeFactor), Mathf.Sin(angle * Time.realtimeSinceStartup * timeFactor)) * rotationMultiplier);

                }
                else if(curEffect == DemoEffect.CIRCLE){
                    Vector3 newPos = new Vector3(Mathf.Cos(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)) * coneCount, Mathf.Sin(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)) * coneCount, Mathf.Sin(backAndForthMultiplier * angle)) * 0.6f;
                    cones[i].transform.position = newPos;
                    //cones[i].transform.Rotate(new Vector3(Mathf.Cos(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)), Mathf.Sin( angle * Time.realtimeSinceStartup * timeFactor), Mathf.Sin(angle * Time.realtimeSinceStartup * timeFactor)) * rotationMultiplier);

                }
                else if(curEffect == DemoEffect.SINWAVE){
                    Vector3 newPos = new Vector3((angle - 4) * 2, Mathf.Sin(angle * Mathf.Sin(Time.realtimeSinceStartup)) * 1.5f, Mathf.Sin(backAndForthMultiplier * angle));
                    cones[i].transform.position = newPos;
                }
            }
                    
        }

        int crabCount = crabs.Count;
        if(crabCount > 0){
            for (int i = 0; i < crabCount; i++)
            {
                    float angle = (i + 1) * Mathf.PI * 2f / crabCount;
                if(crabs[i].transform.localScale.magnitude > new Vector3(1, 1, 1).magnitude){
                        crabs[i].transform.localScale = Vector3.Lerp(crabs[i].transform.localScale, new Vector3(1, 1, 1), Time.deltaTime * 3f);
                }

                if(curEffect == DemoEffect.SINCIRCLE){
                
                    Vector3 newPos = new Vector3(Mathf.Cos(angle * Time.realtimeSinceStartup * timeFactor) * crabCount, Mathf.Sin(angle * Time.realtimeSinceStartup * timeFactor) * crabCount, Mathf.Sin(backAndForthMultiplier * angle)) * 0.8f;
                    crabs[i].transform.position = newPos;
                    crabs[i].transform.Rotate(new Vector3(Mathf.Cos(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)), Mathf.Sin( angle * Time.realtimeSinceStartup * timeFactor), Mathf.Sin(angle * Time.realtimeSinceStartup * timeFactor)) * rotationMultiplier);

                }
                else if(curEffect == DemoEffect.CIRCLE){
                    Vector3 newPos = new Vector3(Mathf.Cos(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)) * crabCount, Mathf.Sin(angle * Mathf.Sin(Time.realtimeSinceStartup * timeFactor)) * crabCount, Mathf.Sin(backAndForthMultiplier * angle)) * 0.8f;
                    crabs[i].transform.position = newPos;

                }
                else if(curEffect == DemoEffect.SINWAVE){
                    Vector3 newPos = new Vector3((angle - 4) * 2, Mathf.Sin(angle * Mathf.Sin(Time.realtimeSinceStartup)) * 1.5f, Mathf.Sin(backAndForthMultiplier * angle));
                    crabs[i].transform.position = newPos;
                }
            }
                    
        }
    }

    public void PulseSize(int[] indexes, float bigsize){
        foreach (int index in indexes)
        {
            cones[index].transform.localScale = new Vector3(bigsize, bigsize, bigsize);
        }
    }

    public void PulseSizeAll(float bigsize){
        List<int> allIndexes = new List<int>();
        for (int i = 0; i < cones.Count; i++)
        {
            allIndexes.Add(i);
        }
        PulseSize(allIndexes.ToArray(), bigsize);
    }

    public void PulseSizeHalf(float bigsize, bool isRightSide){
        List<int> halfIndexes = new List<int>();
        for (int i = 0; i < cones.Count; i++)
        {
            if(!isRightSide && i % 2 == 0){
                halfIndexes.Add(i);
            }
        }
        PulseSize(halfIndexes.ToArray(), bigsize);
    }

    public int GetConeCount(){
        return cones.Count;
    }

    public void ExitDemo(){
        Application.Quit();
    }
}
