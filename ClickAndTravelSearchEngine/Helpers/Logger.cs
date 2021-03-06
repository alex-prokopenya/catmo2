﻿ using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Web;
 using System.Text;
 using System.IO;

namespace ClickAndTravelMiddleOffice.Helpers
{
        public static class Logger
        {
            public static void WriteToLog(string message)
            {
                try
                {
                    string path = "" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + DateTime.Today.ToString("yyyy-MM-dd_HH") + ".log";

                    string str = "";

                    if (File.Exists(path))
                    {
                        using (StreamReader sreader = new StreamReader(path))
                        {
                            str = sreader.ReadToEnd();
                        }

                        File.Delete(path);
                    }

                    using (StreamWriter swriter = new StreamWriter(path, false))
                    {
                        str = "_______________________________________" + swriter.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message + swriter.NewLine + str; //"example text" + Environment.NewLine + str;
                        swriter.Write(str);
                    }
                }
                catch (Exception)
                { }
            }

            public static void WriteToLog(string[] messages)
            {
                try
                {
                    StreamWriter outfile = new StreamWriter("" + AppDomain.CurrentDomain.BaseDirectory + @"/log/" + DateTime.Today.ToString("yyyy-MM-dd_HH") + ".log", true);
                    {
                        outfile.WriteLine("_______________________________________");
                        outfile.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        foreach (string message in messages)
                            outfile.WriteLine(message);
                    }
                    outfile.Close();
                }
                catch (Exception)
                { }
            }
        }
}
