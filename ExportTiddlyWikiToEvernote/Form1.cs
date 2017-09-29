using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ExportTiddlyWikiToEvernote
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();
            if (o.FileName != "")
            {
                txtFileName.Text = o.FileName;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            //FileInfo fi = new FileInfo(txtFileName.Text);

            //string[] Files = Directory.GetFiles(fi.DirectoryName,"*.html");

            ImportData(txtFileName.Text);
        }

        void ImportData(string filename)
        {
            StreamReader InFile = new StreamReader(filename, Encoding.Default);
            StreamWriter OutFile = new StreamWriter(Application.StartupPath + "\\output.enex");

            string FileLine = "";
            string OutString = "";

            string StartDiv = "<div title=\"";
            string EndDiv = "</div>";

            OutFile.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            OutFile.WriteLine("<!DOCTYPE en-export SYSTEM \"http://xml.evernote.com/pub/evernote-export2.dtd\">");
            OutFile.WriteLine("<en-export export-date=\"20170928T014203Z\" application=\"Evernote/Windows\" version=\"6.x\">");

            while (!InFile.EndOfStream)
            {
                FileLine += (char)InFile.Read();
                if (FileLine.Contains(StartDiv))
                {
                    //Clear the output data
                    FileLine = StartDiv;

                    while (!InFile.EndOfStream)
                    {

                        //RemoveTextBlock(ref InFile, ref FileLine, "<a href=\"", "\">");
                        //RemoveTextBlock(ref InFile, ref FileLine, "<pre ", "\">");
                        //RemoveTextBlock(ref InFile, ref FileLine, "<img ", "\">");
                        //RemoveTextBlock(ref InFile, ref FileLine, "<font ", ">");

                        FileLine += (char)InFile.Read();
                        if (FileLine.Contains(EndDiv))
                        {

                            //FileLine contains the entire Note.  Parse the note out of the FileLine and export it.
                            string title;
                            string createddatetime;
                            string tag;
                            string author;
                            string source;
                            string notedata;

                            title = ExtractTextBlock(ref FileLine, "title=\"", "\"");
                            author = ExtractTextBlock(ref FileLine, "creator=\"", "\"");
                            createddatetime = ExtractTextBlock(ref FileLine, "created=\"", "\"");
                            notedata = ExtractTextBlock(ref FileLine, "<pre>", "</pre>");

                            notedata = notedata.Replace("\n", "<br clear=\"none\"/>");             //replace line feeds with HTML breaks
                            notedata = notedata.Replace("\t", "&nbsp; &nbsp; &nbsp; &nbsp;");      //replace tabs with spaces

                            if (!notedata.StartsWith("/***"))
                            {
                                //only export notes that are not plugins (plugins start with /***)

                                OutString = "<note><title>" + title + "</title><content><![CDATA[<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                            "<!DOCTYPE en-note SYSTEM \"http://xml.evernote.com/pub/enml2.dtd\"><en-note><div>";

                                OutString += notedata + "</div></en-note>]]></content> <created>" + createddatetime + "</created>" +
                                            "<note-attributes><author>" + author + "</author>" + "<source>TiddlyWiki</source></note-attributes></note>";

                                OutFile.Write(OutString);
                            }
                            FileLine = "";
                            break;      //exit inner while loop and go to the next note.
                        }
                    }
                }
            }

            OutFile.Write("</en-export>");

            InFile.Close();
            OutFile.Close();
        }

        void RemoveTextBlock(ref StreamReader InFile, ref string FileLine, string BeginString, string EndString)
        {
            if (FileLine.Contains(BeginString))
            {
                string LinkText = "";
                //Remove the <a href=" text:
                FileLine = FileLine.Substring(0, FileLine.Length - BeginString.Length);
                while (!InFile.EndOfStream)
                {
                    LinkText += (char)InFile.Read();
                    if (LinkText.Contains(EndString))
                    {
                        break;
                    }
                }
            }
        }

        string ExtractTextBlock(ref string FileLine, string BeginString, string EndString)
        {
            int Start, End;
            string outputtext = "";
            if (FileLine.Contains(BeginString) && FileLine.Contains(EndString))
            {
                Start = FileLine.IndexOf(BeginString);
                Start += BeginString.Length;
                End = FileLine.IndexOf(EndString, Start);
                outputtext = FileLine.Substring(Start, End - Start);
                FileLine = FileLine.Substring(End+1);
            }

            return outputtext;
        }

        void CleanUpOutputString(ref string OutString)
        {

            OutString = OutString.Replace("//", "  ");

            char[] FunnyTabSpace = new char[3];

            FunnyTabSpace[0] = (char)0xC2;
            FunnyTabSpace[1] = (char)0xA0;
            FunnyTabSpace[2] = (char)0x20;

            string FunnyTabSpaceString = new string(FunnyTabSpace);

            //Replace 0xC2A020 with 0x09    (TAB)
            OutString = OutString.Replace(FunnyTabSpaceString, ((char)0x09).ToString());


            char[] FunnyTab = new char[2];

            FunnyTab[0] = (char)0xC2;
            FunnyTab[1] = (char)0xA0;

            string FunnyTabString = new string(FunnyTab);

            //Replace 0xC2A020 with 0x09    (TAB)
            OutString = OutString.Replace(FunnyTabString, ((char)0x09).ToString());
            OutString = OutString.Replace("<br>", "\n");
            OutString = OutString.Replace("<span>", "");
            OutString = OutString.Replace("</span>", "");
            OutString = OutString.Replace("<u>", "__");
            OutString = OutString.Replace("</u>", "__");
            OutString = OutString.Replace("<b>", "''");
            OutString = OutString.Replace("</b>", "''");

            OutString = OutString.Replace("<ul>", "");
            OutString = OutString.Replace("</ul>", "");

            OutString = OutString.Replace("</font>", "");

            OutString = OutString.Replace("<li>", "* ");
            OutString = OutString.Replace("</li>", "\n");

            OutString = OutString.Replace("<i>", "//");
            OutString = OutString.Replace("</i>", "//");
            OutString = OutString.Replace("</a>", "");

            OutString = OutString.Replace("</pre>", "");
            OutString = OutString.Replace("<div>", "");

            /*
            OutString = OutString.Replace("&#39;", "'");
            OutString = OutString.Replace("&quot;", "\"");
            OutString = OutString.Replace("&amp;", "&");
            OutString = OutString.Replace("&lt;", "<");
            OutString = OutString.Replace("&gt;", ">");
            */
            
            
            
            
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
