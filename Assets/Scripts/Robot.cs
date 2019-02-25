using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : MonoBehaviour {

    public int team;

    bool running;

    public Controller controller;
    public Interpreter interpreter;

    const float clockRate = 1024f;

    const float armLength = 0.8f;
    const float handLength = 0.8f;

    public GameObject cosmBall;
    public GameObject ball;

    private GameObject cosmBallInst;

    public Transform Base;
    public Transform Waist;
    public Transform Arm;
    public Transform Hand;
    public Transform Magnet;

    public float waistAngle; //radians
    public float armAngle;
    public float handAngle;

    public float waistAngularVelocity;
    public float armAngularVelocity;
    public float handAngularVelocity;

    public float waistTorque;
    public float armTorque;
    public float handTorque;

    const float friction = 1f;

    // Use this for initialization
    public void Init (int[] mem) {
        controller = new RobotController(this);
        interpreter = new RISCV(mem, controller);
	}

    private void Update()
    {
        if (running)
        {
            int cycles = Mathf.RoundToInt(clockRate * Time.deltaTime);
            for(int cycle=0; cycle<cycles; cycle++)
            {
                interpreter.ExecuteNextInstruction();
            }
        }
    }

    private void FixedUpdate()
    {
        if (running)
        {
            waistAngularVelocity += waistTorque * Time.fixedDeltaTime;
            armAngularVelocity += armTorque * Time.fixedDeltaTime;
            handAngularVelocity += handTorque * Time.fixedDeltaTime;
        }
        else
        {
            float diff = friction * Time.fixedDeltaTime;
            waistAngularVelocity = (Mathf.Abs(waistAngularVelocity) > diff) ? (waistAngularVelocity - Mathf.Sign(waistAngularVelocity) * diff) : 0;
            armAngularVelocity = (Mathf.Abs(armAngularVelocity) > diff) ? (armAngularVelocity - Mathf.Sign(armAngularVelocity) * diff) : 0;
            handAngularVelocity = (Mathf.Abs(handAngularVelocity) > diff) ? (handAngularVelocity - Mathf.Sign(handAngularVelocity) * diff) : 0;
        }

        waistAngle += waistAngularVelocity * Time.fixedDeltaTime;
        armAngle += armAngularVelocity * Time.fixedDeltaTime;
        handAngle += handAngularVelocity * Time.fixedDeltaTime;
        
        if (armAngle < -Mathf.PI/2f)
        {
            armAngularVelocity = 0f;
            armAngle = -Mathf.PI / 2f;
        }
        if (armAngle > Mathf.PI / 2f)
        {
            armAngularVelocity = 0f;
            armAngle = Mathf.PI / 2f;
        }
        if (handAngle < -Mathf.PI / 2f)
        {
            handAngularVelocity = 0f;
            handAngle = -Mathf.PI / 2f;
        }
        if (handAngle > Mathf.PI / 2f)
        {
            handAngularVelocity = 0f;
            handAngle = Mathf.PI / 2f;
        }

        Waist.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * waistAngle);
        Arm.localRotation = Quaternion.Euler(0f, Mathf.Rad2Deg * armAngle, 0f);
        Hand.localRotation = Quaternion.Euler(0f, Mathf.Rad2Deg * handAngle, 0f);
    }

    public void StartUp()
    {
        running = true;
        interpreter.StartAtPC(0); // XXX
        cosmBallInst = Instantiate(cosmBall, Magnet);
    }

    public void ShutDown()
    {
        running = false;
        Destroy(cosmBallInst);
        GameObject ballInst = Instantiate(ball, Magnet.position, Quaternion.identity);
        ballInst.GetComponent<Rigidbody>().velocity = new Vector3(
            -Master.TeamDir(team) * (-waistAngularVelocity * Mathf.Sin(waistAngle) * (armLength * Mathf.Sin(armAngle) + handLength * Mathf.Sin(armAngle + handAngle))
            + Mathf.Cos(waistAngle) * (armLength * armAngularVelocity * Mathf.Cos(armAngle) + handLength * (armAngularVelocity + handAngularVelocity) * Mathf.Cos(armAngle + handAngle))),
            -armLength * armAngularVelocity * Mathf.Sin(armAngle) - handLength * (armAngularVelocity + handAngularVelocity) * Mathf.Sin(armAngle + handAngle),
            waistAngularVelocity * Mathf.Cos(waistAngle) * (armLength * Mathf.Sin(armAngle) + handLength * Mathf.Sin(armAngle + handAngle))
            + Mathf.Sin(waistAngle) * (armLength * armAngularVelocity * Mathf.Cos(armAngle) + handLength * (armAngularVelocity + handAngularVelocity) * Mathf.Cos(armAngle + handAngle))
        );

        /*
         * Kinematics:
         * <
         *  cos(theta) * ( Asin(alpha) + Hsin(alpha+beta) ),
         *  sin(theta) * ( Asin(alpha) + Hsin(alpha+beta) )
         *  Acos(alpha) + Hcos(alpha+beta)
         * >
         * 
         * The instantaneous change is the derivative
         * 
         */

        /*waistTorque = 0f;
        armTorque = 0f;
        handTorque = 0f;*/
    }

}
