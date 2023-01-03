namespace PFCTools.Drone {


    public enum CustomizerWindows {
        PreInstall,
        ModeSelect,
        Attach,
        InstallAnimators,
        Finalize,
        Customize,
        Uninstall
    }

    public enum SimpleMountPoints {
        SelectAMountPoint = 0,
        LeftHanded = 1,
        RightHanded = 2,

    }

    public enum AdvancedMountPoints {
        SelectAMountPoint = 0,
        LeftArm = 1,
        RightArm = 2,
        Spine = 3,
        UpperSpine = 4,
        LeftLeg = 5,
        RightLeg = 6,
        Custom = 7,
    }
}