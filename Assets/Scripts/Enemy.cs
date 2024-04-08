using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private bool staggered = false;
    private float timeInStagger = 0;
    [SerializeField] private float maxStaggerTime;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private float enemyHeight;
    [SerializeField] private LayerMask whatIsGround;
    private Rigidbody rb;
    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, enemyHeight * 0.5f + 0.2f, whatIsGround);

        StaggerCounter();
    }

    private void FixedUpdate()
    {
        if (!staggered)
        {
            // Do movement
            // Do attack
            //rb.useGravity = true;
        }
        else if (staggered && !grounded)
        {
            rb.useGravity = false;
            rb.velocity = new Vector3(0f, 0f, 0f);
        }
    }

    void StaggerCounter()
    {
        if (staggered)
            timeInStagger += Time.deltaTime;


        if (timeInStagger >= maxStaggerTime)
        {
            staggered = false;
            timeInStagger = 0.0f;

            ResetRotations();
        }
    }

    void ResetRotations()
    {
        enemyTransform.rotation = Quaternion.identity;
    }


    public void Hit()
    {
        staggered = true;
        enemyTransform.Rotate(new Vector3(0f, 0f, UnityEngine.Random.Range(15f, 30f) * (UnityEngine.Random.Range(0, 2) * 2 - 1)));
    }
}
