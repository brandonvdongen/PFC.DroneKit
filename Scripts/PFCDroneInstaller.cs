#if UNITY_EDITOR
using PFCTools2.Utils.VersionManager;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace PFCTools.Drone {

    [ExecuteInEditMode]
    public class PFCDroneInstaller : MonoBehaviour {

        public PFCAssetVersionManager versionManager;

        public bool visualizeMarkers = false;
        public bool AutoCam = true;

        //Installer Components
        public GameObject Prefab = null;
        public GameObject DockLink = null;
        public GameObject HeadLink = null;
        public GameObject HudLink = null;

        public GameObject DockOffset = null;

        public bool advancedMode = false;
        public int selectedMountPoint = 0;
        public Transform customMountPoint = null;

        public bool installed = false;

        public VRCAvatarDescriptor descriptor = null;
        public Animator animator = null;
        public Dictionary<VRCExpressionsMenu, ExpressionMenuData> ExpressionMenus = new Dictionary<VRCExpressionsMenu, ExpressionMenuData>();
        public AnimatorWDState DroneWDState { get; set; }
        public AnimatorWDState FXWDState { get; set; }

        //Installer Variables
        public CustomizerWindows currentCustomizerWindow = CustomizerWindows.ModeSelect;

        public VRCExpressionParameters droneExpParameters;
        public string droneExpName;
        public Texture2D droneExpIcon;
        public VRCExpressionsMenu droneExpMenu;
        public AnimatorController droneAnimationController;

        public Renderer HudRenderer = null;
        public Renderer DroneRenderer = null;

        public GameObject DroneModel = null;
        public GameObject HudModel = null;
        public GameObject DockModel = null;
        public GameObject ScreenRotator = null;
        //public Transform TiltTarget = null;
        public Mesh HudMesh;
        public GameObject FollowPoint;
        public GameObject SelfiePoint;
        public GameObject MapPoint;
        public GameObject MapCutoff;

        private bool HidTools = false;

        public SkinnedMeshRenderer DockRenderer = null;

        private void OnEnable() {
            SceneView.duringSceneGui += PerformHandleChecks;
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= PerformHandleChecks;
        }

        public void Update() {
            if (descriptor == null) {
                descriptor = gameObject.GetComponentInParent<VRCAvatarDescriptor>();
                if (descriptor != null) {
                    transform.parent = descriptor.transform;
                    transform.localScale = Vector3.one;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
            if (descriptor != null) {
                if (animator == null) {
                    animator = descriptor.gameObject.GetComponent<Animator>();
                }
                else {
                }
            }
            if (DroneRenderer == null && DroneModel != null) {
                DroneRenderer = DroneModel.GetComponentInChildren<Renderer>();
            }
            if (HudRenderer == null && HudModel != null) {
                HudRenderer = HudModel.GetComponentInChildren<Renderer>();
            }
            if (DockRenderer == null && DockModel != null) {
                DockRenderer = DockModel.GetComponentInChildren<SkinnedMeshRenderer>();
            }
            if (Prefab != null) {
                if (Prefab.transform.parent != null && Prefab.transform.parent != transform && Prefab.transform.hasChanged && Selection.activeObject != Prefab) {
                    if (Prefab.transform.localScale != Vector3.one) {
                        Vector3 scaleoffset = Prefab.transform.localScale - Vector3.one;
                        Prefab.transform.localScale = Vector3.one;
                        DockOffset.transform.localScale += (scaleoffset);
                    }
                    if (Prefab.transform.localPosition != Vector3.zero) {
                        Vector3 posOffset = Prefab.transform.localPosition;
                        Prefab.transform.localPosition = Vector3.zero;
                    }

                    //MapPoint.transform.position = Prefab.transform.parent.position + (Vector3.up * 50);
                    Prefab.transform.hasChanged = false;
                }
            }
        }

        public void UpdateExpressionMenus() {

            ExpressionMenus.Clear();
            if (descriptor.expressionsMenu != null) {
                GetExpressionMenus("Main Menu", null, descriptor.expressionsMenu);
            }
        }

        private void GetExpressionMenus(string name, ExpressionMenuData parent, VRCExpressionsMenu menu) {
            if (!ExpressionMenus.ContainsKey(menu)) {
                ExpressionMenuData data = new ExpressionMenuData(name, menu, parent);
                ExpressionMenus.Add(menu, data);

                foreach (VRCExpressionsMenu.Control control in menu.controls) {
                    if (control.type == VRCExpressionsMenu.Control.ControlType.SubMenu) {
                        GetExpressionMenus(control.name, data, control.subMenu);
                    }
                }
            }
        }

        public void performStepAttach() {

            Transform parent = null;
            Transform secondaryBone = null;

            Vector3 worldUp = Vector3.up;
            Quaternion cameraRotationOffset = Quaternion.Euler(20, 180, 0);

            if (PrefabUtility.GetPrefabInstanceHandle(this.gameObject) != null) {
                Debug.Log("Unpacking Prefab");
                PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            }
            if (selectedMountPoint != (int)AdvancedMountPoints.SelectAMountPoint) {
                if (advancedMode) {
                    if (selectedMountPoint == (int)AdvancedMountPoints.LeftArm) {
                        parent = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        cameraRotationOffset = Quaternion.Euler(30, 135, 0);
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.LeftLeg) {
                        parent = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                        worldUp = Vector3.forward;
                        cameraRotationOffset = Quaternion.Euler(0, 180, 0);
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.RightArm) {
                        parent = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        cameraRotationOffset = Quaternion.Euler(30, -135, 0);
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.RightLeg) {
                        parent = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                        worldUp = Vector3.forward;
                        cameraRotationOffset = Quaternion.Euler(0, 180, 0);
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.Spine) {
                        parent = animator.GetBoneTransform(HumanBodyBones.Spine);
                        worldUp = Vector3.back;
                        cameraRotationOffset = Quaternion.Euler(20, 0, 0);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.Chest);
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.UpperSpine) {
                        parent = animator.GetBoneTransform(HumanBodyBones.Chest);
                        worldUp = Vector3.back;
                        cameraRotationOffset = Quaternion.Euler(20, 0, 0);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                        if (secondaryBone == null) {
                            secondaryBone = animator.GetBoneTransform(HumanBodyBones.Neck);
                        }
                    }
                    if (selectedMountPoint == (int)AdvancedMountPoints.Custom) {
                        if (customMountPoint == null) {
                            return;
                        }

                        parent = customMountPoint;
                    }
                }
                else {
                    if (selectedMountPoint == (int)SimpleMountPoints.LeftHanded) {
                        parent = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                    }
                    if (selectedMountPoint == (int)SimpleMountPoints.RightHanded) {
                        parent = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        secondaryBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    }
                }

                ParentConstraint DockConstraint = DockLink.GetComponent<ParentConstraint>();
                if (DockConstraint.sourceCount == 0) {
                    DockConstraint.AddSource(new ConstraintSource());
                }

                DockConstraint.SetSource(0, new ConstraintSource() { sourceTransform = parent, weight = 1 });

                Prefab.transform.parent = descriptor.transform;
                Prefab.transform.localPosition = Vector3.zero;
                Prefab.transform.localRotation = Quaternion.identity;
                Prefab.transform.localScale = Vector3.one;

                transform.parent = null;
                Transform bone = animator.GetBoneTransform(HumanBodyBones.Head);
                ParentConstraint HeadConstraint = HeadLink.GetComponent<ParentConstraint>();
                if (HeadConstraint.sourceCount == 0) {
                    HeadConstraint.AddSource(new ConstraintSource());
                }

                HeadConstraint.SetSource(0, new ConstraintSource() { sourceTransform = bone, weight = 1 });
                DockLink.transform.position = parent.position;
                DockLink.transform.rotation = parent.rotation;
                DockOffset.transform.localPosition = Vector3.zero;
                DockOffset.transform.localRotation = Quaternion.identity;

                if (secondaryBone) {
                    DockOffset.transform.position = Vector3.Lerp(parent.position, secondaryBone.position, 0.5f);
                    DockOffset.transform.LookAt(secondaryBone.position, worldUp);
                }

                if (AutoCam) {
                    SceneView.lastActiveSceneView.LookAt(DockOffset.transform.position, cameraRotationOffset, 1);
                }

                RecalculateHudPosition();
                RecalculateHudRotation();
                FixMinimapOffsets();
                RecalculateFollowPoint();
            }
        }

        public void RecalculateFollowPoint() {
            Vector3 pos = DockOffset.transform.position;
            Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            pos.y = headBone.transform.position.y + 0.5f;
            FollowPoint.transform.position = pos;
        }

        public void RecalculateHudPosition() {
            HudLink.transform.position = descriptor.ViewPosition + (Vector3.forward * 0.5f);
        }
        public void RecalculateHudRotation() {
            HudLink.transform.LookAt(descriptor.ViewPosition, Vector3.up);
        }

        public void FixMinimapOffsets(float minHeight = float.PositiveInfinity) {
            if (minHeight == float.PositiveInfinity) {
                Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                minHeight = headBone.transform.position.y + 0.5f;
            }

            MapCutoff.transform.position = Prefab.transform.position + (Vector3.up * (float)minHeight);
            MapPoint.transform.position = MapCutoff.transform.position + (Vector3.up * 50);
        }

        public void removeAllDroneComponents() {
            //attemptToDestroy(gameObject);
            attemptToDestroy(Prefab);
            attemptToDestroy(DockLink);
            attemptToDestroy(HeadLink);
            Debug.Log("Cleared all dock components");
        }

        public void removeDroneAnimatorLayers() {
            AnimatorController animator = GetFXLayer();
            foreach (AnimatorControllerLayer layer in droneAnimationController.layers) {
                for (int i = 0; i < animator.layers.Length; i++) {
                    AnimatorControllerLayer fxlayer = animator.layers[i];
                    if (layer.name == fxlayer.name) {
                        animator.RemoveLayer(i);
                    }
                }
            }
            Debug.Log("Cleared Animator Layers");
        }
        private void attemptToDestroy(GameObject t) {
            if (t != null) {
                DestroyImmediate(t);
            }
        }

        public AnimatorController GetFXLayer() {

            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in descriptor.baseAnimationLayers) {
                if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX) {
                    return layer.animatorController as AnimatorController;
                }
            }

            return null;
        }

        public void InstallSDKFiles() {
            descriptor.customizeAnimationLayers = true;

            for (int i = 0; i < descriptor.baseAnimationLayers.Length; i++) {
                VRCAvatarDescriptor.CustomAnimLayer entry = descriptor.baseAnimationLayers[i];
                if (entry.type == VRCAvatarDescriptor.AnimLayerType.FX) {
                    entry.isDefault = false;
                    entry.animatorController = droneAnimationController;
                }
            }

            descriptor.customExpressions = true;
            descriptor.expressionsMenu = droneExpMenu;
            descriptor.expressionParameters = droneExpParameters;
            EditorUtility.SetDirty(descriptor);
        }

        public Color droneEmissionColor {
            get { return DroneRenderer.sharedMaterial.GetColor("_EmissionColor"); }
            set {
                DroneRenderer.sharedMaterial.SetColor("_EmissionColor", value);
            }
        }
        public bool droneHasEmission() {
            if (DroneRenderer == null) {
                return false;
            }

            return DroneRenderer.sharedMaterial.HasProperty("_EmissionColor");
        }

        public Color HudEmissionColor {
            get { return HudRenderer.sharedMaterials[1].GetColor("_EmissionColor"); }
            set {
                HudRenderer.sharedMaterials[1].SetColor("_EmissionColor", value);
            }
        }

        public bool hudHasEmission() {
            if (HudRenderer == null) {
                return false;
            }

            return HudRenderer.sharedMaterials[1].HasProperty("_EmissionColor");
        }

        /*
        public float TiltReduction {
            get { return TiltTarget.localPosition.y; }
            set {
                Vector3 pos = TiltTarget.localPosition;
                pos.y = value;
                TiltTarget.localPosition = pos;
            }
        }
        */

        private void OnDrawGizmos() {
            if (Prefab == null) {
                return;
            }

            if (Selection.activeGameObject == HudLink || visualizeMarkers == true) {
                if (hudHasEmission()) {
                    Gizmos.color = HudEmissionColor;
                }
                else {
                    Gizmos.color = new Color(0, 0.5f, 1, 0.5f);
                }

                Gizmos.DrawWireMesh(HudMesh, HudLink.transform.position, HudLink.transform.rotation * Quaternion.Euler(-90, 0, 0), Vector3.Scale(new Vector3(1f, 1f, 1f), Quaternion.Euler(-90, 0, 0) * HudLink.transform.lossyScale));
            }
            if (Selection.activeGameObject == FollowPoint || visualizeMarkers == true) {
                Gizmos.color = Color.green;
                DrawArrow.ForGizmo(FollowPoint.transform.position, FollowPoint.transform.rotation * new Vector3(0, 0, .3f));
                Handles.Label(FollowPoint.transform.position, "Follow Point");
            }
            if (Selection.activeGameObject == SelfiePoint || visualizeMarkers == true) {
                Gizmos.color = Color.blue;
                DrawArrow.ForGizmo(SelfiePoint.transform.position, SelfiePoint.transform.rotation * new Vector3(0, 0, .3f));
                Handles.Label(SelfiePoint.transform.position, "Selfie Point");
            }

            if (Selection.activeGameObject == MapCutoff || visualizeMarkers == true) {
                Color c = Color.red;
                c.a = 0.5f;
                Gizmos.color = c;
                Vector3 pos = MapCutoff.transform.position;
                Handles.Label(pos, $"Minimap Cutoff Plane ({MapCutoff.transform.position.y})m");
                Gizmos.DrawCube(pos, new Vector3(1, 0.001f, 1));

                Handles.Label(MapPoint.transform.position, $"Minimap Camera Target ({MapPoint.transform.position.y})m");
            }
        }

        private void PerformHandleChecks(SceneView scene) {
            if (DockOffset == null) {
                return;
            }

            if (Selection.activeObject == Prefab) {
                if (Tools.hidden == false) {
                    Tools.hidden = true;
                    HidTools = true;
                }

                Vector3 handlePos = DockOffset.transform.position;
                Quaternion handleRot = Tools.pivotRotation == PivotRotation.Local ? DockOffset.transform.rotation : Quaternion.identity;

                if (Tools.current == Tool.Move) {
                    DockOffset.transform.position = Handles.DoPositionHandle(handlePos, handleRot);
                }
                else if (Tools.current == Tool.Rotate) {
                    DockOffset.transform.rotation = Handles.DoRotationHandle(DockOffset.transform.rotation, handlePos);
                }
                else if (Tools.current == Tool.Scale) {
                    Vector3 oldScale = DockOffset.transform.localScale;
                    Vector3 newScale = Handles.DoScaleHandle(DockOffset.transform.localScale, handlePos, handleRot, HandleUtility.GetHandleSize(DockOffset.transform.position));
                    if (newScale.magnitude > oldScale.magnitude) {
                        float maxScale = Mathf.Max(newScale.x, newScale.y, newScale.z);
                        newScale = new Vector3(maxScale, maxScale, maxScale);
                    }
                    else {
                        float minScale = Mathf.Min(newScale.x, newScale.y, newScale.z);
                        newScale = new Vector3(minScale, minScale, minScale);
                    }
                    DockOffset.transform.localScale = newScale;
                }
            }
            else if (HidTools) {
                Tools.hidden = false;
                HidTools = false;
            }
        }
    }
}

#endif