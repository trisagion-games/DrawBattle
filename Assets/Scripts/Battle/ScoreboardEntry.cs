﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Attach to each listing in Scoreboard.
public class ScoreboardEntry : MonoBehaviour {
    public Text baseHealth, tankHealth;

    public Image baseImg, tankImg;
    public int playerNum;

    void Start() {
        baseImg.color = ServerInfo.PLAYER_COLOR_PRESETS[playerNum - 1];
        tankImg.color = ServerInfo.PLAYER_COLOR_PRESETS[playerNum - 1];
    }

    public void UpdateEntry(PlayerStats newPlayer) {
        playerNum = newPlayer.GetPlayerNum();
        baseHealth.text = newPlayer.baseHealth.ToString();
        tankHealth.text = newPlayer.tankHealth.ToString();
        baseImg.color = ServerInfo.PLAYER_COLOR_PRESETS[playerNum - 1];
        tankImg.color = ServerInfo.PLAYER_COLOR_PRESETS[playerNum - 1];
    }
}