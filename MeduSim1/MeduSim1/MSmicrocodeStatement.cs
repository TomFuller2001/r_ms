using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeduSim1
{
    /// <summary>
    /// MSmicrocodeStatement represents a single microcode statement. This is the
    ///    basis for microstore contents and is also used for updating 
    ///    the Holzberlein textbook microcode table. An example statement has the form:
    ///    
    ///                    Hi EEPROM    Mid EEPROM    Low EEPROM
    ///    Label  Address  Write Read    ALU Jump    Jump address   Data moves    Jump address label   Comments
    ///    OP-IN	 00        01           0F           xx         MUX:=M(MAR)      HM-OPCODE-IN      // load the next op code
    /// </summary>
    class MSmicrocodeStatement
    { // TODO
        public string sJumpLabel;
        public int memAddress, write, read, alu, jumpCode, jumpAddress;
        
         // Constructor when the memory address is NOT known
            public MSmicrocodeStatement(string _sJumpLabel, int _write, int _read, int _alu, int _jumpCode, int _jumpAddress)
        {
            sJumpLabel = _sJumpLabel;
            memAddress = 0; // This will assigned later by the compiler
            write = _write;
            read = _read;
            alu = _alu;
            jumpCode = _jumpCode;
            jumpAddress = _jumpAddress;
        }

        // Constructor when the memory address IS known
        public MSmicrocodeStatement(string _sJumpLabel, int _memAddress, int _write, int _read, int _alu, int _jump, int _jumpAddress)
        {
            sJumpLabel = _sJumpLabel;
            memAddress = _memAddress;
            write = -write;
            read = _read;
            alu = _alu;
            jumpCode = _jump;
            jumpAddress = _jumpAddress;
        }
    }
}
