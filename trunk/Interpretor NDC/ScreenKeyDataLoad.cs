using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
            int nrScreen = Utils.StrCount((char)28, str, sep-1);

            // nu exista Screen-uri
            if (nrScreen < 0)
            {
                return;
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
                    if (lengthState < 0)
                        ScreenData[i] = str.Substring(sep + 1 + 3);
                    else
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

        public static void SaveToFile(string strDownloadLine, string pathToSave, bool append_file)
        {
            ScreenKeyDataLoad temp = new ScreenKeyDataLoad( strDownloadLine );
            if( append_file == true )
            {
                FileInfo stateDB = new FileInfo(pathToSave);
                if (!stateDB.Exists)
                    return;
                StreamWriter append = stateDB.AppendText();

                for (int i = 0; i < temp.ScreenData.Length; i++)
                {
                    append.Write(temp.ScreenNumber[i]); append.Write(" ");
                    append.WriteLine(temp.ScreenData[i]);
                }
                append.Close();
            }
            else
            {
                FileInfo stateDB = new FileInfo(pathToSave);
                StreamWriter append = stateDB.CreateText();

                for (int i = 0; i < temp.ScreenData.Length; i++)
                {
                    append.Write(temp.ScreenNumber[i]); append.Write(" ");
                    append.WriteLine(temp.ScreenData[i]);
                }
                append.Close();
            }
        }
    }
}
