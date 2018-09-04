using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace eTreeScan
//by Anoop Thomas. Created : 19JUN2017, Last Modified: 04SEP2018
{
    class Program
    {
        public static StreamWriter eXML = new StreamWriter(@"Current.xml");
        public static string eXMLHeader, eXMLFooter, eXMLNode, sFolderName, sUserKey, iFolderName, sMetadataSet;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 4)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Invalid parameters");
                    Console.WriteLine("");
                    Console.WriteLine("USAGE : eTreeScan \"Source Folder Name\" \"XML Ingest Folder Name\" \"Metadata Set Standard ID\" \"User Key\"");
                    Console.WriteLine("");
                    Console.WriteLine("Source Folder Name : Specifies the directory to be indexed. This location should be accessible from the eMAM Application server and the Transcode server.");
                    Console.WriteLine("XML Ingest Folder Name : Specifies the eMAM XML Ingest folder.");
                    Console.WriteLine("Metadata Set Standard ID : Specifies the Metadata Set Srabdard Id. Please use 'eMAM Director->Admin Tools->Manage Metadata->Metadata Set' to obtain the Standard id ");
                    Console.WriteLine("User Key : Encrypted key associated with an active user. Please use 'eMAM Director->Admin Tools->Manage Users' to obtain the user key. Ensure that the selected user is assigned to the XML ingest profile");
                    return;
                }

                sFolderName = args[0].ToString();
                iFolderName = args[1].ToString();
                sMetadataSet = args[2].ToString();
                sUserKey = args[3].ToString();
                eXMLHeader = "<eMAM user-key=\"" + sUserKey + "\">";
                eXMLFooter = "</eMAM>";

                eXML.WriteLine(eXMLHeader);
                TreeScan(sFolderName);
                eXML.WriteLine(eXMLFooter);
                eXML.Close();
                eXML.Dispose();
                Compare();
                File.Copy("Current.xml", "Previous.xml", true);
                //File.Delete("Current.xml");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                eXML.WriteLine(ex.Message.ToString());
            }
        }
        private static void TreeScan(string sDir)
        {
            string fileName;
            string filePath;
            int position;
            try
            {
                foreach (string f in Directory.GetFiles(sDir, "*", SearchOption.AllDirectories))
                {

                    position = f.LastIndexOf('\\');
                    fileName = f.Substring(position + 1);
                    filePath = f.Substring(0, position);
                    string categorypath = filePath.Substring(Path.GetPathRoot(filePath).Length);
                    categorypath = categorypath.Replace("\\", "/");
                    if (categorypath.StartsWith(@"/"))
                        categorypath = categorypath.Substring(1);

                    string fExtention = Path.GetExtension(f);
                    if (fExtention != ".db" && fExtention != ".ini" && fExtention != ".DS_Store" && fExtention != ".cfg" && fExtention != ".san" && fExtention != ".time" && fExtention != "._")
                    {
                        eXMLNode = "<asset file-action=\"index\" file-name=\"" + fileName + "\" file-path=\"" + filePath + "\" ingest-action=\"create-new-asset\"><custom-metadata set-standard-id=\"" + sMetadataSet + "\"/>";
                        if (!(String.IsNullOrEmpty(categorypath)))
                            eXMLNode = eXMLNode + "<categories><category name=\"" + categorypath + "\"/></categories>";
                        eXMLNode = eXMLNode + "</asset>";

                        eXML.WriteLine(eXMLNode);
                    }
                }
            }
            catch (Exception ex)
            {
                eXML.WriteLine(ex.Message.ToString());
            }

        }

        private static void Compare()
        {
            if (!File.Exists("Previous.xml"))
                using (StreamWriter sw = File.CreateText("Previous.xml"))
                {
                    sw.WriteLine(eXMLHeader);
                    sw.WriteLine(eXMLFooter);
                };

            String[] linesA = File.ReadAllLines("Previous.xml");
            String[] linesB = File.ReadAllLines("Current.xml");

            IEnumerable<String> onlyB = linesB.Except(linesA);

            if (onlyB.Count() != 0)
            {
                string IngestXML = iFolderName + "\\Ingest_" + DateTime.Now.ToString("MMMM_dd_yyyy_HHmmss_FFF") + ".xml";
                using (StreamWriter eXML2 = new StreamWriter(IngestXML))
                    eXML2.WriteLine(eXMLHeader);

                File.AppendAllLines(IngestXML, onlyB);
                using (StreamWriter eXML2 = File.AppendText(IngestXML))
                    eXML2.WriteLine(eXMLFooter);
            }

        }
    }
}
