using System;
using System.Collections.Generic;
using System.Text;

namespace Interpretor_NDC
{
    class CentralToTerminal
    {
        public char MessageClass = '0';
        public char ResponseFlag = '0';
        public string LUNO = "000";
        public string MessageSequenceNumber = "000";
        public string Name = "";
        public string MesajNDC = "";       // mesajul ndc care este procesat
        public string Trailer = "";
        // constr
        public CentralToTerminal(int neutilizat)
        {
            MessageClass = Convert.ToChar(MesajNDC[0]);
            if (MesajNDC[1] != '∟')
                ResponseFlag = Convert.ToChar(MesajNDC[1]);
        }
        public CentralToTerminal(string str)
        {
            int sep1 = str.IndexOf('∟', 0);
            int sep2 = str.IndexOf('∟', sep1 + 1);
            int sep3 = str.IndexOf('∟', sep2 + 1);
            MesajNDC = str;

            MessageClass = Convert.ToChar(MesajNDC[0]);
            if (MesajNDC[1] != '∟')
                ResponseFlag = Convert.ToChar(MesajNDC[1]);
            // optional
            if (str[1] != '∟')
                ResponseFlag = Convert.ToChar(str[1]);
            //FS
            if (sep1 + 1 < sep2)
                LUNO = str.Substring(sep1 + 1, 3);
            else
                LUNO = "";
            //FS
            if (sep2 + 1 < sep3)
                MessageSequenceNumber = str.Substring(sep2 + 1, 3);
            else
                MessageSequenceNumber = "";
            //FS        
        }
    };

    class TerminalComand : CentralToTerminal
    {
        public char CommandCode = '0';
        public char CommandModifier = '0';

        public TerminalComand(string str)
            : base(str)
        {
            int sep3 = Utils.StrIndexOf('∟', str, 2);
            //FS
            CommandCode = Convert.ToChar(str[sep3 + 1]);
            if (str.Length - 1 < sep3 + 2)
                return;
            // optional
            CommandModifier = Convert.ToChar(str[sep3 + 2]);
            Trailer = str.Substring(sep3 + 3);

            Name = "Terminal Command";
        }
    };
}