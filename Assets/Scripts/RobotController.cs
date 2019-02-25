using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : Controller
{
    Robot robot;

    const float fixedPoint = 1024f;

    /*
     * CSR addrs:
     * 0: <reserved for ecall>
     * 1: <reserved for ebreak>
     * 2: theta_w
     * 3: theta_a
     * 4: theta_h
     * 5: omega_w
     * 6: omega_a
     * 7: omega_h
     * 8: tau_w
     * 9: tau_a
     * a: tau_h
     * b: configuration
     * c: dist (not yet)
     * 
     */

    public RobotController(Robot robot)
    {
        this.robot = robot;
    }

    public int ReadCSR(int addr)
    {
        float value = -1;
        if(addr == 0x2)
        {
            value = robot.waistAngle;
        }
        else if(addr == 0x3)
        {
            value = robot.armAngle;
        }
        else if(addr == 0x4)
        {
            value = robot.handAngle;
        }
        else if(addr == 0x5)
        {
            value = robot.waistAngularVelocity;
        }
        else if(addr == 0x6)
        {
            value = robot.armAngularVelocity;
        }
        else if(addr == 0x7)
        {
            value = robot.handAngularVelocity;
        }
        else if(addr == 0x8)
        {
            value = robot.waistTorque;
        }
        else if(addr == 0x9)
        {
            value = robot.armTorque;
        }
        else if(addr == 0xa)
        {
            value = robot.handTorque;
        }
        else if(addr == 0xb)
        {
            // unimplemented
        }
        else if(addr == 0xc)
        {
            // unimplemented
        }
        else
        {
            // unrecognized
        }

        return Float2Int(value);
    }

    public void WriteCSR(int addr, int intValue)
    {
        /*MonoBehaviour.print("write csr");
        MonoBehaviour.print("addr: "+addr);
        MonoBehaviour.print("value: "+intValue);*/
        float value = Int2Float(intValue);
        if(addr == 0x8)
        {
            robot.waistTorque = value;
        }
        else if(addr == 0x9)
        {
            robot.armTorque = value;
        }
        else if(addr == 0xa)
        {
            robot.handTorque = value;
        }
        else
        {
            // unregonized
        }
    }

    static int Float2Int(float value)
    {
        return Mathf.RoundToInt(fixedPoint * value);
    }
    static float Int2Float(int value)
    {
        return value / fixedPoint;
    }
}
