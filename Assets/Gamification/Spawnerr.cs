using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;

public class Spawnerr : MonoBehaviour {
  private const float ACCELERATION_TIME = 10.0f;
  private const float ACCELERATION_INCREASE = 1.0f;

  public GameObject spawnGroup;
  public GameObject[] cubes;
  public Transform[] points;
  public float beat = (60.0f / 130.0f) * 5.0f;
  public float timer;

  private int count = 0;
  private int countPrev = 0;
  private int randHolder;
  private int randHolderPrev;
  private int randHolderPlayer1;
  private int randHolderPrevPlayer1;

  private float currentMovementSpeed = 10.0f;
  private float accelerationTimer = ACCELERATION_TIME;

  public List<string> spawnPositions;
  public List<Vector3> positions;

  // function so that same cubed positions to do not spawn together other wise
  // they would collide with eachother
  private (int, int) GetTwoNonEqualRandomNumbers(int start, int finish) {
    int first = Random.Range(start, finish);
    int second = Random.Range(start, finish);

    while (first == second) {
      second = Random.Range(start, finish);
    }

    return (first, second);
  }

  public void Reset() {

    if (PhotonNetwork.IsMasterClient == false) {
      return;
    }

    this.timer = 0.0f;
    this.beat = (60.0f / 130.0f) * 5;
    this.currentMovementSpeed = 12.0f;
    this.accelerationTimer = ACCELERATION_TIME;

    // delete all the object that are still on the scene
    for (int i = this.spawnGroup.transform.childCount - 1; i >= 0; i--) {
      PhotonNetwork.Destroy(this.spawnGroup.transform.GetChild(i).gameObject);
    }
  }

  // Update function to spawn the cubes and speed of the the cubes for spawning
  void Update() {

    if (PauseMenu.GameIsPaused == true) {
      return;
    }

    count = Random.Range(0, 11);
    List<GameObject> spawnedObjects = new List<GameObject>();

    // only spawn object if we are the master client (to avoid that everyone is
    // able to spawn new cubes)
    if (PhotonNetwork.IsMasterClient) {

      if (timer > beat) {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
          if (count == 0) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));
            spawnedObjects.Add(cube);
          } else if (count == 1) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));
            spawnedObjects.Add(cube);
          } else if (count == 2) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));
            spawnedObjects.Add(cube);
          } else if (count == 3) {
            randHolder = Random.Range(0, 4);

            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[0], this.positions[randHolder],
                Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 4) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(3, 8)],
                this.positions[Random.Range(1, 4)], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 5) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[8], this.positions[3], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 6) {
            GameObject cube = PhotonNetwork.Instantiate(this.spawnPositions[14],
                                                        this.positions[3],
                                                        Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 7) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[2], this.positions[1], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 8) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[2], this.positions[2], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 9) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[0], this.positions[9], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 10) {
            GameObject cube = PhotonNetwork.Instantiate(this.spawnPositions[1],
                                                        this.positions[10],
                                                        Quaternion.identity);
            spawnedObjects.Add(cube);
          }
        } else if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
          if (count == 0) {
            GameObject cubePlayer1 = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cubePlayer1.transform.Rotate(transform.forward,
                                         90 * Random.Range(0, 4));

            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(4, 8)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));

            spawnedObjects.Add(cubePlayer1);
            spawnedObjects.Add(cube);
          } else if (count == 1) {
            GameObject cubePlayer1 = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cubePlayer1.transform.Rotate(transform.forward,
                                         90 * Random.Range(0, 4));

            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(4, 8)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));

            spawnedObjects.Add(cubePlayer1);
            spawnedObjects.Add(cube);
          } else if (count == 2) {
            GameObject cubePlayer1 = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(0, 4)], Quaternion.identity);
            cubePlayer1.transform.Rotate(transform.forward,
                                         90 * Random.Range(0, 4));

            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(0, 3)],
                this.positions[Random.Range(4, 8)], Quaternion.identity);
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));
            spawnedObjects.Add(cubePlayer1);
            spawnedObjects.Add(cube);
          } else if (count == 3) {
            randHolder = Random.Range(4, 8);
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[0], this.positions[randHolder],
                Quaternion.identity);

            randHolderPlayer1 = Random.Range(0, 4);
            GameObject cubePlayer1 = PhotonNetwork.Instantiate(
                this.spawnPositions[0], this.positions[randHolderPlayer1],
                Quaternion.identity);

            spawnedObjects.Add(cubePlayer1);
            spawnedObjects.Add(cube);
          } else if (count == 4) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(9, 14)],
                this.positions[Random.Range(4, 8)], Quaternion.identity);
            GameObject cubePlayer1 = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(3, 8)],
                this.positions[Random.Range(1, 4)], Quaternion.identity);

            spawnedObjects.Add(cube);
            spawnedObjects.Add(cubePlayer1);
          } else if (count == 5) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[8], this.positions[3], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 6) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(3, 8)], this.positions[8],
                Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 7) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(3, 8)], this.positions[8],
                Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 8) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[Random.Range(3, 8)], this.positions[8],
                Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 9) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[8], this.positions[6], Quaternion.identity);
            spawnedObjects.Add(cube);
          } else if (count == 10) {
            GameObject cube = PhotonNetwork.Instantiate(
                this.spawnPositions[8], this.positions[7], Quaternion.identity);
            spawnedObjects.Add(cube);
          }
        }

        for (int i = 0; i < spawnedObjects.Count; i++) {
          spawnedObjects[i].transform.SetParent(this.spawnGroup.transform);
          Cube cubeScript = spawnedObjects[i].GetComponent<Cube>();

          if (cubeScript == null) {
            continue;
          }

          cubeScript.Initialize(this.currentMovementSpeed);
        }

        timer -= beat;
      }

      timer += Time.deltaTime;

      this.accelerationTimer -= Time.deltaTime;

      if (this.accelerationTimer <= 0.0f) {
        if (this.currentMovementSpeed >= 25.0f) {
          return;
        }
        this.accelerationTimer = ACCELERATION_TIME;
        this.currentMovementSpeed += ACCELERATION_INCREASE;
      }
    }
  }
}
