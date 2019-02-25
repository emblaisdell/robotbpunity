using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Controller
{
    int ReadCSR(int addr);
    void WriteCSR(int addr, int value);
}
