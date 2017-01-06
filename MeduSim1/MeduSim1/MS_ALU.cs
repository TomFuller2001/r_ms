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
                    c_alu = new MS_ALU();

                // whether new or old, return a reference to the only instance
                return c_alu;
            } // lock
        }

        public void PerformOneMicroStatement(int alu, MSstate.RegSet destination, MSstate.RegSet source) { }

    }
}
