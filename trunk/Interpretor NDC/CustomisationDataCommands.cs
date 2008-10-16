// Clasa derivata din clasa de baza, care ar fi o clasa de baza pentru alte cateva clase
// Marian V.

using System;

namespace Interpretor_NDC
{
    class CustomisationDataCommands : CentralToTerminal
    {
        char MessageSubClass = '0';
        char Identifier = '0';
        public CustomisationDataCommands(string str)
            : base(str)
        {
            int sep1 = str.IndexOf('∟', 0);
            int sep2 = str.IndexOf('∟', sep1 + 1);
            int sep3 = str.IndexOf('∟', sep2 + 1);
            int len = str.Length;
            //FS
            MessageSubClass = Convert.ToChar(str[sep3 + 1]);
            Identifier = Convert.ToChar(str[sep3 + 2]);
        }
    };
}