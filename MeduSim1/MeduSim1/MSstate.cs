using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeduSim1
{
    /// <summary>
    /// MSstate stores the contents of I/O and Data Path registers. It makes these available to other 
    ///    solution classes via a Dictionary of registers indexed by RegSet names. 
    ///    MSstate is a singleton class (Design Patterns in C# by Steven Metsker, Chapter 8)
    ///    since Medusa can only have one state at a time.
    /// </summary>
    class MSstate
    {
        // This enumerated type include registers themselves and several special items used in MScontrolTable (like nop and incPC).
        public enum RegSet {MARlo, MARhi, UART, AClo, AChi, BRlo, BRhi, PClo, PChi, SPlo, SPhi, ALUin, nop, MEM, incPC, incMAR, incPC_MAR };
        private const byte cFF = unchecked ( (byte) 0xFF); // default contents for registers at startup, FF
        // This list is used by ToString() below
        private RegSet[] aRegisters = { RegSet.UART, RegSet.AClo, RegSet.AChi, RegSet.BRlo,
            RegSet.BRhi, RegSet.PClo, RegSet.PChi, RegSet.SPlo, RegSet.SPhi, RegSet.ALUin };

        public Dictionary<RegSet, byte> RegisterContent;

        /// <summary>
        /// The single object instance for this class. The C# compiler
        ///    guarantees that this is initialized to null.
        /// </summary>
        private static MSstate c_state;

        /// <summary>
        /// To prevent access by more than one thread. This is the specific lock 
        /// belonging to the PO_NameFixer Class object.
        /// </summary>
        private static Object c_stateLock = typeof(MSstate);

        /// <summary>
        /// Instead of a constructor, we offer a static method to return the only instance.
        /// This is a private constructor so no one else can create instances of MSstate.
        /// </summary>
        private MSstate() { }

        /// <summary>
        /// Gets the only instance of the PO_NameFixer class that may exist.  If a single
        /// instance of PO_NameFixer has not yet been created, one is created.
        /// 
        /// PRE:    None.
        /// POST:   An MSstate (reference) has been returned, or created and returned.
        /// </summary>
        /// <returns>Returns a reference to the only existing instance of MSstate.</returns>
        public static MSstate Get_MSstate()
        {
            lock (c_stateLock)
            {
                // if this is the first request, initialize the one instance
                if (c_state == null)
                {
                    // create the single object instance
                    c_state = new MSstate();

                    c_state.RegisterContent = new Dictionary<RegSet, byte>();
                    c_state.LoadInitStates();
                } // if null

                // whether new or old, return a reference to the only instance
                return c_state;
            } // lock
        }
        private void LoadInitStates()
        {
            // first set everything to FF and then handle the exceptions
            foreach (RegSet rs in aRegisters)
                RegisterContent[rs] = cFF;

            // exceptions, corresponding to a system reset
            RegisterContent[RegSet.PClo]  =
            RegisterContent[RegSet.PChi]  =
            RegisterContent[RegSet.MARlo] =
            RegisterContent[RegSet.MARhi] = (byte)0;
        }

        public override string ToString ()
        {
            string ret = "";
            // ToString("X") alternatively prints the integer address as hexadecimal; this should also print the *contents* at the current MAR address TODO
            ret += string.Format("MAR:  {0,2} {1,2:}", RegisterContent[RegSet.MARhi].ToString("X"), RegisterContent[RegSet.MARlo].ToString("X"));
            ret += System.Environment.NewLine;

            // Print the remaining register contents
            foreach (RegSet rs in aRegisters)
                ret += string.Format("{0}:{1,2}  ", rs.ToString(), RegisterContent[rs].ToString("X"));

            // Add the state of the microsequencer
            ret += System.Environment.NewLine;
            ret += MSmicrosequencer.Get_MSmicrosequencer().ToString();

            // Add the status word from the ALU
            ret += MS_ALU.Get_MS_ALU().ToString();
            ret += System.Environment.NewLine;

            return ret;
        }
    }
}
