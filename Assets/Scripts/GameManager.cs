using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject coinPrefab;
    [SerializeField]
    private int coinCount = 4;

    // Start is called before the first frame update
    void Start()
    {
        CreatePlayer();
        CreateCoins();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreatePlayer()
    {
        Instantiate(playerPrefab, new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5)), Quaternion.identity);
    }

    private void CreateCoins()
    {
        for(int i = 0; i < coinCount; i++)
        {
            Instantiate(coinPrefab, new Vector3(Random.Range(-10, 10), 0.5f, Random.Range(-10, 10)), Quaternion.identity);
        }
    }
}
