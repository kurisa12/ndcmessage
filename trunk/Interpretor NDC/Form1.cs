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
            StateTablesLoad t = new StateTablesLoad("3 ∟123∟456∟12∟000A601001012002002002001615∟001D400000000000000000000000∟002K011011011011615615615213∟");
            Console.WriteLine(t.ResponseFlag);
            //t.Configure("1 ∟123∟456∟02n");

        }
    }
}