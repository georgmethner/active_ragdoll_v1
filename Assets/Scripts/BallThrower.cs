using UnityEngine;

public class BallThrower : MonoBehaviour
{
    public GameObject ballPrefab;
    public float strength = 20;
    
    //throw ball every 2 seconds
    private float throwInterval = 2f;
    
    private float throwTimer = 0f;
    
    void Update()
    {
        throwTimer += Time.deltaTime;
        
        if (throwTimer >= throwInterval)
        {
            throwTimer = 0f;
            ThrowBall();
        }
    }
    
    void ThrowBall()
    {
        GameObject ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
        Destroy(ball, 2f);
        
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * strength, ForceMode.Impulse);
    }
}
