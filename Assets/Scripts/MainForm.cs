/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace voice_recognition.cs
{
    public partial class MainForm
    {
        PXCMAudioSource source;
        private PXCMSession session;
        private Dictionary<int, Int32> modules = new Dictionary<int,int>();
        private Dictionary<int, PXCMAudioSource.DeviceInfo> devices=  new Dictionary<int,PXCMAudioSource.DeviceInfo>();
        private Dictionary<int, PXCMSpeechRecognition.ProfileInfo> langs = new Dictionary<int, PXCMSpeechRecognition.ProfileInfo>();

        public string g_file; //SM: ToDo function for return the file
        public string v_file; //SM: ToDo function for return the file

        public int selectLangNo = 0;
        public string[] langList;
        public int selectSorceNo = 0;
        public string[] sorceList;

        public MainForm(PXCMSession session)
        {
            this.session = session;
            PopulateSource();
            PopulateModule();
            PopulateLanguage();
        }

        private void PopulateSource()
        {
            devices.Clear();
            Debug.Log("PopulateSource");
            source = session.CreateAudioSource();
            if (source != null)
            {
                source.ScanDevices();

                for (int i = 0; ; i++)
                {
                    PXCMAudioSource.DeviceInfo dinfo;
                    if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    devices[i] = dinfo;
                }
                sorceList = new string[devices.Count];
                for (int i = 0; i < devices.Count; i++)
                {
                    sorceList[i] = devices[i].name;
                }
                source.Dispose();
            }
        }

        private void PopulateModule() {
            modules.Clear();
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.cuids[0] = PXCMSpeechRecognition.CUID;
            for (int i = 0; ; i++)
            {
                PXCMSession.ImplDesc desc1;
                if (session.QueryImpl(desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                modules[i] = desc1.iuid;
            }
        }

        private void PopulateLanguage()
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.cuids[0] = PXCMSpeechRecognition.CUID;
            desc.iuid=GetCheckedModule();

            PXCMSpeechRecognition vrec;
            if (session.CreateImpl<PXCMSpeechRecognition>(desc, out vrec) < pxcmStatus.PXCM_STATUS_NO_ERROR) return;

            for (int i = 0; ; i++)
            {
                PXCMSpeechRecognition.ProfileInfo pinfo;
                if (vrec.QueryProfile(i,out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                langs[i] = pinfo;
            }
            vrec.Dispose();
            
            langList = new string[langs.Count];
            for (int i = 0; i < langs.Count; i++)
            {
                langList[i] = langs[i].language.ToString();
            }
           
        }

        public PXCMAudioSource.DeviceInfo GetCheckedSource()
        {
            return devices[selectSorceNo];
        }

        public Int32 GetCheckedModule()
        {
            return modules[0];
        }

        public int GetCheckedLanguage()
        {
            
            return selectLangNo;
        }

        public bool IsCommandControl()
        {
            //return commandControlToolStripMenuItem.Checked;
            return false;
        }




        public void Start_Click()
        {
            stop = false;
            System.Threading.Thread thread = new System.Threading.Thread(DoVoiceRecognition);
            thread.Start();
            System.Threading.Thread.Sleep(5);
            
        }

        private delegate void VoiceRecognitionCompleted();
        private void DoVoiceRecognition()
        {
            VoiceRecognition vr=new VoiceRecognition();
            vr.DoIt(this, session, source);

        }

        private void Stop_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        public static string TrimScore(string s) {
            s=s.Trim();
            int x=s.IndexOf('[');
            if (x<0) return s;
            return s.Substring(0,x);
        }

        private string LanguageToString(PXCMSpeechRecognition.LanguageType language)
        {
            switch (language)
            {
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_US_ENGLISH:    return "US English";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_GB_ENGLISH:    return "British English";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_DE_GERMAN:     return "Deutsch";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_IT_ITALIAN:    return "Italiano";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_BR_PORTUGUESE: return "BR Português";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_CN_CHINESE:    return "中文";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_FR_FRENCH:     return "Français";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_JP_JAPANESE:   return "日本語";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_US_SPANISH:    return "US Español";
                case PXCMSpeechRecognition.LanguageType.LANGUAGE_LA_SPANISH:    return "LA Español";
            }
            return null;
        }

        private delegate void TreeViewCleanDelegate();

        public void CleanConsole()
        {
            //Console2.Invoke(new TreeViewCleanDelegate(delegate  { Console2.Nodes.Clear(); } ));
        }

        private delegate void TreeViewUpdateDelegate(string line);
        public void PrintConsole(string line)
        {
            Debug.Log(line);
            //Console2.Invoke(new TreeViewUpdateDelegate(delegate (string line1) { Console2.Nodes.Add(line1).EnsureVisible(); }),new object[] { line });
        }

        

        public void PrintStatus(string line)
        {
            Debug.Log(line);
            //Status2.Invoke(new TreeViewUpdateDelegate(delegate(string line1){ Status2.Nodes.Add(line1).EnsureVisible();}), new object[] { line });
        }

        //private delegate void ConsoleReplaceTextDelegate(TreeNode tn1, string text);

        public void ClearScores()
        {
            /*foreach (TreeNode n in Console2.Nodes)
            {
                string s=TrimScore(n.Text);
                if (s.Length > 0)
                    Console2.Invoke(new ConsoleReplaceTextDelegate(delegate(TreeNode tn1, string text) { tn1.Text = text; }), new object[] { n, s });
            }*/
        }

        public void SetScore(int label, int confidence) {
            /*for (int i=0;i<Console2.Nodes.Count;i++) {
                string s=TrimScore(Console2.Nodes[i].Text);
                if (s.Length==0) continue;
                if ((label--)!=0) continue;
                Console2.Invoke(new ConsoleReplaceTextDelegate(delegate(TreeNode tn1, string text) { tn1.Text = text; }), new object[] { Console2.Nodes[i], Console2.Nodes[i].Text + " [" + confidence + "%]" });
                break;
            }*/
        }

        public string[] GetCommands() {
            int ncmds=0;
            /*foreach (TreeNode tn in Console2.Nodes)
                if (TrimScore(tn.Text).Length>0) ncmds++;
            if (ncmds == 0) return null;*/
            string[] cmds=new string[ncmds];
            /*for (int i = 0, k = 0; i < Console2.Nodes.Count; i++)
            {
                string cmd = TrimScore(Console2.Nodes[i].Text);
                if (cmd.Length <= 0) continue;
                cmds[k++] = cmd;
            }*/
            return cmds;
        }

        public void FillCommandListConsole(string filename)
        {
            string line;

            CleanConsole();
            PrintConsole("[Enter New Command]");

            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            try
            {
                while ((line = file.ReadLine()) != null)
                {
                    PrintConsole(line);
                }
                file.Close();
            }
            catch {
                file.Close();
            }
            
        }

        public string AlertToString(PXCMSpeechRecognition.AlertType label) {
	        switch (label) {
	        case PXCMSpeechRecognition.AlertType.ALERT_SNR_LOW:                return "SNR_LOW";
	        case PXCMSpeechRecognition.AlertType.ALERT_SPEECH_UNRECOGNIZABLE:  return "SPEECH_UNRECOGNIZABLE";
	        case PXCMSpeechRecognition.AlertType.ALERT_VOLUME_HIGH:            return "VOLUME_HIGH";
	        case PXCMSpeechRecognition.AlertType.ALERT_VOLUME_LOW:             return "VOLUME_LOW";
            case PXCMSpeechRecognition.AlertType.ALERT_SPEECH_BEGIN:           return "SPEECH_BEGIN";
            case PXCMSpeechRecognition.AlertType.ALERT_SPEECH_END:             return "SPEECH_END";
            case PXCMSpeechRecognition.AlertType.ALERT_RECOGNITION_ABORTED:    return "REC_ABORT";
            case PXCMSpeechRecognition.AlertType.ALERT_RECOGNITION_END:        return "REC_END";
            }
	        return "Unknown";
        }

        private volatile bool stop=true;
        private bool closing=false;

        public bool IsStop()
        {
            return stop;
        }


        private void setGrammarFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*GrammarFileDialog.Filter = "jsgf files (*.jsgf)|*.jsgf|list files (*.list)|*.list|All files (*.*)|*.*";
            GrammarFileDialog.FilterIndex = 1;
            GrammarFileDialog.RestoreDirectory = true;

            if (GrammarFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    g_file = GrammarFileDialog.FileName;
                    setGrammarFromFileToolStripMenuItem.Checked = true;
                    Console2.Nodes.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            else
            {
                setGrammarFromFileToolStripMenuItem.Checked = false;
                g_file = null;

                Console2.Nodes.Clear();
                AlwaysAddNewCommand();

            }*/

        }

        private void addVocabularyFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*VocabFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            VocabFileDialog.FilterIndex = 1;
            VocabFileDialog.RestoreDirectory = true;

            if (VocabFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    v_file = VocabFileDialog.FileName;
                    addVocabularyFromFileToolStripMenuItem.Checked = true;
                    Console2.Nodes.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            else {
                addVocabularyFromFileToolStripMenuItem.Checked = false;
                v_file = null;
            }*/
        }

        

        public void Destroy()
        {
            stop = true;
        }
    }

    
}
