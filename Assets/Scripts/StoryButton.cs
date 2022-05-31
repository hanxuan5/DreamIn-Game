using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryButton : MonoBehaviour
{
    public GameObject story;

    public void click()
    {
        if (story.activeSelf == true)
        {
            story.SetActive(false);
        } else
        {
            story.SetActive(true);
        }
    }
}
