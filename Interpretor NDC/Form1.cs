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
        string beginMessage = "[";
        string endMessage = "]";
        string startSeparatorMessage = "-->";
        string stopSeparatorMessage = "<--";
        int skipChar = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            if (pathFile == "" || pathFile == null || pathFile == "MessageIn.log")
                return;

            StreamReader sr = new StreamReader(pathFile);

            if (!Directory.Exists(@"SplitIn"))
                Directory.CreateDirectory("SplitIn");
            FileInfo f = new FileInfo(@"SplitIn\MessageIn.anl");
            StreamWriter writer = f.CreateText(); 

            string line = sr.ReadLine();

            int nrMessage = 0;
            bool multiline = false;
            while(line!=null)
            {
                if (multiline == true)    // multiline mesaj
                {
                    string message = line;
                    if (!message.Contains(endMessage)) // nu am ajuns la sfarsitul mesajului
                        writer.WriteLine(message);
                    else                                // sfarsitul mesajului multiline
                    {
                        multiline = false;
                        int stopMessaj = line.IndexOf(endMessage);
                        message = line.Substring(0, stopMessaj);
                        writer.WriteLine(message);
                        writer.WriteLine(stopSeparatorMessage);
                    }
                }
                else if( line.Substring(skipChar).Contains(beginMessage) && line.Substring(skipChar).Contains(endMessage) )  // mesaj de o singura linie
                {
                    multiline = false;
                    int startMesaj = line.IndexOf(beginMessage, skipChar) + beginMessage.Length;
                    int stopMesaj = line.IndexOf(endMessage, skipChar);
                    string message = line.Substring(startMesaj, stopMesaj - startMesaj);
                    writer.Write(startSeparatorMessage); writer.Write("Ms"); writer.WriteLine(nrMessage); nrMessage++;
                    writer.WriteLine(message);
                    writer.WriteLine(stopSeparatorMessage);
                }
                else if (line.Substring(skipChar).Contains(beginMessage) && !line.Substring(skipChar).Contains(endMessage))    // mesaj cu mai multe linii, asta e prima linie
                {
                    multiline = true;
                    int startMesaj = line.IndexOf(beginMessage) + beginMessage.Length;
                    string message = line.Substring(startMesaj);
                    writer.Write(startSeparatorMessage); writer.Write("Ms"); writer.WriteLine(nrMessage); nrMessage++;
                    writer.WriteLine(message);
                }            

                line = sr.ReadLine();
            }

            sr.Close();
            writer.Close();

            
            sr = new StreamReader(@"SplitIn\MessageIn.anl");

            line = sr.ReadLine();
            textMessage = new string[nrMessage+1];
            int i = 0;
            listBoxMessageIn.Items.Clear();
            while(i < nrMessage)
            {
                if (line.Contains(startSeparatorMessage))
                {
                    string itemsStr = "Ms" + (i+1).ToString();
                    listBoxMessageIn.Items.Add(itemsStr);
                    textMessage[i] = "";
                }
                else if (line.Contains(stopSeparatorMessage))
                    i++;
                else
                {
                    textMessage[i] += line;
                }
                line = sr.ReadLine();
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
            StreamReader sr = new StreamReader(@"SplitIn\MessageIn.anl");
            sr.BaseStream.Position = 0;

            if (checkBoxTerminalCommands.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");
                
                FileInfo f = new FileInfo(@"SplitIn\Terminal Commands.anl");
                StreamWriter writer = f.CreateText(); 

                string line = sr.ReadLine();

                while (line != null)
                {
                    if (line != "")
                    {
                        if (line[0] == '1')
                            writer.WriteLine(line);
                    }
                    
                    line = sr.ReadLine();
                }
                writer.Close();
            }
            sr.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxInceputulMesajului.Text = beginMessage;
            textBoxSfarsitulMesajului.Text = endMessage;
        }

        private void textBoxInceputulMesajului_TextChanged(object sender, EventArgs e)
        {
            beginMessage = textBoxInceputulMesajului.Text;
        }

        private void textBoxSfarsitulMesajului_TextChanged(object sender, EventArgs e)
        {
            endMessage = textBoxSfarsitulMesajului.Text;
        }

        private void textBoxSariPestePrimele_TextChanged(object sender, EventArgs e)
        {
            skipChar = Convert.ToInt32(textBoxSariPestePrimele.Text);
        }
    }
}