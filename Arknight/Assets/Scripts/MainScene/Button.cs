﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public GameObject m_Option;
    public GameObject m_OptionButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene()
    {

    }

    public void OnOption()
    {
        m_OptionButton.SetActive(false);
        m_Option.SetActive(true);
    }
}
