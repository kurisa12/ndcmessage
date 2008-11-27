using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;



using System.Drawing.Drawing2D;

namespace Interpretor_NDC
{
    public partial class Form1 : Form
    {
        string[] textMessage = null;
        string beginMessage = "[";
        string endMessage = "]";
        string startSeparatorMessage = "{<(--";
        string stopSeparatorMessage = "--)]}";
        int skipChar = 0;
        string pathFileStateTableTDB = "";
        ButtonState[] BS = null;

        public Form1()
        {
            InitializeComponent();
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
            if (textBoxSariPestePrimele.Text == "")
                skipChar = 0;
            skipChar = Convert.ToInt32(textBoxSariPestePrimele.Text);
        }

        private void buttonArataLog_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.FilterIndex = 0;
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            if (pathFile == "" || pathFile == null )
                return;

            StreamReader sr = new StreamReader(pathFile);
            string line = sr.ReadLine();
            textBoxArataLog.Text = "";
            while (line != null)
            {
                // precalculeaza hederul de mesaj(data si ora sau ora si numar de mesaj sau ce mai e....)
                if (line != "")
                {
                    if (line[0] == '[')
                    {
                        int lungimeHeder = line.IndexOf('[', 1)-1;
                        if (lungimeHeder > 0)
                            textBoxSariPestePrimele.Text = lungimeHeder.ToString();
                    }
                    else
                    {
                        int lungimeHeder = line.IndexOf('[', 1)-1;
                        if (lungimeHeder > 0)
                            textBoxSariPestePrimele.Text = lungimeHeder.ToString();
                    }
                }

                if (textBoxArataLog.Text.Length >= 2000)
                {
                    textBoxArataLog.Text += "---Out of text(2000)---\r\n";
                    return;
                }
                textBoxArataLog.Text += line + "\r\n";
                line = sr.ReadLine();
            }
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.FilterIndex = 1;
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            if (pathFile == "" || pathFile == null )
                return;

            StreamReader sr = new StreamReader(pathFile);

            if (!Directory.Exists(@"SplitIn"))
                Directory.CreateDirectory("SplitIn");
            FileInfo f = new FileInfo(@"SplitIn\MessageIn.anl");
            StreamWriter writer = f.CreateText(); 

            string line = sr.ReadLine();

            int nrMessage = 0;
            bool multiline = false;
            
            while( line!=null )
            {
                if( multiline == false && line.Length < skipChar + 1 )       // elimina eventualele spatii dintre mesaje
                    ;
                else if( multiline == true )    // multiline mesaj
                {
                    string message = line;
                    if( !message.Contains(endMessage) ) // nu am ajuns la sfarsitul mesajului
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
                    string hederMessage = line.Substring(0, startMesaj-1);
                    writer.WriteLine("{0}{1} MsIn{2}", startSeparatorMessage, hederMessage, nrMessage); nrMessage++;
                    writer.WriteLine(message);
                    writer.WriteLine(stopSeparatorMessage);
                }
                else if( line.Substring(skipChar).Contains(beginMessage) && !line.Substring(skipChar).Contains(endMessage) )    // mesaj cu mai multe linii, asta e prima linie
                {
                    multiline = true;
                    int startMesaj = line.IndexOf(beginMessage, skipChar) + beginMessage.Length;
                    string message = line.Substring(startMesaj);
                    string hederMessage = line.Substring(0, startMesaj - 1);
                    writer.WriteLine("{0}{1} MsIn{2}", startSeparatorMessage, hederMessage, nrMessage); nrMessage++;
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
            while( i < nrMessage )
            {
                if( line.Length < startSeparatorMessage.Length || line.Length < stopSeparatorMessage.Length )
                    textMessage[i] += line;
                else if( line.Substring(0,startSeparatorMessage.Length).Contains(startSeparatorMessage) )
                {
                    string itemsStr = "Ms" + (i+1).ToString();
                    listBoxMessageIn.Items.Add(itemsStr);
                    textMessage[i] = "";
                }
                else if (line.Substring(0, startSeparatorMessage.Length).Contains(stopSeparatorMessage))
                    i++;
                else
                    textMessage[i] += line;
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
            
            // terminal commands
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
                    if (line.Length > 5)
                    {
                        if (line[0] == '1' && line[2] == (char)28)
                            writer.WriteLine(line);
                    }
                    
                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // screen/keyboard data 
            sr.BaseStream.Position = 0;
            if (checkBoxScreenKeyboard.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Screen Keyboard.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == '1')  // 3.....11
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // state tables
            sr.BaseStream.Position = 0;
            if (checkBoxStateTables.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\State Tables.anl");
                StreamWriter writer = f.CreateText();

                FileInfo stateDB = new FileInfo(@"SplitIn\State Tables.tdb");
                StreamWriter writer2 = stateDB.CreateText();
                writer2.Close();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == '2')  // 3.....12
                        {
                            writer.WriteLine(line);
                            StateTablesLoad stl = new StateTablesLoad(line);
                            stl.SaveToFile(@"SplitIn\State Tables.tdb");
                        }
                    }

                    line = sr.ReadLine();
                }
                
                writer.Close();
            }

