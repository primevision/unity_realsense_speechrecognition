/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2013-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace voice_recognition.cs
{
    class VoiceRecognition
    {
        MainForm form;
        PXCMAudioSource source;
        PXCMSpeechRecognition sr;

        void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
        {
            Debug.Log("OnRecognition");
            if (data.scores[0].label < 0)
            {
                form.PrintConsole(data.scores[0].sentence);
                if (data.scores[0].tags.Length > 0)
                    form.PrintConsole(data.scores[0].tags);
            }
            else
            {
                form.ClearScores();
                for (int i = 0; i < PXCMSpeechRecognition.NBEST_SIZE; i++)
                {
                    int label = data.scores[i].label;
                    int confidence = data.scores[i].confidence;
                    if (label < 0 || confidence == 0) continue;
                    form.SetScore(label, confidence);
                }
                if (data.scores[0].tags.Length > 0)
                    form.PrintConsole(data.scores[0].tags);
            }

        }

        void OnAlert(PXCMSpeechRecognition.AlertData data)
        {
            Debug.Log("OnAlert");
            form.PrintStatus(form.AlertToString(data.label));
        }

        void CleanUp() {
            if (sr != null)
            {
                sr.Dispose();
                sr = null;
            }
            if (source != null)
            {
                source.Dispose();
                source = null;
            }
        }

        public bool SetVocabularyFromFile(String VocFilename)
        {

            pxcmStatus sts = sr.AddVocabToDictation(PXCMSpeechRecognition.VocabFileType.VFT_LIST, VocFilename);
            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) return false;

            return true;
        }

        bool SetGrammarFromFile(String GrammarFilename) 
        {

            Int32 grammar = 1;

	        pxcmStatus sts = sr.BuildGrammarFromFile(grammar, PXCMSpeechRecognition.GrammarFileType.GFT_NONE, GrammarFilename);
	        if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) {
		        form.PrintStatus("Grammar Compile Errors:");
                form.PrintStatus(sr.GetGrammarCompileErrors(grammar));
		        return false;
	        }

	        sts = sr.SetGrammar(grammar);
	        if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) return false;


            return true;
        }

        public void DoIt(MainForm form1, PXCMSession session, PXCMAudioSource s) {
            Debug.Log("DoIt");
            form = form1;
            Debug.Log("DoIt:01");
            /* Create the AudioSource instance */
            //source = s;
            source =session.CreateAudioSource();
            Debug.Log("DoIt:02");
            if (source == null) {
                CleanUp();
                form.PrintStatus("Stopped");
                return;
            }
            Debug.Log("DoIt:03");
            /* Set audio volume to 0.2 */
            source.SetVolume(0.2f);
            Debug.Log("DoIt:04");
            /* Set Audio Source */
            source.SetDevice(form.GetCheckedSource());
            Debug.Log("DoIt:05");
            /* Set Module */
            PXCMSession.ImplDesc mdesc = new PXCMSession.ImplDesc();
            mdesc.iuid = form.GetCheckedModule();
            Debug.Log("DoIt:06");
            pxcmStatus sts = session.CreateImpl<PXCMSpeechRecognition>(out sr);
            Debug.Log("DoIt:07");
            
            if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                Debug.Log("DoIt:10");
                PXCMSpeechRecognition.ProfileInfo pinfo;
                sr.QueryProfile(form.GetCheckedLanguage(), out pinfo);
                sr.SetProfile(pinfo);

                if (form.IsCommandControl())
                {
                    Debug.Log("DoIt:20");
                    string[] cmds = form.GetCommands();
                    if (form.g_file != null && form.g_file.Length != 0)
                    {
                        Debug.Log("DoIt:30");
                        if (form.g_file.EndsWith(".list")){
                            Debug.Log("DoIt:40");
                            form.FillCommandListConsole(form.g_file);
                            cmds = form.GetCommands();
					        if (cmds.GetLength(0) == 0)
						        form.PrintStatus("Command List Load Errors");
                        }

                        // input Command/Control grammar file available, use it
                        if (!SetGrammarFromFile(form.g_file))
                        {
                            Debug.Log("DoIt:41");
                            form.PrintStatus("Can not set Grammar From File.");
					        CleanUp();
                            return;
				        };
                    }
                    else if (cmds != null && cmds.GetLength(0) != 0)
                    {
                        Debug.Log("DoIt:31");
                        // voice commands available, use them
                        sts = sr.BuildGrammarFromStringList(1, cmds, null);
                        sts = sr.SetGrammar(1);
                    } else {
                        Debug.Log("DoIt:32");
                        form.PrintStatus("No Command List. Dictation instead.");
                        if (form.v_file != null && form.v_file.Length != 0) SetVocabularyFromFile(form.v_file);
                        sts = sr.SetDictation();
                    }
                }
                else
                {
                    Debug.Log("DoIt:21");
                    if (form.v_file != null && form.v_file.Length != 0) SetVocabularyFromFile(form.v_file);
                    sts = sr.SetDictation();
                }

                if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    form.PrintStatus("Can't start recognition.");
                    CleanUp();
                    return;
                }

	            form.PrintStatus("Init Started");
                PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
                handler.onRecognition=OnRecognition;
                handler.onAlert=OnAlert;
                
                sts=sr.StartRec(source, handler);
                if (sts>=pxcmStatus.PXCM_STATUS_NO_ERROR) {
                    form.PrintStatus("Init OK");

                    // Wait until the stop button is clicked
                    while (!form.IsStop()) {
                        System.Threading.Thread.Sleep(5);
                    }

                    sr.StopRec();
                } else {
                    form.PrintStatus("Failed to initialize");
                }
	        } else {
		        form.PrintStatus("Init Failed");
        	}
            

            Debug.Log("DoIt:98");
            CleanUp();
	        form.PrintStatus("Stopped");
            Debug.Log("DoIt:99");
        }

        public void Destroy()
        {
            if (sr != null)
            {
                sr.StopRec();
                sr.Dispose();
            }
        }
    }
}
