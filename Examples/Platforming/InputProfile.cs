namespace Carpet.Examples.Platforming
{
    public sealed class InputProfile
    {
        private readonly List<InputElement> _elements = [];

        public InputProfile(InputSettings settings)
        {
            Settings = settings;

            Jump = CreateButton("Jump");
            Grapple = CreateButton("Grapple");
            CancelGrappling = CreateButton("CancelGrappling");

            Up = CreateButton("Up");
            Down = CreateButton("Down");

            XAxis = CreateAxis("XAxis", "Right", "Left");

            MenuXAxis = CreateAxis("MenuXAxis", "MenuRight", "MenuLeft");
            MenuYAxis = CreateAxis("MenuYAxis", "MenuDown", "MenuUp");
        }

        public InputSettings Settings { get; private init; }

        public VirtualButton Jump { get; private set; }
        public VirtualButton Grapple { get; private set; }
        public VirtualButton CancelGrappling { get; private set; }

        public VirtualButton Up { get; private set; }
        public VirtualButton Down { get; private set; }

        public VirtualAxis XAxis { get; private set; }

        public VirtualAxis MenuXAxis { get; private set; }
        public VirtualAxis MenuYAxis { get; private set; }

        public void Update(float deltaTime)
        {
            foreach (InputElement element in _elements)
                element.Update(deltaTime);
        }

        private VirtualButton CreateButton(string name)
        {
            var button = new VirtualButton()
                .AddButton(Settings.GetButton(name))
                .AddKey(Settings.GetKey(name));

            return Add(button);
        }

        private VirtualAxis CreateAxis(string axisName, string positiveName, string negativeName)
        {
            var axis = new VirtualAxis()
                .SetAxis(Settings.GetAxis(axisName))
                .AddButtons(Settings.GetButton(positiveName), Settings.GetButton(negativeName))
                .AddKeys(Settings.GetKey(positiveName), Settings.GetKey(negativeName));

            return Add(axis);
        }

        private VirtualStick CreateStick(string xName, string yName, string leftName, string rightName, string upName, string downName)
        {
            var stick = new VirtualStick()
                .SetAxes(Settings.GetAxis(xName), Settings.GetAxis(yName))
                .AddButtons(Settings.GetButton(leftName), Settings.GetButton(rightName), Settings.GetButton(upName), Settings.GetButton(downName))
                .AddKeys(Settings.GetKey(leftName), Settings.GetKey(rightName), Settings.GetKey(upName), Settings.GetKey(downName));

            return stick;
        }

        private T Add<T>(T element) where T : InputElement
        {
            _elements.Add(element);

            return element;
        }
    }
}
