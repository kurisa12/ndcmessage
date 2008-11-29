using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;


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
        DrawState[] DS = null;


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

            comboBoxViewStateTabel.Text = "ALL";
            StreamReader sr = new StreamReader(pathFile);
            string line = sr.ReadLine();
            listBoxStateTabels.Items.Clear();

            StateTable.ListOfStateTables.Clear();
            
            while( line != null )
            {
                StateTable.ListOfStateTables.Add(new StateTable(line));
                listBoxStateTabels.Items.Add(line);
                line = sr.ReadLine();
            }

        }

        private void listBoxStateTabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            StateTable st = new StateTable(listBoxStateTabels.Items[listBoxStateTabels.SelectedIndex].ToString());
            st.particulariseState(@"C:\Config.xml");

            label1.Text = st.namePart[0];
            label2.Text = st.namePart[1];
            label3.Text = st.namePart[2];
            label4.Text = st.namePart[3];
            label5.Text = st.namePart[4];
            label6.Text = st.namePart[5];
            label7.Text = st.namePart[6];
            label8.Text = st.namePart[7];
            label9.Text = st.namePart[8];
            label10.Text = st.namePart[9];
            // ext
            label11.Text = st.namePart[10];
            label12.Text = st.namePart[11];
            label13.Text = st.namePart[12];
            label14.Text = st.namePart[13];
            label15.Text = st.namePart[14];
            label16.Text = st.namePart[15];
            label17.Text = st.namePart[16];
            label18.Text = st.namePart[17];
            label19.Text = st.namePart[18];
            label20.Text = st.namePart[19];            
            // ext 2
            label21.Text = st.namePart[20];
            label22.Text = st.namePart[21];
            label23.Text = st.namePart[22];
            label24.Text = st.namePart[23];
            label25.Text = st.namePart[24];
            label26.Text = st.namePart[25];
            label27.Text = st.namePart[26];
            label28.Text = st.namePart[27];
            label29.Text = st.namePart[28];
            label30.Text = st.namePart[29];
            
            
            textBox1.Text = st.part[0];
            textBox2.Text = st.part[1];
            textBox3.Text = st.part[2];
            textBox4.Text = st.part[3];
            textBox5.Text = st.part[4];
            textBox6.Text = st.part[5];
            textBox7.Text = st.part[6];
            textBox8.Text = st.part[7];
            textBox9.Text = st.part[8];
            textBox10.Text = st.part[9];
            //
            textBox11.Text = st.part[10];
            textBox12.Text = st.part[11];
            textBox13.Text = st.part[12];
            textBox14.Text = st.part[13];
            textBox15.Text = st.part[14];
            textBox16.Text = st.part[15];
            textBox17.Text = st.part[16];
            textBox18.Text = st.part[17];
            textBox19.Text = st.part[18];
            textBox20.Text = st.part[19];
            //
            textBox21.Text = st.part[20];
            textBox22.Text = st.part[21];
            textBox23.Text = st.part[22];
            textBox24.Text = st.part[23];
            textBox25.Text = st.part[24];
            textBox26.Text = st.part[25];
            textBox27.Text = st.part[26];
            textBox28.Text = st.part[27];
            textBox29.Text = st.part[28];
            textBox30.Text = st.part[29];

            textBoxDescriptionPart1.Text = st.descriptionPart[0];
            textBoxDescriptionPart2.Text = st.descriptionPart[1];
            textBoxDescriptionPart3.Text = st.descriptionPart[2];
            textBoxDescriptionPart4.Text = st.descriptionPart[3];
            textBoxDescriptionPart5.Text = st.descriptionPart[4];
            textBoxDescriptionPart6.Text = st.descriptionPart[5];
            textBoxDescriptionPart7.Text = st.descriptionPart[6];
            textBoxDescriptionPart8.Text = st.descriptionPart[7];
            textBoxDescriptionPart9.Text = st.descriptionPart[8];
            textBoxDescriptionPart10.Text = st.descriptionPart[9];
            //
            textBoxDescriptionPart11.Text = st.descriptionPart[10];
            textBoxDescriptionPart12.Text = st.descriptionPart[11];
            textBoxDescriptionPart13.Text = st.descriptionPart[12];
            textBoxDescriptionPart14.Text = st.descriptionPart[13];
            textBoxDescriptionPart15.Text = st.descriptionPart[14];
            textBoxDescriptionPart16.Text = st.descriptionPart[15];
            textBoxDescriptionPart17.Text = st.descriptionPart[16];
            textBoxDescriptionPart18.Text = st.descriptionPart[17];
            textBoxDescriptionPart19.Text = st.descriptionPart[18];
            textBoxDescriptionPart20.Text = st.descriptionPart[19];
            //
            textBoxDescriptionPart21.Text = st.descriptionPart[20];
            textBoxDescriptionPart22.Text = st.descriptionPart[21];
            textBoxDescriptionPart23.Text = st.descriptionPart[22];
            textBoxDescriptionPart24.Text = st.descriptionPart[23];
            textBoxDescriptionPart25.Text = st.descriptionPart[24];
            textBoxDescriptionPart26.Text = st.descriptionPart[25];
            textBoxDescriptionPart27.Text = st.descriptionPart[26];
            textBoxDescriptionPart28.Text = st.descriptionPart[27];
            textBoxDescriptionPart29.Text = st.descriptionPart[28];
            textBoxDescriptionPart30.Text = st.descriptionPart[29];

            // conditie de deschidere
            if (label11.Text != "")
                groupBox2.Visible = true;
            else
                groupBox2.Visible = false;
            //
            if (label21.Text != "")
                groupBox3.Visible = true;
            else
                groupBox3.Visible = false;
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
            FileInfo f = new FileInfo(@"State Tables.xml");
            StreamWriter writer = f.CreateText();

            writer.WriteLine("<STATES>");
            for (int i = 0; i < listBoxStateTabels.Items.Count; i++)
            {
                StateTable temp = new StateTable(listBoxStateTabels.Items[i].ToString());
                temp.SaveToXML(writer);
            }
            writer.WriteLine("</STATES>");
            writer.Close();
        }

        private void buttonClearMap_Click(object sender, EventArgs e)
        {
            if (DS != null)
                for (int i = 0; i < DS.Length; i++)
                    DS[i].Dispose();
            DS = null;

            DS = new DrawState[2];
            DS[0] = new DrawState(new Point(0, 0), new Size(100, 40), "A");
            DS[1] = new DrawState(new Point(10, 2000), new Size(100, 40), "B");

            this.panelDrawStates.Controls.Add(DS[0]);
            this.panelDrawStates.Controls.Add(DS[1]);

            panelDrawStates.Refresh();

        }

        private void panelDrawStates_Paint(object sender, PaintEventArgs e)
        {
            Graphics G = e.Graphics;

            Point P = this.AutoScrollPosition;
            Rectangle dstR = this.ClientRectangle;
            RectangleF srcR = new RectangleF(-100, -200, dstR.Width, dstR.Height);
            G.DrawLine(new Pen(Color.Black, 1), -10, 0, 100, 100);
            G.DrawLine(new Pen(Color.Black, 1), 100, 100, 1000, 0);
            
           if (DS == null)
                return;
            if (DS.Length < 1 ) // nu sunt elemente 
                return;
            
            
            Graphics g = e.Graphics;
            for (int i = 0; i < DS.Length; i++ )
            {
                DrawState temp = DS[i];
                //temp.Draw("", e);
            }

            //panelDrawStates.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DS == null)
                return;
           
            for (int i = 0; i < DS.Length; i++)
            {
                DrawState temp = DS[i];
                SizeF factor = new SizeF(2.0f, 2.0f);
                DS[i].Scale(factor);
               // temp.Draw(e);
            }
        }

        private void panelDrawStates_Scroll(object sender, ScrollEventArgs e)
        {
            //panelDrawStates.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (DS == null)
                return;

            for (int i = 0; i < DS.Length; i++)
            {
                DrawState temp = DS[i];
                SizeF factor = new SizeF(0.5f, 0.5f);
                DS[i].Scale(factor);
                // temp.Draw(e);
            }
        }

    };

    class DrawState : Button
    {
        string strState = "";

        public DrawState( Point P1, Size fullSize, string str )
        {
            /*elementContiner = new Area(P1, P2, "dreptunghi");
            viewState = new Area(new Point(P1.X+30, P1.Y+10), new Point(P2.X - 30, P2.Y - 10), "dreptunghi");

            Point temp1 = new Point(viewState.P1.X-10, viewState.P1.Y);
            Point temp2 = new Point(viewState.P1.X, viewState.P1.Y);
            inPoint = new Area(temp1, temp2, "linie");

            temp1 = new Point(viewState.P2.X + 10, viewState.P2.Y);
            temp2 = new Point(viewState.P2.X, viewState.P2.Y);
            outPoint = new Area(temp1, temp2, "linie");*/

            strState = str;

            this.Location = new System.Drawing.Point(P1.X, P1.Y);
            this.Name = str;
            this.Size = fullSize;
            this.Visible = true;
            //this.Paint += System.Windows.Forms.PaintEventHandler(this.Draw2());

            this.BackColor = System.Drawing.Color.Transparent;
            this.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.FlatAppearance.BorderSize = 0;
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            //this.button2.Name = "button2";
            //this.button2.Size = new System.Drawing.Size(40, 20);
            //this.button2.TabIndex = 34;
            this.Text = str;
            this.UseVisualStyleBackColor = false;

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.buttonGenerateMap_Paint);
        }

        ~DrawState()
        {

        }


        public void buttonGenerateMap_Paint(object sender, PaintEventArgs e)
        {
            DrawState b = (DrawState)sender;
            Graphics g = e.Graphics;
            //g.DrawRectangle(elementContiner._Pen, elementContiner.P1.X, elementContiner.P1.Y, elementContiner.P2.X - elementContiner.P1.X, elementContiner.P2.Y - elementContiner.P1.Y);
            g.DrawRectangle(new Pen(Color.Black, 1), b.Location.X + b.Size.Width / 3, b.Location.Y + b.Size.Height / 3, b.Size.Width / 3, b.Size.Height / 3);
            g.DrawLine(new Pen(Color.Black, 1), b.Location.X + b.Size.Width / 3, b.Location.Y + b.Size.Height / 3, b.Location.X, b.Location.Y);
            //g.DrawLine(outPoint._Pen, outPoint.P1.X, outPoint.P1.Y, outPoint.P2.X, outPoint.P2.Y);
            g.DrawString(b.strState, new Font("Arial", 7), new LinearGradientBrush(new Point(0, 0), new Point(1, 1), Color.Black, Color.Black), b.Location.X, b.Location.Y + 2);

            //this.Refresh();
        }
    };

    
}
