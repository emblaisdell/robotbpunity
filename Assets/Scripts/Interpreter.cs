using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interpreter {
    void StartAtPC(int pc);
    bool IsRunning();
    void ExecuteNextInstruction();
}
