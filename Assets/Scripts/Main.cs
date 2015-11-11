using UnityEngine;
using System.Collections;
using voice_recognition.cs;

public class Main : MonoBehaviour {
    private int step = 0;

    private MainForm mf;
    private bool allowQuitting = false;
    public float showSplashTimeout = 2.0F;
    
    void Start () {
        PXCMSession session = PXCMSession.CreateInstance();
        Debug.Log(session);
        if (session != null)
        {
            // Optional steps to send feedback to Intel Corporation to understand how often each SDK sample is used.
            PXCMMetadata md = session.QueryInstance<PXCMMetadata>();
            if (md != null)
            {
                string sample_name = "Voice Recognition CS";
                md.AttachBuffer(1297303632, System.Text.Encoding.Unicode.GetBytes(sample_name));
            }

            mf = new MainForm(session);
            //session.Dispose();
        }
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    
    void OnGUI()
    {
        if (step == 0)
        {
            mf.selectLangNo = GUI.SelectionGrid(new Rect(20, 20, 200, 50), mf.selectLangNo, mf.langList, 1);
            mf.selectSorceNo = GUI.SelectionGrid(new Rect(240, 20, 200, 50), mf.selectSorceNo, mf.sorceList, 1);
            if (GUI.Button(new Rect(460, 20, 100, 40), "start"))
            {
                mf.Start_Click();
                step = 1;
            }
        } else if (step == 1)
        {
            if (GUI.Button(new Rect(20, 20, 100, 40), "stop"))
            {
                mf.Destroy();
                step = 0;
            }
        }
        
       
        
    }

    void OnApplicationQuit()
    {
        if (mf != null)
        {
            mf.Destroy();
        }
        /*if (Application.loadedLevelName.ToLower() != "s02")
        {
            StartCoroutine("DelayedQuit");
        }
        if (!allowQuitting)
        {
            Application.CancelQuit();
        }*/
       
    }
    IEnumerator DelayedQuit()
    {
        Application.LoadLevel("s02");
        yield return new WaitForSeconds(showSplashTimeout);
        allowQuitting = true;
        Application.Quit();
    }

}
