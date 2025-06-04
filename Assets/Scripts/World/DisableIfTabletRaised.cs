using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfTabletRaised : MonoBehaviour
{
    private void Start()
    {
        if (SaveSystem.HasTabletBeenRaised())
        {
            gameObject.SetActive(false);
        }
    }
}

