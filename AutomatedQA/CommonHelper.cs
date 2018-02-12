using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;

namespace AutomatedQA
{
    public class CommonHelper
    {
        public static void Serialize<T>(object obj, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                formatter.Serialize(fs, obj);
            }
        }

        public static T DeSerialize<T>(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(T));
                return (T)formatter.Deserialize(fs);
            }
        }

        public static List<string> SplitCommand(string command,string separator)
        {
            int lengthCommand = command.Length;
            List<string> seperateCommands = new List<string>();
            int quotesStarted = -1;
            //int quotesEnded = -1;
            string tempCommand = string.Empty;
            for (int i=0;i<lengthCommand;i++)
            {
                char s = command[i];
                //find the pair of "
                if (s.Equals('"') && (i == 0 || (i > 0 && !command[i - 1].Equals('\\'))))
                {
                    if (quotesStarted > -1)
                    {
                        quotesStarted = -1;
                    }
                    else
                    {
                        quotesStarted = i;
                    }
                    if (i == lengthCommand - 1)
                    {
                        seperateCommands.Add(tempCommand);
                        tempCommand = string.Empty;
                    }
                }
                // the character is not "
                else
                {
                    //check whthere the character is ' '
                    if (s.Equals(' '))
                    {
                        //the ' ' is not in ""
                        if (quotesStarted == -1)
                        {
                            if (i != 0 && !command[i - 1].Equals(' '))
                            {
                                seperateCommands.Add(tempCommand);
                                tempCommand = string.Empty;
                            }
                        }
                        //the ' ' is in ""
                        else
                        {
                            tempCommand = tempCommand + s;
                            if (i == lengthCommand - 1)
                            {
                                seperateCommands.Add(tempCommand);
                                tempCommand = string.Empty;
                            }
                        }
                    }
                    //the character is not ' '
                    else
                    {
                        tempCommand = tempCommand + s;
                        if (i == lengthCommand - 1)
                        {
                            seperateCommands.Add(tempCommand);
                            tempCommand = string.Empty;
                        }
                    }
                }
            }
            return seperateCommands;
        }

        //check the string is matched with regex or whole value.
        public static bool IsMatchRegex(string pattern, string compare)
        {
            bool matched = false;
            //Regex Check
            if (pattern.StartsWith("Regex:"))
            {
                pattern = pattern.Substring(6);
                Regex regexCompare = new Regex(pattern, RegexOptions.Singleline);
                matched = regexCompare.IsMatch(compare);
            }
            else
            {
                matched = compare.Equals(pattern, StringComparison.CurrentCultureIgnoreCase);
            }
            return matched;
        }
    }
}
