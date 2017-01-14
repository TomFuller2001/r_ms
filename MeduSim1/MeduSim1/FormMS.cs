using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/// <summary>
/// This application will simulate Tom Holzberlein's Medusa computer.
/// It includes simulated state, microcode execution, op code simulation,
///    and creates test files that can be incorporated into Tom's textbook.
///    
/// Author: Tom Fuller
/// 
/// Copyright: This project is a derivative work based on Tom Holzberlein's 
///    book. The Principia holds the copyright to the book and to this application.
///    
/// Revision: Jan  7, 2017 - rough design ideas, built, and ran OK
/// 
/// Time log:
/// Initial design and code: 12 hr (Jan 4-11, 2017)
/// 
/// Open design questions.
/// 1. We use the Dictionary in MSstate for clean ToString() code and some simplicity in the ordering of the registers. 
///       Should we give it up for a simple array?
///       
/// 2. We use int throughout for 8-bit registers. These do not behave precisely as 8-bit registers (rollover at 255, e.g.).
///       Should be we use the C# sbyte instead? However we may still run into problems with incrementing counters (255 + 1)
///       We may need to handle MAR, PC, SP differently.
///       
/// 3. The microsequencer and ALU seem too entangled. Both have responsibilities for executing microcode.
///       Should we either combine them, or have one or the other handle micro-execution exclusively?
/// </summary>


namespace MeduSim1
{
    public partial class Form_MS : Form
    {
        public Form_MS()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Just testing to see how things work.
        /// TODO We need a facade to manage the simulation.
        /// </summary>
        private void btnStart_Click(object sender, EventArgs e)
        {
            MSstate state = MSstate.Get_MSstate();
#if false
            // Show the initial state
            tbOut.Text += state.ToString() + Environment.NewLine + Environment.NewLine;
#endif
            // Make a few state changes
            state.RegisterContent[MSstate.RegSet.MARlo] = 0x11;
            state.RegisterContent[MSstate.RegSet.MARhi] = 0x22;
            state.RegisterContent[MSstate.RegSet.AClo] = 0x50;
            state.RegisterContent[MSstate.RegSet.AChi] = 0x42;
            state.RegisterContent[MSstate.RegSet.SPhi] = 0x50;

            // Show the results
            tbOut.Text += state.ToString() + Environment.NewLine + Environment.NewLine;

            // Execute some microcode
            MS_ALU alu = MS_ALU.Get_MS_ALU();
            alu.PerformOneMicroStatement(new MSmicrocodeStatement("", 13, 9, 0, 0, 0)); // ALUin := SPhi
            tbOut.Text += state.ToString() + Environment.NewLine + Environment.NewLine;

            alu.PerformOneMicroStatement(new MSmicrocodeStatement("", 5, 2, 0, 0, 0)); // BRhi := AClo
            alu.PerformOneMicroStatement(new MSmicrocodeStatement("", 14, 0, 0, 0, 0)); // Inc MAR&PC // TODO not working
            alu.PerformOneMicroStatement(new MSmicrocodeStatement("", 8, 9, 7, 0, 0)); // SPlo := SPhi+ALUin 50+50=A0

            // Show the results
            tbOut.Text += state.ToString() + Environment.NewLine + Environment.NewLine;
        }
    }
}
