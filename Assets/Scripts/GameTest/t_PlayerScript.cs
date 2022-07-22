using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class t_PlayerScript : MonoBehaviour
{
    public TMP_Text nameText;
    public GameObject nameTextObj;

    internal string playerName;
    private int playerIndex;//player index in character array
    private int playerIdentity;
    private GameData gameData;

    private Rigidbody2D body;
    public float runSpeed = 20.0f;

    private Animator animator;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v);
        body.velocity = dir * runSpeed;

        if (v > 0)
        {
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        else if (v < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", true);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
        if (h > 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", true);
        }
        else if (h < 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", true);
            animator.SetBool("right", false);
        }

        if (h == 0 && v == 0)
        {
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
        }
    }
    public string GetPlayerName()
    {
        return playerName;
    }

    public string GetPlayerIdentity()
    {
        switch (playerIdentity)
        {
            case 0:
                return "Detective";
            case 1:
                return "Murderer";
            case 2:
                return "Suspect";
            default:
                Debug.LogError("wrong identity info!");
                break;
        }
        return "";
    }
    public string GetPlayerInfo()
    {
        return gameData.character[playerIndex].background;
    }
}