            // configuration parameters
            sr.BaseStream.Position = 0;
            if (checkBoxConfigurationParameters.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Configuration Parameters.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == '3')  // 3.....13
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // FIT data
            sr.BaseStream.Position = 0;
            if (checkBoxFIT.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\FIT.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == '5')  // 3.....15
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Configuration ID Number Load
            sr.BaseStream.Position = 0;
            if (checkBoxConfigurationIDNumber.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Configuration ID Number.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == '6')  // 3.....16
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Enhanced Configuration Parameters Load
            sr.BaseStream.Position = 0;
            if (checkBox1EnhancedConfigurationParameters.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Enhanced Configuration Parameters.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == 'A')  // 3.....16
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // MAC Field Selection Load
            sr.BaseStream.Position = 0;
            if (checkBoxMACFieldSelection.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\MAC Field Selection.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == 'B')  // 3.....1B
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Date and Time
            sr.BaseStream.Position = 0;
            if (checkBoxDateAndTime.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Date and Time.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == 'C')  // 3.....1C
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Dispenser Currency Cassette Mapping Table
            sr.BaseStream.Position = 0;
            if (checkBoxDispencerCurrencyCassetteMapp.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Dispenser Currency Cassette Mapping Table.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == 'E')  // 3.....1E
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // XML Configuration Download
            sr.BaseStream.Position = 0;
            if (checkBoxXMLConfigurationDownload.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\XML Configuration Download.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '1' && line[posSep3 + 2] == 'I')  // 3.....1I
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Interactive Transaction Response
            sr.BaseStream.Position = 0;
            if (checkBoxInteractiveTransactionResponse.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Interactive Transaction Response.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '2' && (line[posSep3 + 2] >= '0' && line[posSep3 + 2] <= '9') )  // 3.....2(0-9)
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Encryption Key Change
            sr.BaseStream.Position = 0;
            if (checkBoxEncryptionKeyChange.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Encryption Key Change.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '3' && (line[posSep3 + 2] >= '1' && line[posSep3 + 2] <= '9'))  // 3.....3(1-9)
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Extended Encryption Key Change
            sr.BaseStream.Position = 0;
            if (checkBoxExtendedEncryptionKeyChange.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Extended Encryption Key Change.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '3' && line[posSep3 + 1] == '3' && ((line[posSep3 + 2] >= '1' && line[posSep3 + 2] <= '9') || (line[posSep3 + 2] >= 'A' && line[posSep3 + 2] <= 'K')))  // 4.....2(1-9)|(A-K)
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // HostToExitMessages
            sr.BaseStream.Position = 0;
            if (checkBoxHostToExitMessages.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Host to Exit Messages.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                while (line != null)
                {
                    if (line.Length > 5)
                    {
                        int posSep3 = Utils.StrIndexOf((char)28, line, 1);
                        if (posSep3 > 0 && line[0] == '7' && line[posSep3 + 1] == '1')  // 7.....1
                            writer.WriteLine(line);
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            // Transaction Reply Command
            sr.BaseStream.Position = 0;
            if (checkBoxTransactionReplyCommand.Checked == true)
            {
                if (!Directory.Exists(@"SplitIn"))
                    Directory.CreateDirectory("SplitIn");

                FileInfo f = new FileInfo(@"SplitIn\Transaction Reply Command.anl");
                StreamWriter writer = f.CreateText();

                string line = sr.ReadLine();
                bool mutiline = false;

                while (line != null)
                {
                    if (mutiline == true)
                    {
                        if (line.Contains(stopSeparatorMessage) && line[0] == stopSeparatorMessage[0]) // sfarsitul mesajului
                        {
                            mutiline = false;
                            writer.WriteLine(stopSeparatorMessage);
                        }
                        else
                            writer.WriteLine(line);
                    }

                    if (line.Length > 5)
                    {

                        int posSep3 = Utils.StrIndexOf((char)28, line, 2);
                        if (posSep3 > 0 && line[0] == '4')  // 4.....
                        {
                            mutiline = true;
                            writer.WriteLine(startSeparatorMessage);
                            writer.WriteLine(line);
                        }
                    }

                    line = sr.ReadLine();
                }
                writer.Close();
            }

            sr.Close();
        }

        private void buttonOpenStateTabels_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.FilterIndex = 2;
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            pathFileStateTableTDB = pathFile;

            if (pathFile == "" || pathFile == null )
                return;

            StreamReader sr = new StreamReader(pathFile);
            string line = sr.ReadLine();
            listBoxStateTabels.Items.Clear();
            comboBoxViewStateTabel.Text = "ALL";
            while( line != null )
            {
                listBoxStateTabels.Items.Add(line);
                line = sr.ReadLine();
            }

        }

        private void listBoxStateTabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            StateTable st = new StateTable(listBoxStateTabels.Items[listBoxStateTabels.SelectedIndex].ToString());

            textBoxMessType.Text = st.Type.ToString();

            label1.Text = st.numePart1;
            label2.Text = st.numePart2;
            label3.Text = st.numePart3;
            label4.Text = st.numePart4;
            label5.Text = st.numePart5;
            label6.Text = st.numePart6;
            label7.Text = st.numePart7;
            label8.Text = st.numePart8;
            textBox1.Text = st.part1;
            textBox2.Text = st.part2;
            textBox3.Text = st.part3;
            textBox4.Text = st.part4;
            textBox5.Text = st.part5;
            textBox6.Text = st.part6;
            textBox7.Text = st.part7;
            textBox8.Text = st.part8;
            
            textBoxDescriptionStateType.Text = st.descriptionType;
            textBoxDescriptionPart1.Text = st.descriprionPart1;
            textBoxDescriptionPart2.Text = st.descriprionPart2;
            textBoxDescriptionPart3.Text = st.descriprionPart3;
            textBoxDescriptionPart4.Text = st.descriprionPart4;
            textBoxDescriptionPart5.Text = st.descriprionPart5;
            textBoxDescriptionPart6.Text = st.descriprionPart6;
            textBoxDescriptionPart7.Text = st.descriprionPart7;
            textBoxDescriptionPart8.Text = st.descriprionPart8;

            // generate all buttons
            // init
            int posX = 40;
            int posY = 40;
            // dispose
            if (BS != null)
                for (int i = 0; i < BS.Length; i++)
                    BS[i].Dispose();

            BS = new ButtonState[1];
            for (int i = 0; i < 1; i++)
            {
                BS[i] = new ButtonState(new Point(posX, posY), i, listBoxStateTabels.Items[i].ToString().Substring(0, 4), listBoxStateTabels.Items[i].ToString());
                if (posX >= 500)
                {
                    posY += 80;
                    posX = 40;
                    /*if (posY + 20 >= panelDrawStates.Size.Height)
                        panelDrawStates.Size = new System.Drawing.Size(620, posY + 20 +40);*/
                }
                else
                {
                    posX += 120;
                    //posY = 20;
                }

                this.panelDrawStates.Controls.Add(BS[i]);

                /*PaintEventArgs temp3 = new PaintEventArgs(null, new Rectangle());
                Graphics g = temp3.Graphics;
                Pen p = new Pen(Color.Black, 10);
                g.DrawLine(p, 25, 25, 375, 375);*/
            }
        }

