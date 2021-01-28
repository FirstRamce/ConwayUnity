using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCell : MonoBehaviour
{

    public int x { get; set; }
    public int y { get; set; }

    
    // Start is called before the first frame update
    void Start()
    {
        if(this.tag == "2d")
        {
            transform.Rotate(Vector2.right * 90);
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
            //rotate and set height to 0.1
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
