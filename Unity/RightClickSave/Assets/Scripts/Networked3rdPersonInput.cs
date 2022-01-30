using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Networked3rdPersonInput : NetworkBehaviour
{

    NetworkManager networkManager = NetworkManager.Singleton;
    // Input state

    public bool walkByDefault;        // toggle for walking state
    public bool canCrouch = true;
    public bool canJump = true;
    [SerializeField]
    public InputState localState = new InputState();

    public NetworkVariable<InputState> state = new NetworkVariable<InputState>(NetworkVariableReadPermission.Everyone);           // The current state of the user input

    protected Transform cam;                    // A reference to the main camera in the scenes transform

    public Camera LocalCamera;

    bool IsLocal = false;
    public bool IsThisSever = false;

    protected virtual void Start()
    {
        // get the transform of the main camera

        IsLocal = IsOwner;
        IsThisSever = IsServer;

        if (IsLocal)
        {
            LocalCamera.enabled = true;
            cam = LocalCamera.transform;
        }
        else
        {
            LocalCamera.enabled = false;
            cam = Camera.main.transform;
        }
    }

    public override void OnNetworkSpawn()
    {

    }

    protected virtual void Update()
    {
        if (IsServer)
        {

            localState = state.Value;
            transform.position += localState.move * Time.deltaTime;
        }
        if (IsOwner)
        {
            // read inputs
            localState.crouch = canCrouch && Input.GetKey(KeyCode.C);
            localState.jump = canJump && Input.GetButton("Jump");

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // calculate move direction
            Vector3 move = cam.rotation * new Vector3(h, 0f, v).normalized;

            // Flatten move vector to the character.up plane
            if (move != Vector3.zero)
            {
                Vector3 normal = transform.up;
                Vector3.OrthoNormalize(ref normal, ref move);
                localState.move = move;
            }
            else localState.move = Vector3.zero;

            bool walkToggle = Input.GetKey(KeyCode.LeftShift);

            // We select appropriate speed based on whether we're walking by default, and whether the walk/run toggle button is pressed:
            float walkMultiplier = (walkByDefault ? walkToggle ? 1 : 0.5f : walkToggle ? 0.5f : 1);

            localState.move *= walkMultiplier;

            // calculate the head look target position
            localState.lookPos = transform.position + cam.forward * 100f;

            //transform.position += state.move * Time.deltaTime;
            if (IsClient)
            {
                NetworkedInputServerRpc(localState);
            }
            else if(IsHost)
            {
                state.Value = localState;
            }
        }

    }

    [ServerRpc]
    void NetworkedInputServerRpc(InputState iState)
    {
        state.Value = iState;
    }

    [ServerRpc]
    public void RandomTeleportServerRpc()
    {
        var oldPosition = transform.position;
        transform.position = GetRandomPositionOnXYPlane();
        var newPosition = transform.position;
        print($"{nameof(RandomTeleportServerRpc)}() -> {nameof(OwnerClientId)}: {OwnerClientId} --- {nameof(oldPosition)}: {oldPosition} --- {nameof(newPosition)}: {newPosition}");
    }

    private static Vector3 GetRandomPositionOnXYPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
    }
}

[System.Serializable]
public struct InputState : INetworkSerializable
{
    public Vector3 move;
    public Vector3 lookPos;
    public bool crouch;
    public bool jump;
    public int actionIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref lookPos);
        serializer.SerializeValue(ref crouch);
        serializer.SerializeValue(ref jump);
        serializer.SerializeValue(ref actionIndex);

    }
}


