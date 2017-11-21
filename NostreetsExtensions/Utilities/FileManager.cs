using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Utilities
{
    public class FileManager
    {
        public FileManager(string directory)
        {
            if (!directory.DirectoryExists()) { throw new Exception("Directory Path is not valid..."); }

            TargetedDirectory = directory;
        }

        public string TargetedDirectory { get; set; }
        public string LastFileAccessed { get; internal set; }


        public void CreateFile(string fileName)
        {
            string filePath = (!TargetedDirectory[TargetedDirectory.Length - 1].Equals("\\")) ? TargetedDirectory + "\\" + fileName : TargetedDirectory + fileName;

            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) ;
            }

            if (!File.Exists(filePath)) { throw new Exception("File was not created..."); }

            LastFileAccessed = fileName;
        }

        public void WriteToFile(string textToWrite, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                {
                    switch (args[i])
                    {
                        case DateTime time:
                            args[i] = time.ToShortDateString().Replace('/', '-');
                            break;

                        default:
                            args[i] = args[i].ToString();
                            break;
                    }
                }
                else
                {
                    args[i] = "";
                }

            }

            WriteToFile(LastFileAccessed, String.Format(textToWrite, args));
        }

        public void WriteToFile(string textToWrite)
        {
            WriteToFile(LastFileAccessed, textToWrite);
        }

        public void WriteToFile(string fileName, string textToWrite)
        {
            string filePath = (!TargetedDirectory[TargetedDirectory.Length - 1].Equals("\\")) ? TargetedDirectory + "\\" + fileName : TargetedDirectory + fileName;

            string[] splitText = textToWrite.Split(new[] { "\n" }, StringSplitOptions.None);

            if (File.Exists(filePath))
            {
                if (splitText != null && splitText.Length > 0)
                {
                    foreach (string text in splitText)
                    {
                        using (StreamWriter sw = new StreamWriter(filePath, true))
                        {
                            sw.WriteLine(text);
                        }
                    }

                }
            }

            LastFileAccessed = fileName;

        }

        public void CopyFile(string dirOfFileToCopy)
        {
            CopyFile(dirOfFileToCopy, LastFileAccessed);
        }

        public void CopyFile(string targetDir, string nameOfFileToCopy)
        {
            string sourcePath = (!TargetedDirectory[TargetedDirectory.Length - 1].Equals("\\")) ? TargetedDirectory + "\\" + nameOfFileToCopy : TargetedDirectory + nameOfFileToCopy;
            string targetPath = (!targetDir[targetDir.Length - 1].Equals("\\")) ? targetDir + "\\" + nameOfFileToCopy : targetDir + nameOfFileToCopy;

            if (File.Exists(TargetedDirectory))
            {
                // If file already exists in destination, delete it.
                if (File.Exists(sourcePath))
                {
                    File.Delete(sourcePath);
                }

                File.Copy(targetPath, sourcePath);
            }

            LastFileAccessed = nameOfFileToCopy;

        }

        public void DeleteFile()
        {
            DeleteFile(LastFileAccessed);
        }

        public void DeleteFile(string fileName)
        {
            string filePath = (!TargetedDirectory[TargetedDirectory.Length - 1].Equals("\\")) ? TargetedDirectory + "\\" + fileName : TargetedDirectory + fileName;

            if (File.Exists(TargetedDirectory))
            {
                File.Delete(TargetedDirectory);
            }

            LastFileAccessed = fileName;

        }
    }
}
