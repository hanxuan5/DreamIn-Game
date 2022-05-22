using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using FrostweepGames.VoicePro;
using UnityEngine;

public class PlayerVoice: MonoBehaviour
{
    public Image voiceImage;
    private PhotonView photonView;
    private Recorder recorder;
    private Listener listener;

    [SerializeField]
    private bool isRecording;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        recorder = GetComponent<Recorder>();
        listener = GetComponent<Listener>();
        voiceImage.enabled = false;
    }
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown(KeyCode.T) && !isRecording)
        {
            StartRecord();
            isRecording = true;
        }
        else if (Input.GetKeyUp(KeyCode.T) && isRecording)
        {
            StopRecord();
            isRecording = false;
        }
    }
    void StartRecord()
    {
        recorder.StartRecord();
        photonView.RPC("RPCSetVoiceImage", RpcTarget.All, true);
    }
    void StopRecord()
    {
        recorder.StopRecord();
        photonView.RPC("RPCSetVoiceImage", RpcTarget.All, false);
        listener.StartListen();
    }

    [PunRPC]
    public void RPCSetVoiceImage(bool state)
    {
        voiceImage.enabled = state;
    }
}
