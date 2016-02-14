using UnityEngine;
using System.Collections;

public class TestUpdate : MonoBehaviour
{
    public bool useFixed;
    public float speed;
    
    public float speed2 = 8f;
    
    public float sign = 1;
    
    [SerializeField]
    float mathSign;
    
    Rigidbody2D rg2d;
    void Awake()
    {
        rg2d = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        
        if( useFixed ) return;
        
        if( rg2d == null ) transform.Translate( speed * Time.deltaTime ,0f,0f);
        else
        {
            mathSign = Mathf.Sign(sign);
            Vector2 vel = rg2d.velocity;
            vel.x = Mathf.MoveTowards(vel.x, speed * mathSign, Time.deltaTime * speed2);
            rg2d.velocity = vel;
        }
    }
    
    void FixedUpdate()
    {
        if( useFixed == false ) return;
        
        if( rg2d == null )transform.Translate( speed * Time.deltaTime ,0f,0f);
        else
        {
            mathSign = Mathf.Sign(sign);
            Vector2 vel = rg2d.velocity;
            vel.x = Mathf.MoveTowards(vel.x, speed * mathSign, Time.deltaTime * speed2);
            rg2d.velocity = vel;
        }
    }
}
