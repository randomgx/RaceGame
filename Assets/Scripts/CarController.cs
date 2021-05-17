using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float m_horizontalInput;
    private float m_verticalInput;

    private Rigidbody rb;

    public float m_groundedDrag = 3f;

    public GameObject[] hoverPoints;
    public GameObject planeCheck;
    public float hoverHeight = 1.5f;
    public float hoverForce = 1000f;

    public float gravityForce = 1000f;

    public float forwardForce = 8000f;
    public float reverseForce = 4000f;

    public float jumpForce = 1500f;
    float thrust = 0f;
    float airturn = 0f;
    float deadZone = 0.1f;

    public float maxVelocity = 50;

    public float sidewaysTraction;

    public float turnStrength = 1000f;

    float turnValue = 0f;

    public Transform CenterOfMass;
    public Transform accelPoint;

    public Transform frontL, frontR;
    public Transform rearL, rearR;

    int layerMask;

    public bool grounded = false;
    public bool jump = false;

    public bool tilt = false;

    RaycastHit planeHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CenterOfMass.localPosition;

        layerMask = 1 << LayerMask.NameToLayer("Vehicle");
        layerMask = ~layerMask;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && grounded)
        {
            jump = true;
        }

        if(Input.GetButtonDown("Tilt"))
        {
            tilt = true;
        }
        if(Input.GetButtonUp("Tilt"))
        {
            tilt = false;
        }

        // Main Thrust
        thrust = 0.0f;
        float acceleration = Input.GetAxis("Acceleration");
        airturn = Input.GetAxis("AirTurn");

        if (acceleration > deadZone)
            thrust = acceleration * forwardForce;
        else if (acceleration < -deadZone)
            thrust = acceleration * reverseForce;

        // Turning
        turnValue = 0.0f;
        float turnAxis = Input.GetAxis("Turn");
        if (Mathf.Abs(turnAxis) > deadZone)
            turnValue = turnAxis;
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        grounded = false;
        for (int i = 0; i < hoverPoints.Length; i++)
        {
            var hoverPoint = hoverPoints[i];
            if (Physics.Raycast(hoverPoint.transform.position, -hoverPoint.transform.up, out hit, hoverHeight, layerMask))
            {
                Debug.DrawRay(hoverPoint.transform.position, -hoverPoint.transform.up * (1.0f - (hit.distance / hoverHeight)), Color.yellow);
                rb.AddForceAtPosition(Vector3.up * hoverForce * (1.0f - (hit.distance / hoverHeight)), hoverPoint.transform.position);
                grounded = true;
            }
        }

        if (Physics.Raycast(planeCheck.transform.position, -planeCheck.transform.up, out planeHit, hoverHeight * 5, layerMask))
        {
            Debug.DrawRay(planeCheck.transform.position, -Vector3.up*hoverHeight, Color.green);
        }

        if (jump)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jump = false;
        }

        if (grounded)
        {
            rb.drag = m_groundedDrag;
        }
        else
        {
            rb.drag = 0.1f;
            thrust /= 100f;
            //turnValue /= 5f;
        }

        if (Mathf.Abs(thrust) > 0)
        { 

            Vector3 dir = accelPoint.forward;
            dir.y = -0.1f;
            dir.Normalize();

            Vector3 f = Vector3.ProjectOnPlane(accelPoint.forward, planeHit.normal);

            rb.AddForceAtPosition(f * thrust, accelPoint.position);

            Debug.DrawRay(accelPoint.position, f * thrust, Color.red);
        }

        if (!grounded)
        {
            if(airturn > 0 || airturn < 0)
            {
                rb.AddRelativeTorque(Vector3.right * airturn * turnStrength);
            }
        }

        if (turnValue > 0)
        {
            if(!tilt)
            rb.AddRelativeTorque(Vector3.up * turnValue * turnStrength);

            if(!grounded && tilt)
            rb.AddRelativeTorque(-Vector3.forward * turnValue * turnStrength);
        }
        else if (turnValue < 0)
        {
            if(!tilt)
            rb.AddRelativeTorque(Vector3.up * turnValue * turnStrength);

            if (!grounded && tilt)
                rb.AddRelativeTorque(-Vector3.forward * turnValue * turnStrength);
        }

        // Limit max velocity
        if (rb.velocity.sqrMagnitude > (rb.velocity.normalized * maxVelocity).sqrMagnitude)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        // Sideways traction
        Vector3 contrarySidewaysVelocity = -Vector3.Project(rb.velocity, transform.right);
        if (contrarySidewaysVelocity.sqrMagnitude > 0.0f)
        {
            //rb.AddForce(contrarySidewaysVelocity * sidewaysTraction, ForceMode.Acceleration);
            Debug.DrawRay(transform.position, contrarySidewaysVelocity, Color.cyan);
        }

        // Wheels Rolling Resistance, turn front and back traction (so vehicle doesnt tend to move front/back forever)
        //const float WHEELS_ROLLING_RESISTANCE_COEFFICIENT = 0.2f;
        //Vector3 wheelsRollingResistanceForce = -(straightVelocity * WHEELS_ROLLING_RESISTANCE_COEFFICIENT);
        //rb.AddForce(wheelsRollingResistanceForce, ForceMode.Acceleration);
    }
}