using System;
using System.Collections.Generic;
using System.Text;

namespace Interpretor_NDC
{
    // State tables load
    class ScreenKeyDataLoad : CustomisationDataCommands
    {
        public string[] ScreenNumber = null;
        public string[] ScreenData = null;
        public string MessageAuthenticationCode = "";

        public ScreenKeyDataLoad(string str)
            : base(str)
        {
            Name = "Screen/Key Load";

            int sep = Utils.StrIndexOf((char)28, str, 3);
            // nurara Screen-urile transmise
            int nrScreen = Utils.StrCount((char)28, str, sep + 1);

            // nu exista Screen-uri
            if (nrScreen < 0)
            {
                ScreenNumber = new string[1];
                ScreenData = new string[1];
                ScreenNumber[0] = "ERROR";
                ScreenData[0] = "ERROR";
            }
            else
            {
                ScreenNumber = new string[nrScreen];
                ScreenData = new string[nrScreen];
                // load state
                for (int i = 0; i < ScreenData.Length; i++)
                {
                    ScreenNumber[i] = str.Substring(sep + 1, 3);
                    int lengthState = str.IndexOf((char)28, sep + 1) - sep - 1 - 3;
                    ScreenData[i] = str.Substring(sep + 1 + 3, lengthState);
                    sep = str.IndexOf((char)28, sep + 1);
                }
            }
            //FS
            // Optional
            if (str.Length - 1 < sep + 1) // afara din string
                return;

            //MessageAuthenticationCode = str.Substring(sep + 1, 8);
            //Trailer = str.Substring(sep + 1 + 8);
        }
    }
}
