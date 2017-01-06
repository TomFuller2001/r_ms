using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeduSim1
{
    /// <summary>
    /// MScontrolTable represents Table 13.1b  "Control Logic Select Codes And Micro-Jump Address Information" in Holzberlein. 
    ///    It is currently hard-coded but after testing, (TODO) it will be loaded from an xml file. It has four columns with
    ///    16 entries in each column:
    ///    
    ///             Hi EEPROM    Mid EEPROM
    ///    Address  Write Read    ALU Jump 
    ///      00       0    1       0    F     
    /// </summary>
    class MScontrolTable
    {
        public enum ALU_fn { B, A, not_A, AandB, AorB, AxorB, At1, AtB, AtBt1, A_1, A_B, A_B_1, t1, t0, t_1 };
        // Jump commands
        public enum ALU_jmp { nop, call, always, rtn, neg, zero, pos, carry, NOT_CDAV, OP_in};

#if false 
No Write(mux)0	0	No Read     B	0	0	No M-JUMP XX
Write Mem	1	1	Read Mem    A	1	1	M-JUMP Alw.M-ADD
Write ACLo  2	2	Read ACLo   Not A	2	2	M-CALL M-ADD
Write ACHi  3	3	Read ACHi   A and B 3	3	M-RTN XX
Write BRLo	4	4	Read BRLo   A or B  4	4	M-JUMP neg  M-ADD
Write BRHi  5	5	Read BRHi   A xor B 5	5	M-JUMP zero M-ADD
Write PCLo  6	6	Read PCLo   not used	6	6	M-JUMP pos  M-ADD
Write PCHi  7	7	Read PCHi   A + 1	7	7	M-JUMP carry    M-ADD
Write SPLo  8	8	Read SPLo   A + B   8	8	JUMP if /CDAV M-ADD
Write SPHi  9	9	Read SPHi   A + B + 1	9	9	not used    XX
Write MARLo A   A   inc PC A - 1	A A   not used    XX
Write MARHi B   B   inc MAR A - B B   B not used XX
not used    C   C   not used    A - B - 1	C C   not used    XX
Write ALUi D   D    ALUi read   1	D D   not used    XX /// TODO impossible to read ALUin directly
inc MAR&PC E   E    not used    0	E E   not used    XX /// TODO better not choose two inc in one instruction!
UART Xmit   F F     UART RcvDat	-1	F F   HM OPCODEin XX
#endif
        // We hard-code the control table while testing. TODO read in from xml file
        public MSstate.RegSet[] aCTwrite = { MSstate.RegSet.nop, MSstate.RegSet.MEM, MSstate.RegSet.AClo, MSstate.RegSet.AChi, MSstate.RegSet.BRlo, MSstate.RegSet.BRhi, MSstate.RegSet.PClo, MSstate.RegSet.PChi,
            MSstate.RegSet.SPlo, MSstate.RegSet.SPhi, MSstate.RegSet.MARlo, MSstate.RegSet.MARhi, MSstate.RegSet.nop, MSstate.RegSet.ALUin, MSstate.RegSet.incPC_MAR, MSstate.RegSet.UART };

        public MSstate.RegSet[] aCTread = { MSstate.RegSet.nop, MSstate.RegSet.MEM, MSstate.RegSet.AClo, MSstate.RegSet.AChi, MSstate.RegSet.BRlo, MSstate.RegSet.BRhi, MSstate.RegSet.PClo, MSstate.RegSet.PChi,
            MSstate.RegSet.SPlo, MSstate.RegSet.SPhi, MSstate.RegSet.incPC, MSstate.RegSet.incMAR, MSstate.RegSet.nop, MSstate.RegSet.nop, MSstate.RegSet.nop, MSstate.RegSet.UART };

        public ALU_fn[] aFn = { ALU_fn.B, ALU_fn.A, ALU_fn.not_A, ALU_fn.AandB, ALU_fn.AorB, ALU_fn.AxorB, ALU_fn.At1, ALU_fn.AtB,
            ALU_fn.AtBt1, ALU_fn.A_1, ALU_fn.A_B, ALU_fn.A_B_1, ALU_fn.t1, ALU_fn.t0, ALU_fn.t_1};

        public ALU_jmp[] aJump = { ALU_jmp.nop, ALU_jmp.always, ALU_jmp.call, ALU_jmp.rtn, ALU_jmp.neg, ALU_jmp.zero, ALU_jmp.pos,
            ALU_jmp.carry, ALU_jmp.NOT_CDAV, ALU_jmp.nop, ALU_jmp.nop, ALU_jmp.nop, ALU_jmp.nop, ALU_jmp.nop, ALU_jmp.nop, ALU_jmp.OP_in };
    
        public MScontrolTable () {}
    }
}
