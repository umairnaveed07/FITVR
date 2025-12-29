using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;

using Photon.Realtime;

public class BonusManager : MonoBehaviour

{

  public GameObject bonus1;

  public GameObject bonus2;

  public GameObject bonus3;

  public GameObject bonus4;

  public GameObject bonus5;

  public GameObject bonus6;

  public PhotonView PV;

  public int playercount = 1;

      public List<Vector3> positions;

  int count = 1;

  float waitTime = 10.0f;

      // Start is called before the first frame update 

    void Start() 

    {

            bonus1.SetActive(false);

            bonus2.SetActive(false);

            bonus3.SetActive(false);

            bonus4.SetActive(false);

            bonus5.SetActive(false);

            bonus6.SetActive(false);

       
  }

      // Update is called once per frame 

    void Update() 

    {

    if (PhotonNetwork.IsMasterClient) {

            waitTime -= Time.deltaTime;

            

     if (waitTime <= 0.0f) 

      {

              if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)  

      {

                    Debug.Log("player count is stil 1");

                  if (count == 1) 

        {

                        waitTime = 20.0f;

                        PhotonNetwork.Instantiate(
                "bonusPoint1", this.positions[0], Quaternion.identity);

            GameObject healthObj = GameObject.FindWithTag("Health");
            GameObject frenzyObj = GameObject.FindWithTag("frenzy");

            if (healthObj != null) {
                          PhotonNetwork.Destroy(healthObj);
            }

            if (frenzyObj != null) {
                          PhotonNetwork.Destroy(frenzyObj);
            }

                       

            count++;

                   
          }

                  else if (count == 2) 

        {

                      waitTime = 20.0f;

                       PhotonNetwork.Instantiate(
                "bonusPoint3", this.positions[2], Quaternion.identity);

            GameObject streakObj = GameObject.FindWithTag("streak");
            GameObject frenzyObj = GameObject.FindWithTag("frenzy");

            if (streakObj != null) {
                          PhotonNetwork.Destroy(streakObj);
            }

            if (frenzyObj != null) {
                          PhotonNetwork.Destroy(frenzyObj);
            }

                        count++;

                   
          }

                  else if (count == 3) 

        {

                      waitTime = 20.0f;
                        PhotonNetwork.Instantiate(
                "bonusPoint2", this.positions[1], Quaternion.identity);

            GameObject streakObj = GameObject.FindWithTag("streak");
            GameObject healthObj = GameObject.FindWithTag("Health");

                        if (streakObj != null) {
                          PhotonNetwork.Destroy(streakObj);
            }

            if (healthObj != null) {
                          PhotonNetwork.Destroy(healthObj);
            }

                        count = 1;

                   
          }

               
        }

                 else if (PhotonNetwork.CurrentRoom.PlayerCount > 1) 

                {

                    Debug.Log("player count is stil 1");

           if (count == 1) 

        {

                        waitTime = 20.0f;

                      PhotonNetwork.Instantiate(
                "bonusPoint4", this.positions[3], Quaternion.identity);

            GameObject healthObj = GameObject.FindWithTag("Health");
            GameObject frenzyObj = GameObject.FindWithTag("frenzy");

            if (healthObj != null) {
                          PhotonNetwork.Destroy(healthObj);
            }

            if (frenzyObj != null) {
                          PhotonNetwork.Destroy(frenzyObj);
            }

                      

            count++;

                   
          }

                  else if (count == 2) 

        {

                      waitTime = 20.0f;

                        PhotonNetwork.Instantiate(
                "bonusPoint6", this.positions[5], Quaternion.identity);

            GameObject streakObj = GameObject.FindWithTag("streak");
            GameObject frenzyObj = GameObject.FindWithTag("frenzy");

            if (streakObj != null) {
                          PhotonNetwork.Destroy(streakObj);
            }

            if (frenzyObj != null) {
                          PhotonNetwork.Destroy(frenzyObj);
            }

                        count++;

                   
          }

                  else if (count == 3) 

        {

                      waitTime = 20.0f;
             
            PhotonNetwork.Instantiate("bonusPoint5", this.positions[4],
                                       Quaternion.identity);

            GameObject streakObj = GameObject.FindWithTag("streak");
            GameObject healthObj = GameObject.FindWithTag("Health");

            if (streakObj != null) {
                          PhotonNetwork.Destroy(streakObj);
            }

            if (healthObj != null) {
                          PhotonNetwork.Destroy(healthObj);
            }

                      
            count = 1;

                   
          }

               
        }

           
      }
    }

       
  }

       
   
}
