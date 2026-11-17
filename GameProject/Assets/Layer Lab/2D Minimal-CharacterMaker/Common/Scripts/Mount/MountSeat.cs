using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace LayerLab.ArtMakerUnity
{
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: "LayerLab.ArtMakerUnity", sourceClassName: "MountSeat")]
    [ExecuteAlways]
    public sealed class MountSeat : MonoBehaviour
    {
        [SerializeField, HideInInspector] Transform rider;
        [UnityEngine.Serialization.FormerlySerializedAs("craft")]
        [SerializeField, HideInInspector] Transform mount;
        [SerializeField, HideInInspector] GameObject shadow;
        [SerializeField, Min(0)] float seatHeight;
        [SerializeField, HideInInspector] float shadowGroundY = 0.001f;
        [SerializeField, HideInInspector] Transform dead;
        [SerializeField, HideInInspector] float deadGroundY;

        public Transform Mount => mount;

        [SerializeField, HideInInspector] UnityEngine.Animations.PositionConstraint shadowGround;
        float appliedLift = float.NaN;
        bool appliedMounted;
        readonly List<SpriteRenderer> hiddenLeftHand = new List<SpriteRenderer>();

        void OnEnable()
        {
            appliedMounted = false;
            appliedLift = float.NaN;
            Refresh();
        }

        void OnDisable() => RestoreLeftHand();

        void LateUpdate() => Refresh();

        void HideLeftHand()
        {
            Transform hand = rider.Find("HandLeft");
            if (!hand) return;

            hand.GetComponentsInChildren(true, hiddenLeftHand);
            hiddenLeftHand.RemoveAll(renderer => !renderer || renderer.forceRenderingOff);
            foreach (var renderer in hiddenLeftHand)
                renderer.forceRenderingOff = true;
        }

        void RestoreLeftHand()
        {
            foreach (var renderer in hiddenLeftHand)
                if (renderer) renderer.forceRenderingOff = false;
            hiddenLeftHand.Clear();
        }

        void Refresh()
        {
            if (!rider || !mount) return;
            bool mounted = mount.gameObject.activeSelf;
            float lift = mounted ? seatHeight : 0f;
            if (appliedMounted == mounted && appliedLift == lift) return;
            if (mounted && !appliedMounted) HideLeftHand();
            if (!mounted && appliedMounted) RestoreLeftHand();
            appliedMounted = mounted;
            appliedLift = lift;
            SetHeight(rider, lift);
            if (dead) SetHeight(dead, deadGroundY - lift);
            if (shadowGround) shadowGround.enabled = mounted;
            if (shadow) SetHeight(shadow.transform, shadowGroundY - lift);
        }

        static void SetHeight(Transform part, float y)
        {
            var position = part.localPosition;
            if (Mathf.Approximately(position.y, y)) return;
            position.y = y;
            part.localPosition = position;
        }
    }
}
