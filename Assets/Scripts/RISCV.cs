using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RISCV : Interpreter {
    bool isRunning;

    public const int memLength = 1048576;
    public Controller controller;
    private int pc = 0;
    private int[] registers = new int[32];
    private int[] memory;

    public RISCV(int[] initMemory, Controller controller)
    {
        memory = initMemory;
        this.controller = controller;
    }

    public void StartAtPC(int pc)
    {
        this.pc = pc;
        isRunning = true;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    private int ReadReg(int reg)
    {
        if (reg == 0) // zero register is constant 0
        {
            return 0;
        }
        return registers[reg];
    }

    private void WriteReg(int reg, int value)
    {
        registers[reg] = value;
    }

    private int LoadMemory(int addr)
    {
        return memory[addr];
    }

    private void StoreMemory(int addr, int value)
    {
        memory[addr] = value;
    }

    private int SignExtend(int num, int bitIndex) // bitIndex = bits-1
    {
        if ((num >> bitIndex) == 1)
        {
            return ((-1) << bitIndex) | num;
        }
        return num;
    }

    public void ExecuteNextInstruction()
    {
        ExecuteInstruction(LoadMemory(pc));
    }

    public void ExecuteInstruction(int instr)
    {
        //MonoBehaviour.print(pc);
        //MonoBehaviour.print(System.Convert.ToString(instr, 16));
        //MonoBehaviour.print(System.Convert.ToString(instr, 2));
        int opcode = instr & 0x7f;
        if (opcode == 0x37) // LUI
        {
            UType args = new UType(instr);
            WriteReg(args.rd, (args.imm << 12) | (ReadReg(args.rd) & 0xfff));
        } else if (opcode == 0x17) // AUIPC
        {
            UType args = new UType(instr);
            WriteReg(args.rd, (args.imm << 12) + pc);
        } else if (opcode == 0x6f) // JAL
        {
            JType args = new JType(instr);
            WriteReg(args.rd, pc + 4);
            pc += SignExtend(args.imm, 20) / 4 - 1;
        } else if (opcode == 0x67) // JALR
        {
            IType args = new IType(instr);
            WriteReg(args.rd, pc + 4);
            pc = ((ReadReg(args.rs1) + pc) >> 1) << 1;
        } else if (opcode == 0x63) // branch
        {
            BType args = new BType(instr);
            /*MonoBehaviour.print("branch");
            MonoBehaviour.print(args.imm);
            MonoBehaviour.print(SignExtend(args.imm, 12));*/
            bool jump;
            if (args.funct3 == 0x0) // BEQ
            {
                jump = (ReadReg(args.rs1) == ReadReg(args.rs2));
            } else if (args.funct3 == 0x1) // BNE
            {
                jump = (ReadReg(args.rs1) != ReadReg(args.rs2));
            } else if (args.funct3 == 0x4) // BLT
            {
                jump = (ReadReg(args.rs1) < ReadReg(args.rs2));
            } else if (args.funct3 == 0x5) // BGE
            {
                jump = (ReadReg(args.rs1) >= ReadReg(args.rs2));
            } else if (args.funct3 == 0x6) // BLTU
            {
                jump = (((uint)ReadReg(args.rs1)) < ((uint)ReadReg(args.rs2)));
            } else if (args.funct3 == 0x7) // BGEU
            {
                jump = (((uint)ReadReg(args.rs1)) >= ((uint)ReadReg(args.rs2)));
            } else // unrecognized
            {
                jump = false;
            }
            if (jump)
            {
                pc += SignExtend(args.imm, 12) / 4 - 1;
            }
        } else if (opcode == 0x03) // load
        {
            IType args = new IType(instr);
            if (args.funct3 == 0x0) // LB
            {
                // unimplemented
            }
            else if (args.funct3 == 0x1) // LH
            {
                // unimplemented
            }
            else if (args.funct3 == 0x2) // LW
            {
                WriteReg(args.rd, LoadMemory(SignExtend(args.imm, 12) + ReadReg(args.rs1)));
            }
            else if (args.funct3 == 0x4) // LBU
            {
                // unimplemented
            }
            else if (args.funct3 == 0x5) // LHU
            {
                // unimplemented
            }
            else
            {
                // unrecognized
            }
        } else if (opcode == 0x23) // store
        {
            SType args = new SType(instr);
            if (args.funct3 == 0x0) // SB
            {
                // unimplemented
            }
            else if (args.funct3 == 0x1) // SH
            {
                // unimplemented
            }
            else if (args.funct3 == 0x2) // SW
            {
                StoreMemory(SignExtend(args.imm, 12) + ReadReg(args.rs1), ReadReg(args.rs2));
            }
            else
            {
                // unrecognized
            }
        } else if (opcode == 0x13) // immediate
        {
            IType args = new IType(instr);
            args.imm = SignExtend(args.imm, 11);
            int value;
            if (args.funct3 == 0x0) // ADDI
            {
                value = ReadReg(args.rs1) + args.imm;
            } else if (args.funct3 == 0x2) // SLTI
            {
                value = (ReadReg(args.rs1) < args.imm) ? 1 : 0;
            } else if (args.funct3 == 0x3) // SLTIU
            {
                value = (((uint)ReadReg(args.rs1)) < ((uint)args.imm)) ? 1 : 0;
            } else if (args.funct3 == 0x4) // XORI
            {
                value = ReadReg(args.rs1) ^ args.imm;
            } else if (args.funct3 == 0x6) // ORI
            {
                value = ReadReg(args.rs1) | args.imm;
            } else if (args.funct3 == 0x7) // ANDI
            {
                value = ReadReg(args.rs1) & args.imm;
            } else // unrecognized
            {
                value = 0;
            }
            WriteReg(args.rd, value);
        } else if (opcode == 0x33) // arithmetic
        {
            RType args = new RType(instr);
            int value;
            int rs1 = ReadReg(args.rs1);
            int rs2 = ReadReg(args.rs2);
            if (args.funct3 == 0x0)
            {
                if (args.funct7 == 0x00) // ADD
                {
                    value = rs1 + rs2;
                } else if (args.funct7 == 0x20) // SUB
                {
                    value = rs1 - rs2;
                } else // unrecognized
                {
                    value = 0;
                }
            } else if (args.funct3 == 0x1)
            {
                if (args.funct7 == 0x00) // SLL
                {
                    value = rs1 << rs2;
                }
                else
                {
                    value = 0;
                }
            } else if (args.funct3 == 0x2)
            {
                if (args.funct7 == 0x00) // SLT
                {
                    value = (rs1 < rs2) ? 1 : 0;
                } else
                {
                    value = 0;
                }
            } else if (args.funct3 == 0x3)
            {
                if (args.funct7 == 0x00) // SLTU
                {
                    value = (((uint)rs1) < ((uint)rs2)) ? 1 : 0;
                }
                else
                {
                    value = 0;
                }
            }
            else if (args.funct3 == 0x4)
            {
                if (args.funct7 == 0x00) // XOR
                {
                    value = rs1 ^ rs2;
                }
                else
                {
                    value = 0;
                }
            }
            else if (args.funct3 == 0x5)
            {
                if (args.funct7 == 0x00) // SRL
                {
                    value = (int)(((uint)rs1) >> rs2);
                } else if (args.funct7 == 0x20) // SRA
                {
                    value = rs1 >> rs2;
                }
                else
                {
                    value = 0;
                }
            }
            else if (args.funct3 == 0x6)
            {
                if (args.funct7 == 0x00) // OR
                {
                    value = rs1 | rs2;
                }
                else
                {
                    value = 0;
                }
            }
            else if (args.funct3 == 0x7)
            {
                if (args.funct7 == 0x00) // AND
                {
                    value = rs1 & rs2;
                }
                else
                {
                    value = 0;
                }
            }
            else
            {
                value = 0;
            }
            WriteReg(args.rd, value);
        } else if (opcode == 0x0f) // fence
        {
            // unimplemented
        } else if (opcode == 0x73) // environment
        {
            IType args = new IType(instr);
            if (args.funct3 == 0x0) { // system
                if (args.imm == 0x000)
                {
                    if (instr == 0x00100073) // ECALL
                    {
                        // unimplemented
                    }
                    else
                    {
                        // unrecognized
                    }
                }
                else if (args.imm == 0x001) {
                    if (instr == 0x00100073) // EBREAK
                    {
                        isRunning = false;
                    }
                    else
                    {
                        // unrecognized
                    }
                }
                else
                {
                    // unrecognized
                }
            }
            else if(args.funct3 == 0x1) // CSRRW
            {
                WriteReg(args.imm, controller.ReadCSR(args.rd));
                controller.WriteCSR(args.imm, ReadReg(args.rs1));
            }
            else if(args.funct3 == 0x2) // CSRRS
            {
                int csr = controller.ReadCSR(args.rd);
                WriteReg(args.imm, csr);
                int mask = ReadReg(args.rs1);
                controller.WriteCSR(args.imm, mask | csr);
            }
            else if (args.funct3 == 0x3) // CSRRC
            {
                int csr = controller.ReadCSR(args.rd);
                WriteReg(args.imm, csr);
                int mask = ~ReadReg(args.rs1);
                controller.WriteCSR(args.imm, mask & csr);
            }
            else if (args.funct3 == 0x5) // CSRRWI
            {
                WriteReg(args.imm, controller.ReadCSR(args.rd));
                controller.WriteCSR(args.imm, args.rs1);
            }
            else if (args.funct3 == 0x6) // CSRRSI
            {
                int csr = controller.ReadCSR(args.rd);
                WriteReg(args.imm, csr);
                int mask = args.rs1;
                controller.WriteCSR(args.imm, mask | csr);
            }
            else if (args.funct3 == 0x7) // CSRRCI
            {
                int csr = controller.ReadCSR(args.rd);
                WriteReg(args.imm, csr);
                int mask = ~args.rs1;
                controller.WriteCSR(args.imm, mask & csr);
            }
            else
            {
                // unrecognized
            }
        } else // unrecognized
        {

        }
        pc++;
    }

    class RType {
        public int funct3,funct7;
        public int rs1,rs2,rd;
        public RType(int instr)
        {
            funct3 = (instr >> 12) & 0x7;
            funct7 = (instr >> 25) & 0x7f;
            rs1 = (instr >> 15) & 0x1f;
            rs2 = (instr >> 20) & 0x1f;
            rd = (instr >> 7) & 0x1f;
        }
    }
    class IType {
        public int imm;
        public int funct3;
        public int rs1, rd;
        public IType(int instr)
        {
            imm = (instr >> 20) & 0xfff;
            funct3 = (instr >> 12) & 0x7;
            rs1 = (instr >> 15) & 0x1f;
            rd = (instr >> 7) & 0x1f;
        }
    }
    class SType
    {
        public int imm;
        public int funct3;
        public int rs1, rs2;
        public SType(int instr)
        {
            imm = ((instr >> 21) & 0x7f0) |
                ((instr >> 7) & 0xf);
            funct3 = (instr >> 12) & 0x7;
            rs1 = (instr >> 15) & 0x1f;
            rs2 = (instr >> 20) & 0x1f;
        }
    }
    class BType
    {
        public int imm;
        public int funct3;
        public int rs1, rs2;
        public BType(int instr)
        {
            imm = ((instr >> 19) & 0x1000) |
                ((instr << 4) & 0x800) |
                ((instr >> 20) & 0x7e0) |
                ((instr >> 7) & 0x1e);
            funct3 = (instr >> 12) & 0x7;
            rs1 = (instr >> 15) & 0x1f;
            rs2 = (instr >> 20) & 0x1f;
        }
    }
    class UType
    {
        public int imm;
        public int rd;
        public UType(int instr)
        {
            imm = (instr >> 12) & 0xfffff;
            rd = (instr >> 7) & 0x1f;
        }
    }
    class JType
    {
        public int imm;
        public int rd;
        public JType(int instr)
        {
            imm = ((instr >> 11) & 0x100000) |
                (instr & 0xff000) |
                ((instr >> 9) & 0x800) |
                ((instr >> 20) & 0x7fe);
            rd = (instr >> 7) & 0x1f;
        }
    }
}
