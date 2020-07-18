using UnityEngine;
using System.Collections;

public class AutoRecycle : MonoBehaviour
{
    bool bStarted;
    public float delay = 1;
    public bool autoDestroy = true;

    // Use this for initialization
    IEnumerator Start()
    {
        bStarted = true;
        yield return new WaitForSeconds(delay);
        gameObject.Recycle();
    }

    public void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(Start());
    }

    void OnEnable()
    {
        if (bStarted)
            StartCoroutine(Start());
    }

    void OnDisable()
    {
        if (autoDestroy)
            Destroy(this);
    }
}

