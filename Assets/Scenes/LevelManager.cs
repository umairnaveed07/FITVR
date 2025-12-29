using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class LevelManager : MonoBehaviourPunCallbacks
{
    public GameObject loadingPanel;
    public GameObject destroyThis;
    //public Slider loadingBar;
    //public TMP_Text loadingText;
    private string levelToLoad = "";


    public void LoadLevel (string levelName)
    {
        Time.timeScale = 1.0f;
            Destroy(destroyThis);
        this.levelToLoad = levelName;
       PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected (DisconnectCause cause)
    {

        if(this.levelToLoad == "")
        {
            return ;
        }

        StartCoroutine(LoadSceneAsync(this.levelToLoad));
    }
    

    IEnumerator LoadSceneAsync ( string levelName )
    {
        loadingPanel.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(levelName);
        //AsyncOperation op = SceneManager.LoadSceneAsync(levelName , LoadSceneMode.Additive);

        while ( !op.isDone )
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
	    //Debug.Log(op.progress);
            //loadingBar.value = progress;
           // loadingText.text = progress * 100f + "%";

            yield return null;
        }
    }
}

