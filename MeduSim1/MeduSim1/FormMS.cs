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
///    book. The Principia holds the copyright to the book and this application.
///    
/// Revision: Jan  6, 2017 - rough design ideas, built, and ran OK
/// 
/// Time log:
/// Initial design and code: 6 hr (Jan 4-6, 2017)
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

            // Show the initial state
            tbOut.Text += state.ToString() + System.Environment.NewLine + System.Environment.NewLine;

            // Make a few state changes
            state.RegisterContent[MSstate.RegSet.MARlo] = 0x11;
            state.RegisterContent[MSstate.RegSet.MARhi] = 0x22;
            state.RegisterContent[MSstate.RegSet.AClo] = 0x33;
            state.RegisterContent[MSstate.RegSet.AChi] = 0x42;

            // Show the results
            tbOut.Text += state.ToString() + System.Environment.NewLine + System.Environment.NewLine;

            // Execute some microcode
            MSmicrosequencer mseq = MSmicrosequencer.Get_MSmicrosequencer();
            mseq.PerformOneMicroStatement(new MSmicrocodeStatement("", 4, 3, 0, 0, 0));
            mseq.PerformOneMicroStatement(new MSmicrocodeStatement("", 5, 2, 0, 0, 0));

            // Show the results
            tbOut.Text += state.ToString() + System.Environment.NewLine + System.Environment.NewLine;
        }
    }
}
