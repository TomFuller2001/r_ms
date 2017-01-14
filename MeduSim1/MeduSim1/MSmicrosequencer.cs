using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeduSim1
{
    /// <summary>
    /// MSmicrosequencer stores the contents of the read and write busses. It stores the five-bit status word. 
    ///    It performs the state changes corresponding to the current microcode instruction. It can post these
    ///    changes to a string when so requested. The string has the form:
    ///    STATE: ...
    ///       microinstruction
    ///    STATE: ...
    ///       microinstruction
    ///    STATE: ...
    ///       microinstruction
    ///
    ///    It is a singleton class (Design Patterns in C# by Steven Metsker, Chapter 8)
    ///    since Medusa can only have one microsequencer at a time.
    /// </summary>
    class MSmicrosequencer
    {
        public enum RegisterSet { PipelineWrite = 0, PipelineRead = 1, PipelineALU = 2, PipelineJump = 3, MUX = 4, MRTN = 5, MPC = 6};
        private const int iRegCount = 7; // count of registers
        // This list is used by ToString
        private RegisterSet[] aRegisters = { RegisterSet.PipelineWrite, RegisterSet.PipelineRead, RegisterSet.PipelineALU, RegisterSet.PipelineJump,
            RegisterSet.MUX, RegisterSet.MRTN, RegisterSet.MPC };

        // These are the actual contents of the registers.
        private byte[] MicroRegisterContent; 

        // References to other useful objects TODO: These may no longer be needed
        private MSstate state;
        private MScontrolTable ControlTable;
        private MS_ALU alu;

        /// <summary>
        /// The single object instance for this class. The C# compiler
        ///    guarantees that this is initialized to null.
        /// </summary>
        private static MSmicrosequencer c_microSeq;

        /// <summary>
        /// To prevent access by more than one thread. This is the specific lock 
        /// belonging to the PO_NameFixer Class object.
        /// </summary>
        private static Object c_seqLock = typeof(MSmicrosequencer);

        /// <summary>
        /// Instead of a constructor, we offer a static method to return the only instance.
        /// This is a private constructor so no one else can create instances of MSmicrosequencer.
        /// </summary>
        private MSmicrosequencer() { }

        /// <summary>
        /// Gets the only instance of the Get_MSmicrosequencer class that may exist.  If a single
        /// instance of Get_MSmicrosequencer has not yet been created, one is created.
        /// PRE:    None.
        /// POST:   An MSmicrosequencer (reference) has been returned, or created and returned.
        /// </summary>
        /// <returns>Returns a reference to the only existing instance of MSmicrosequencer.</returns>
        public static MSmicrosequencer Get_MSmicrosequencer()
        {
            lock (c_seqLock)
            {
                // if this is the first request, initialize the one instance
                if (c_microSeq == null)
                {
                    // create the single object instance
                    c_microSeq = new MSmicrosequencer();
                    c_microSeq.LoadInitStates();
                } // if null

                // whether new or old, return a reference to the only instance
                return c_microSeq;
            } // lock
        }

        private void LoadInitStates()
        {
            // allocate memory; it is automatically set to zeros
            MicroRegisterContent = new byte[iRegCount];

            state = MSstate.Get_MSstate();
            alu = MS_ALU.Get_MS_ALU();
            ControlTable = new MScontrolTable();
        }

        public override string ToString()
        {
            string ret = "";
            // Print register contents; ToString("X") prints the integer address as hexadecimal
            foreach (RegisterSet rs in aRegisters)
                ret += string.Format("{0}:{1,2:X2}  ", rs.ToString(), MicroRegisterContent[(int) rs].ToString());
            ret += System.Environment.NewLine;

            return ret;
        }

        /// <summary>
        /// This method determines the next address, based on the current microinstruction,
        ///    status word, MRTN register, etc. 
        /// </summary>
        public bool GetJump(MSmicrocodeStatement mc)
        { // TODO This hard code neglects the MScontrolTable!
            if (mc.jumpCode == 0)
            {
                // No jump; increment MPC and set to MUX register
                MicroRegisterContent[(int)RegisterSet.MUX] = ++MicroRegisterContent[(int)RegisterSet.MPC];
                return true;
            }

            // TODO: rest of jump codes
            return false;
        }

    }
}
