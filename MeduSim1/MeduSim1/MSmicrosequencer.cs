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
        public enum RegisterSet { PipelineWrite = 0, PipelineRead = 1, PipelineALU = 3, PipelineJump = 4, MUX, MRTN, MPC};
        private const int cFF = 0xFF; // default contents for registers at startup (=255)
        // This list is used by toString
        private RegisterSet[] aRegisters = { RegisterSet.PipelineWrite, RegisterSet.PipelineRead, RegisterSet.PipelineALU, RegisterSet.PipelineJump,
            RegisterSet.MUX, RegisterSet.MRTN, RegisterSet.MPC };

        private Dictionary<RegisterSet, int> MicroRegisterContent;
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

                    c_microSeq.MicroRegisterContent = new Dictionary<RegisterSet, int>();
                    c_microSeq.LoadInitStates();
                    c_microSeq.state = MSstate.Get_MSstate();
                    c_microSeq.alu  = MS_ALU.Get_MS_ALU();
                } // if null

                // whether new or old, return a reference to the only instance
                return c_microSeq;
            } // lock
        }
        private void LoadInitStates()
        {
            foreach (RegisterSet rs in aRegisters)
                MicroRegisterContent[rs] = 0;

            ControlTable = new MScontrolTable();
        }

        public override string ToString()
        {
            String ret = "";
            // Print register contents; ToString("X") prints the integer address as hexadecimal
            foreach (RegisterSet rs in aRegisters)
                ret += string.Format("{0}:{1}  ", rs.ToString(), MicroRegisterContent[rs].ToString("X"));
            ret += System.Environment.NewLine;

            return ret;
        }

        public bool PerformOneMicroStatement(MSmicrocodeStatement mc)
        {
            // handle the easy cases first
            if (ControlTable.aCTread[mc.write] == MSstate.RegSet.incPC_MAR)
            {
                PerformIncMAR(); PerformIncPC(); return true;
            }

            if (ControlTable.aCTread[mc.read] == MSstate.RegSet.incPC)
            {
                PerformIncPC(); return true;
            }

            if (ControlTable.aCTread[mc.read] == MSstate.RegSet.incMAR)
            {
                PerformIncMAR(); return true;
            }

            // determine the source and destination of this instruction
            //    (Though they may not always be applicable)
            MSstate.RegSet RWdestination, RWsource;

            if (mc.write == 13 || mc.write == 15 || (mc.write > 0 && mc.write < 12))
                RWdestination = ControlTable.aCTwrite[mc.write];
            else return false; // TODO throw exception

            if (mc.read == 13 || mc.read == 15 || (mc.read > 0 && mc.read < 10))
                    RWsource = ControlTable.aCTwrite[mc.read];
                else return false; // TODO throw exception

            // Handle the most frequent case (assignment), ALU "does" B
            if (ControlTable.aFn[mc.alu] == MScontrolTable.ALU_fn.B)
            {
                PerformAssign(RWdestination, RWsource); return true;
            }

            // More complicated ALU functions are remanded to the ALU object
            //    but return here to handle the possible micro jump.
            alu.PerformOneMicroStatement(mc.alu, RWdestination, RWsource);

            // Determine the next micro code address
            return false;
        }

        private void PerformAssign(MSstate.RegSet destination, MSstate.RegSet source)
        {
            // perform the assignment
            state.RegisterContent[destination] = state.RegisterContent[source];
        }

        /// <summary>
        /// This is a bit subtle. The actual hardware increment for PC or MAR increments the
        ///    entire register. That is, if PClo is 255, it becomes 0 and PChi is incremented by one.
        ///    This method emulates that action. 
        /// </summary>
        /// <returns>Returns true.</returns>
        private bool PerformIncPC()
        {
            if (state.RegisterContent[MSstate.RegSet.PClo] == 255)
            {
                state.RegisterContent[MSstate.RegSet.PClo] = 0;
                state.RegisterContent[MSstate.RegSet.PChi] += 1;
            }
            else
                state.RegisterContent[MSstate.RegSet.PClo] += 1;
            return true;
        }

        /// <summary>
        /// This is a bit subtle. The actual hardware increment for PC or MAR increments the
        ///    entire register. That is, if MARlo is 255, it becomes 0 and MARhi is incremented by one.
        ///    This method emulates that action. 
        /// </summary>
        /// <returns>Returns true.</returns>
        private bool PerformIncMAR()
        {
            if (state.RegisterContent[MSstate.RegSet.MARlo] == 255) {
                state.RegisterContent[MSstate.RegSet.MARlo] = 0;
                state.RegisterContent[MSstate.RegSet.MARhi] += 1;
            } else
                state.RegisterContent[MSstate.RegSet.MARlo] += 1;
            return true;
        }

    }
}
