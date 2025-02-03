using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;


public class bl_PlayerNetwork : MonoBehaviour
{
    #region Public members
    public Team RemoteTeam { get; set; }
    public PlayerFPState FPState = PlayerFPState.Idle;
    [FormerlySerializedAs("HeatTarget")]
    public Transform HeadTarget;
    public float SmoothingDelay = 8f;
    public List<bl_NetworkGun> NetworkGuns = new List<bl_NetworkGun>();
    public Material InvicibleMat;
    #endregion

    #region Private members
    private bool isWeaponBlocked = false;
    private bl_NetworkGun currentGun;
    private bl_NamePlateBase DrawName;
    private Transform m_Transform;
    private int currentGunID = -1;
    #endregion

    #region Public Properties
    public bool isFriend
    {
        get;
        set;
    }
    public int networkGunID
    {
        get;
        set;
    }
    public bl_NetworkGun CurrenGun
    {
        get;
        set;
    }
    public PlayerState NetworkBodyState
    { get; set; }
    #endregion
    protected void Awake()
    {
        OnPhotonPlayerConnected();
        bl_EventHandler.onChangeWeapon += ChangeWeaponID;
        m_Transform = transform;
        DrawName = GetComponent<bl_NamePlateBase>();
        NetworkGuns.ForEach(x =>
        {
            if (x != null) x.gameObject.SetActive(false);
        });

    }
    private void ChangeWeaponID(int newGunID)
    {
        currentGunID = newGunID;
    }
    protected void OnDisable()
    {
        if (NetGunsRoot != null) NetGunsRoot.gameObject.SetActive(false);
    }
    public void Update()
    {
        OnLocalPlayer();
        OnRemotePlayer();
    }
    void OnLocalPlayer()
    {
        PlayerReferences.playerAnimations.BodyState = fpControler.State;
        PlayerReferences.playerAnimations.IsGrounded = fpControler.isGrounded;
        PlayerReferences.playerAnimations.Velocity = fpControler.Velocity;
        PlayerReferences.playerAnimations.FPState = FPState;
    }
    void OnRemotePlayer()
    {
        if (!isWeaponBlocked)
        {
            CurrentTPVGun();
        }
    }

    private int _preFrameGunID;
    public void CurrentTPVGun(bool local = false, bool force = false)
    {
        if (PlayerReferences.gunManager == null)
            return;
        local = true;
        if (_preFrameGunID == currentGunID) return;
        for (int i = 0; i < NetworkGuns.Count; i++)
        {
            currentGun = NetworkGuns[i];
            if (currentGun == null) continue;

            int currentID = (local) ? PlayerReferences.gunManager.GetCurrentWeapon().GunID : networkGunID;
            if (currentGun.GetWeaponID == currentID)
            {
                currentGun.gameObject.SetActive(true);
                CurrenGun = currentGun;
                CurrenGun.SetUpType();
            }
            else
            {
                if (currentGun != null)
                    currentGun.gameObject.SetActive(false);
            }
        }
        _preFrameGunID = currentGunID;
    }
    public void ReplicateFire(GunType weaponType, Vector3 hitPosition, Vector3 inacuracity)
    {
        FireSync(weaponType, hitPosition, inacuracity);
    }
    public void IsFireGrenade(float t_spread, Vector3 pos, Quaternion rot, Vector3 direction)
    {
        FireGrenadeRpc(t_spread, pos, rot, direction);
    }
    void FireSync(GunType weaponType, Vector3 hitPosition, Vector3 inaccuracity)
    {
        if (CurrenGun == null) return;
        switch (weaponType)
        {
            case GunType.Machinegun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Machinegun);
                break;
            case GunType.Pistol:
                CurrenGun.Fire(hitPosition, inaccuracity);
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Pistol);
                break;
            case GunType.Burst:
            case GunType.Sniper:
            case GunType.Shotgun:
                CurrenGun.Fire(hitPosition, inaccuracity);
                break;
            case GunType.Knife:
                CurrenGun.KnifeFire();//if you need add your custom fire launcher in networkgun
                PlayerReferences.playerAnimations.PlayFireAnimation(GunType.Knife);
                break;
            default:
                Debug.LogWarning("Not defined weapon type to sync bullets.");
                break;
        }
    }
    public void SyncCustomProjectile(Hashtable data) => RPCFireCustom(data);//photonView.RPC(nameof(
    void RPCFireCustom(Hashtable data)
    {
        CurrenGun?.FireCustomLogic(data);
        if (CurrenGun != null)
            PlayerReferences.playerAnimations.PlayFireAnimation(CurrenGun.Info.Type);
    }
    void FireGrenadeRpc(float m_spread, Vector3 pos, Quaternion rot, Vector3 direction)
    {
        CurrenGun.GrenadeFire(m_spread, pos, rot, direction);
    }

    public void SetWeaponBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        RPCSetWBlocked(blockState);
    }
    public void RPCSetWBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (isWeaponBlocked)
        {
            for (int i = 0; i < NetworkGuns.Count; i++)
            {
                NetworkGuns[i].gameObject.SetActive(false);
            }
        }
        else
        {
            CurrentTPVGun(false, true);
        }

        PlayerReferences.playerAnimations.BlockWeapons(blockState);
        currentGunID = -1;
    }


    public void OnPhotonPlayerConnected()
    {
        RPCSetWBlocked(isWeaponBlocked ? 1 : 0);
    }

    public Transform NetGunsRoot
    {
        get
        {
            var ng = NetworkGuns.FirstOrDefault(x => x != null);
            if (ng != null) return ng.transform.parent;

            return null;
        }
    }
    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
    private bl_FirstPersonControllerBase fpControler => PlayerReferences.firstPersonController;
}