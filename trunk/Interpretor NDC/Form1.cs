using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Interpretor_NDC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EnhancedConfigurationParametersLoad t = new EnhancedConfigurationParametersLoad("3∟∟∟1A∟∟0000201002030020600109002100021100212001130011400115001∟00010010100200503040040150502006030070050802009010960059801099005", 13);
            Console.WriteLine(t.ResponseFlag);
            //t.Configure("1 ∟123∟456∟02n");

        }
    }
}