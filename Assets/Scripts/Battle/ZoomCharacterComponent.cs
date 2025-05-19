using Cinemachine;
using UnityEngine;

namespace Battle
{
    public class ZoomCharacterComponent : CharacterComponent
    {
        private bool zoomIn;
        private CharacterAnimator zoomInAnim;
        private CharacterAnimator zoomOutAnim;
        private CharacterJoycon zoomInJoycon;
        private CharacterJoycon zoomOutJoycon;
        private CharacterMovement characterMovement;
        private ShootingView view;
        [Header("View")]
        [SerializeField, Range(0, 180f)] private float verticalRange;
        [SerializeField, Range(0, 1000)] private float mouseSensitivity = 500;
        [SerializeField] private CinemachineVirtualCamera wideCam;
        [SerializeField] private CinemachineVirtualCamera zoomInCam;

        private void UpdateView()
        {
            if (FlagHelper.HasFlag(character.BodyState, BodyState.Jump)) return;
            view.UpdateView();
        }
        private void UpdateZoomIn()
        {
            if (view.CamType is not CamType.Zoom && Input.GetKey(KeyCode.Mouse1)) DoZoomIn();
            else if (view.CamType is CamType.Zoom && !Input.GetKey(KeyCode.Mouse1)) DoZoomOut();
        }
        private void DoZoomOut()
        {
            zoomIn = false;
            view.SetCamera(CamType.Wide);
        }
        private void DoZoomIn()
        {
            zoomIn = true;
            view.SetCamera(CamType.Zoom);
        }
        protected override void OnUpdate()
        {
            UpdateZoomIn();
            UpdateView();
        }
        protected override void OnLateUpdate() { }
        protected override void OnFixedUpdate() { }
        protected override void OnAwake()
        {
            zoomInAnim = new HanZoomInAnimator(character, animator);
            zoomOutAnim = new HanZoomOutAnimator(character, animator);
            view = new(body: transform, wideCam, zoomInCam)
            {
                VerticalRange = verticalRange,
                MouseSensitivity = mouseSensitivity
            };
            view.SetActive(true);
            zoomInJoycon = new HanZoomInJoycon(character, characterController);
            zoomOutJoycon = new HanZoomOutJoycon(character, characterController);
            DoZoomOut();
            characterMovement = new CharacterMovement(character, characterController)
            {
                JumpPower = JumpPower,
                MoveSpeed = MoveSpeed,
                SlidePower = SlidePower
            };
        }
        protected override void GetAnimatorFromFrame(out CharacterAnimator newAnimator)
        {
            newAnimator = zoomIn ? zoomInAnim : zoomOutAnim;
        }
        protected override void GetJoyconFromFrame(out IController newJoycon)
        {
            newJoycon = zoomIn ? zoomInJoycon : zoomOutJoycon;
        }
        protected override void GetMovementFromFrame(out CharacterMovement newMovement)
        {
            newMovement = characterMovement;
        }
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (view != null)
            {
                view.VerticalRange = verticalRange;
                view.MouseSensitivity = mouseSensitivity;
            }
        }

        protected override void OnHit(HitBoxCollision collision)
        {
        }
    }
}