        private void comboBoxViewStateTabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pathFileStateTableTDB == null || pathFileStateTableTDB == "" || !pathFileStateTableTDB.Contains(".tdb"))
                return;

            StreamReader sr = new StreamReader(pathFileStateTableTDB);
            string line = sr.ReadLine();
            listBoxStateTabels.Items.Clear();
            
            while (line != null)
            {
                if( comboBoxViewStateTabel.Text == "ALL" )
                    listBoxStateTabels.Items.Add(line);
                else
                {
                    if( line[3] == Convert.ToChar(comboBoxViewStateTabel.Text) )
                        listBoxStateTabels.Items.Add(line);
                }
                line = sr.ReadLine();
            }
        }

        private void buttonGenerateMap_Click(object sender, EventArgs e)
        {
            int nrOfStates = listBoxStateTabels.Items.Count;
            
            // generate all buttons
            // init
            int posX = 40;
            int posY = 40;
            // dispose
            if( BS != null )
                for (int i = 0; i < BS.Length; i++)
                    BS[i].Dispose();

            BS = new ButtonState[nrOfStates]; 
            for (int i = 0; i < nrOfStates; i++)
            {
                BS[i] = new ButtonState(new Point(posX, posY), i, listBoxStateTabels.Items[i].ToString().Substring(0, 4), listBoxStateTabels.Items[i].ToString());
                if (posX >= 500)
                {
                    posY += 80;
                    posX = 40;
                    /*if (posY + 20 >= panelDrawStates.Size.Height)
                        panelDrawStates.Size = new System.Drawing.Size(620, posY + 20 +40);*/
                }
                else
                {
                    posX += 120;
                    //posY = 20;
                }
                
                this.panelDrawStates.Controls.Add(BS[i]);

                /*PaintEventArgs temp3 = new PaintEventArgs(null, new Rectangle());
                Graphics g = temp3.Graphics;
                Pen p = new Pen(Color.Black, 10);
                g.DrawLine(p, 25, 25, 375, 375);*/
            }

            
        }

        private void buttonClearMap_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < BS.Length; i++ )
                BS[i].Dispose();
        }

    };


    class ButtonState : Button
    {
        public Point PointOut = new Point(20, 25);
        public Point PointIn = new Point(20, -5);

        public string stateText = "";

        public ButtonState(Point Location, int index, string textButton, string strText)
            : base()
        {
            this.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Location = new System.Drawing.Point(Location.X, Location.Y);
            this.Name = "buttonState"+index.ToString();
            this.Size = new System.Drawing.Size(40, 20);
            this.TabIndex = index;
            this.Text = textButton;
            this.UseVisualStyleBackColor = true;
            stateText = strText;
        }

        ~ButtonState()
        {
            
        }
    };
}
