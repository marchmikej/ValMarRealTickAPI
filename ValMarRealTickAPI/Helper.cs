using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ValMarRealTickAPI
{
    public static class Helper
    {
        public static void writeToFile(string newLine)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = Path.Combine(folder, "valmarapi.log");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(newLine);
                }
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(newLine);
            }
        }
    }
}
