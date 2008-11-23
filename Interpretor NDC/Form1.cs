using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Interpretor_NDC
{
    public partial class Form1 : Form
    {
        string[] textMessage = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            if (pathFile == "" || pathFile == null)
                return;

            FileStream inStream = File.OpenRead(pathFile);
            BinaryReader br = new BinaryReader(inStream);

            FileInfo f = new FileInfo("MessageIn.anl");
            StreamWriter writer = f.CreateText(); 

            char ch = br.ReadChar();
            char ch2;
            char ch3;
            char ch4;

            int nrLines = 0;

            while (br.BaseStream.Position < br.BaseStream.Length-1)
            {
                string message = "";

                if (ch == '3' || ch == '1' /*|| ch == '4'*/)                         // "3 ∟"
                {
                    ch2 = br.ReadChar();
                    ch3 = br.ReadChar();
                    if (ch2 == 32 && ch3 == (char)28)
                    {
                        message = Convert.ToString(ch) + Convert.ToString(ch2) + Convert.ToString(ch3);
                        ch = br.ReadChar();
                        while (true) // "] \r\n"
                        {
                            if (ch == ']')
                            {
                                ch2 = br.ReadChar();
                                ch3 = br.ReadChar();
                                ch4 = br.ReadChar();
                                if (ch2 == 32 && ch3 == '\r' && ch4 == '\n')
                                    break;
                                else
                                    br.BaseStream.Position -= 3;
                            }
                            message += Convert.ToString(ch);
                            ch = br.ReadChar();
                            
                        }
                        
                        //Console.WriteLine(message);
                        //writer.Write("Ms{0}->{1}", nrLines, message);
                        nrLines++;
                        //writer.Write("\r\n");
                        writer.WriteLine(message);
                    }
                    else
                        br.BaseStream.Position -= 2;
                    //dataGridViewMessageIn.Row
                }

                ch = br.ReadChar();
            }

            inStream.Close();
            writer.Close();


            StreamReader sr = new StreamReader("MessageIn.anl");

            textMessage = new string[nrLines];
            
            int i = 0;
            listBoxMessageIn.Items.Clear();
            while (i < nrLines)
            {
                /*string tempRemove = "Ms";
                tempRemove += Convert.ToString(i) + "->";*/
                textMessage[i] = sr.ReadLine();//.Replace( tempRemove, "" );
                i++;
                string idMess = "Ms" + Convert.ToString(i);
                listBoxMessageIn.Items.Add(idMess);
            }
            sr.Close();
        }

        private void listBoxMessageIn_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxMessageIn.Text = textMessage[listBoxMessageIn.SelectedIndex];
            int i = CentralToTerminal.FindMessageType(textBoxMessageIn.Text);

            CentralToTerminal ct = new CentralToTerminal(textBoxMessageIn.Text);

            switch (i)
            {
                case -1:
                    break;
                case 0:
                    ct = new TerminalComand(textBoxMessageIn.Text);
                    break;
                case 1:
                    ct = new ScreenKeyDataLoad(textBoxMessageIn.Text);
                    break;
                case 2:
                    ct = new StateTablesLoad(textBoxMessageIn.Text);
                    break;
            }
            //CentralToTerminal ct = new CentralToTerminal(textBoxMessageIn.Text);
        }

        private void buttonSplitSelect_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader("MessageIn.anl");
            sr.BaseStream.Position = 0;

            if (checkBoxTerminalCommands.Checked == true)
            {
                FileInfo f = new FileInfo("Terminal Commands.anl");
                StreamWriter writer = f.CreateText(); 

                string line = sr.ReadLine();
                int i = 0;
                while (line != null/*sr.BaseStream.Position < sr.BaseStream.Length-1*/ )
                {
                    if (line[0] == '1')
                    {
                        writer.WriteLine(line);
                    }
                    line = sr.ReadLine();
                }
                writer.Close();
            }
            sr.Close();
        }
    }
}