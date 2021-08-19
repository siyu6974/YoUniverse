using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    // Start is called before the first frame update
    private int counter = 0;
    private Vector3 orgScale;
    void Start()
    {
        orgScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (counter < 100) {
            counter++;
            transform.localScale = orgScale * counter / 100;
        }
    }
}
