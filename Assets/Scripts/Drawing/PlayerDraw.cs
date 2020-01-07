﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDraw : PlayerDrawBehavior {

    public PaintCanvas paintCanvas;

    /// Last frame's mouse position. Used for lerping
    Vector2 prevPos;

    /// Used to keep track of mouseup/down events so we don't fill in the space between 2 points if there was a pen lift
    bool isDragging = false;

    int completedPlayers = 0;

    ServerInfo serverInfo;

    public bool eraserEnabled = false;
    public Sprite penEnabledImg, penDisabledImg, eraserEnabledImg, eraserDisabledImg, submittedImg;
    public Button penButton, eraserButton;
    public Button submitButton;

    private void Start() {
        serverInfo = GameObject.FindObjectOfType<ServerInfo>();
    }

    private void Update() {

        // Pencil/eraser
        if(Input.GetKeyDown(KeyCode.P)) {
            SetEraserEnabled(false);
        }
        if(Input.GetKeyDown(KeyCode.E)) {
            SetEraserEnabled(true);
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (Input.GetMouseButton(0)) {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                var pallet = hit.collider.GetComponent<PaintCanvas>();
                if (pallet != null) {

                    // Check to make sure the canvas was clicked
                    Renderer rend = hit.transform.GetComponent<Renderer>();
                    MeshCollider meshCollider = hit.collider as MeshCollider;

                    if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
                        return;

                    Texture2D tex = rend.material.mainTexture as Texture2D;
                    Vector2 pixelUV = hit.textureCoord;

                    pixelUV.x *= tex.width;
                    pixelUV.y *= tex.height;

                    Color currColor = new Color(0, 0, 0, 0);

                    if (!eraserEnabled)
                        currColor = GameObject.FindObjectOfType<ColorPicker>().currColor;

                    int currSize = GameObject.FindObjectOfType<BrushPicker>().brushSize;
                    if (!isDragging)
                        prevPos = pixelUV;
                    else
                        paintCanvas.ColorBetween(prevPos, pixelUV, currColor, currSize);

                    paintCanvas.BrushAreaWithColor(pixelUV, currColor, currSize);
                    prevPos = pixelUV;
                    isDragging = true;
                }
            }
        }
    }

    public override void SendFullTexture(RpcArgs args) {
        byte[] textureData = args.GetNext<byte[]>();
        paintCanvas.SetAllTextureData(textureData.Compress());

        // // Testing
        // NetworkManager.Instance.InstantiateProjectile().networkObject.SendRpc(RPC_SEND_FULL_TEXTURE, Receivers.AllBuffered, textureData);
    }

    /// Should be run ONLY by the server.
    public override void SendDrawingComplete(RpcArgs args) {
        int playerNum = args.GetNext<int>();

        if (ServerInfo.isServer) {
            completedPlayers++;

            if (completedPlayers == serverInfo.networkObject.numPlayers) {
                serverInfo.networkObject.SendRpc(ServerInfo.RPC_CHANGE_PHASE, Receivers.All, (int) ServerInfo.GamePhase.Battling);
            }
        } else {
            Debug.LogError("Server-only RPC SendDrawing was called on a client!");
        }
    }

    public void RequestCompleteDrawing() {
        if (networkObject != null) {
            networkObject.SendRpc(RPC_SEND_DRAWING_COMPLETE, Receivers.Server, ServerInfo.playerNum);

            // Save drawing
            byte[] textureData = paintCanvas.GetAllTextureData().Compress();
            PlayerShoot.textureData = textureData;

            // Update button
            submitButton.image.sprite = submittedImg;
        }
    }

    // public void RequestSendTexture() {
    //     if (networkObject != null) {
    //         // networkObject.SendRpc(RPC_SEND_FULL_TEXTURE, Receivers.AllBuffered, paintCanvas.GetAllTextureData());
    //         // Testing
    //         byte[] textureData = paintCanvas.GetAllTextureData();
    //         // paintCanvas.SetAllTextureData(textureData.Compress());
    //         print("Sending texture data...");
    //         Projectile newProj = NetworkManager.Instance.InstantiateProjectile() as Projectile;
    //         newProj.tempTextureData = textureData.Compress();
    //     }
    // }

    public void SetEraserEnabled(bool newVal) {
        eraserEnabled = newVal;

        if (eraserEnabled) {
            eraserButton.image.sprite = eraserEnabledImg;
            penButton.image.sprite = penDisabledImg;
        } else {
            eraserButton.image.sprite = eraserDisabledImg;
            penButton.image.sprite = penEnabledImg;
        }
    }
}