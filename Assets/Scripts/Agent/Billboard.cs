using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camTRf;
    // Start is called before the first frame update
    void Start()
    {
        _camTRf = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(_camTRf);
    }
}
