using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Rigidbody2D>().MovePosition(new Vector2(this.transform.position.x + (Mathf.Sin(Time.time) *.2f), this.transform.position.y));
    }
}
