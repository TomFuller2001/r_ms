using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeduSim1
{
    /// <summary>
    /// MS_ALU stores the contents of the read and write busses. It stores the five-bit status word. 
    ///    It can perform the Medusa-demanded functions of a commercial ALU.
    ///    It's a singleton class (Design Patterns in C# by Steven Metsker, Chapter 8)
    ///    since Medusa can only have one ALU at a time.
    /// </summary>
    class MS_ALU
    {
        // Simulate the status word:
        bool bPos, bZero, bNeg, bCarry, bNOT_CDAV;

        // References to other useful objects TODO: These may no longer be needed
        private MSstate state;
        private MSmicrosequencer mseq;
        private MScontrolTable ControlTable;

        /// <summary>
        /// The single object instance for this class. The C# compiler
        ///    guarantees that this is initialized to null.
        /// </summary>
        private static MS_ALU c_alu;

        /// <summary>
        /// To prevent access by more than one thread. This is the specific lock 
        /// belonging to the MS_ALU Class object.
        /// </summary>
        private static Object c_aluLock = typeof(MS_ALU);

        /// <summary>
        /// Instead of a constructor, we offer a static method to return the only instance.
        /// This is a private constructor so no one else can create instances of MS_ALU.
        /// </summary>
        private MS_ALU() { }

        /// <summary>
        /// Gets the only instance of the PO_NameFixer class that may exist.  If a single
        /// instance of PO_NameFixer has not yet been created, one is created.
        /// 
        /// PRE:    None.
        /// POST:   An MSstate (reference) has been returned, or created and returned.
        /// </summary>
        /// <returns>Returns a reference to the only existing instance of MSstate.</returns>
        public static MS_ALU Get_MS_ALU()
        {
            lock (c_aluLock)
            {
                // if this is the first request, initialize the one instance
                if (c_alu == null)
                {
                    c_alu = new MS_ALU();
                    c_alu.Init();
                }

                // whether new or old, return a reference to the only instance
                return c_alu;
            } // lock
        }

        // Perform initialization
        private void Init()
        {
            bPos = bZero = bNeg = bCarry = bNOT_CDAV = false;

            state = MSstate.Get_MSstate();
            mseq = MSmicrosequencer.Get_MSmicrosequencer();
            ControlTable = new MScontrolTable();
        }

    public bool PerformOneMicroStatement(MSmicrocodeStatement mc)
        {
            // handle the easy cases first. Note that increments do NOT affect the status
            if (ControlTable.aCTwrite[mc.write] == MSstate.RegSet.incPC_MAR)
            {
                PerformIncMAR(); PerformIncPC(); return mseq.GetJump(mc);
            }

            if (ControlTable.aCTread[mc.read] == MSstate.RegSet.incPC)
            {
                PerformIncPC(); return mseq.GetJump(mc);
            }

            if (ControlTable.aCTread[mc.read] == MSstate.RegSet.incMAR)
            {
                PerformIncMAR(); return mseq.GetJump(mc);
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
                byte result = state.RegisterContent[RWsource];
                PerformAssign(RWdestination, result);
                return SetStatusAndJump(result, mc);
            } // alu:B

            if (ControlTable.aFn[mc.alu] == MScontrolTable.ALU_fn.A)
            {
                byte result = state.RegisterContent[MSstate.RegSet.ALUin];
                PerformAssign(RWdestination, result);
                return SetStatusAndJump(result, mc);
            } // alu:A

            if (ControlTable.aFn[mc.alu] == MScontrolTable.ALU_fn.not_A)
            {
                byte notA = (byte) ~state.RegisterContent[MSstate.RegSet.ALUin];
                PerformAssign(RWdestination, notA);
                return SetStatusAndJump(notA, mc);
            } // alu:not_A

            if (ControlTable.aFn[mc.alu] == MScontrolTable.ALU_fn.AandB)
            {
                byte A = state.RegisterContent[MSstate.RegSet.ALUin];
                byte B = state.RegisterContent[RWsource];
                // bitwise AND
                byte result = (byte)(A & B);
                PerformAssign(RWdestination, result);
                return SetStatusAndJump(result, mc);
            } // alu:AandB

            if (ControlTable.aFn[mc.alu] == MScontrolTable.ALU_fn.AtB) // A plus B TODO: test 
            {
                // All registers are stored as unsigned bytes. We need to use unsigned
                //    integers for the arithmetic operation
                uint A = unchecked ( (uint) state.RegisterContent[MSstate.RegSet.ALUin]);
                uint B = unchecked ( (uint) state.RegisterContent[RWsource]);
                uint uiResult = A + B;

                // adjust the result as necessary to fit into a byte
                bCarry = false; // assume this until proven wrong
                byte byteResult = 0x0; // to receive the arithmetic result

                if (uiResult == 0) // easy case
                {
                    byteResult = unchecked((byte)uiResult);
                    bPos = bNeg = false;
                    bZero = true;
                }

                else if (uiResult < 128) // positive number, no carry
                {
                    byteResult = unchecked((byte)uiResult);
                    bNeg = bZero = false;
                    bPos = true;
                }

                else if (uiResult < 256) // negative number, no carry
                {
                    byteResult = unchecked((byte)uiResult);
                    bPos = bZero = false;
                    bNeg = true;
                }

                else // positive number with carry
                {
                    byteResult = unchecked((byte) (uiResult % 256)); //truncate
                    bNeg = bZero = false;
                    bPos = bCarry = true;
                }

                PerformAssign(RWdestination, byteResult);
                return mseq.GetJump(mc);
            } // alu:AandB

            // TODO: more ALU selections

            return false;
        } // PerformOneMicroStatement

        // Assignment when destination is given as a state index and 
        //    source is given as an integer
        private void PerformAssign(MSstate.RegSet destination, byte source)
        {
            // perform the assignment
            state.RegisterContent[destination] = source;
        }

        /// <summary>
        /// The next two methods are a bit subtle. The actual hardware increment for PC or MAR increments the
        ///    entire register (four 74LS193 counter chips). That is, if PClo is 255, it becomes 0 on increment and PChi is incremented by one.
        ///    This method emulates that hardware action. Note that incrementing PC and/or MAR has no effect
        ///    on the status word.
        /// </summary>
        /// <returns>Returns true.</returns>
        private bool PerformIncPC()
        {
            if (state.RegisterContent[MSstate.RegSet.PClo] == unchecked((byte)0xFF))
            {
                state.RegisterContent[MSstate.RegSet.PClo] = 0;
                state.RegisterContent[MSstate.RegSet.PChi] += 1;
            }
            else
                state.RegisterContent[MSstate.RegSet.PClo] += 1;
            return true;
        }

        /// <summary> See prior method. </summary>
        private bool PerformIncMAR()
        {
            if (state.RegisterContent[MSstate.RegSet.MARlo] == unchecked ( (byte) 0xFF))
            {
                state.RegisterContent[MSstate.RegSet.MARlo] = 0;
                state.RegisterContent[MSstate.RegSet.MARhi] += 1;
            }
            else
                state.RegisterContent[MSstate.RegSet.MARlo] += 1;
            return true;
        }

        // This is called after an assignment or ALU operation. 
        // result is the assigned or calculated value.
        // Note that bNOT_CDAV is controlled by events other than assignments
        //    and ALU operations.
        public bool SetStatusAndJump(byte result, MSmicrocodeStatement mc)
        {
            SetStatus(result);
            return mseq.GetJump(mc);
        }

        public void SetStatus(byte result) // TODO Test this since the change to byte from sbyte!
        {
            sbyte sResult = unchecked((sbyte)state.RegisterContent[MSstate.RegSet.ALUin]);

            bPos = sResult > 0;
            bZero = sResult == 0;
            bNeg = sResult < 0;
            bCarry = false;
        }
        public override string ToString()
        {
            // Print status contents
            string ret = " (NOTCd,Co,PZN): " + (bNOT_CDAV ? "1" : "0") + (bCarry ? "1" : "0")
                + (bPos ? "1" : "0") + (bZero ? "1" : "0") + (bNeg ? "1" : "0")
                + System.Environment.NewLine;

            return ret;
        }


    }
}
