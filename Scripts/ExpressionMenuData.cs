using VRC.SDK3.Avatars.ScriptableObjects;

namespace PFCTools.Drone {
    public class ExpressionMenuData {

        public ExpressionMenuData(string Name, VRCExpressionsMenu Menu, ExpressionMenuData Parent) {
            this.Name = Name;
            this.Menu = Menu;
            this.Parent = Parent;
        }

        public string Name { get; set; }
        public VRCExpressionsMenu Menu { get; set; }
        public ExpressionMenuData Parent { get; set; }


    }
}