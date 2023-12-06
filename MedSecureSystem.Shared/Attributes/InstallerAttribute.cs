namespace MedSecureSystem.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class InstallerAttribute : Attribute
    {
        public int Position { get; }

        public InstallerAttribute(int position)
        {
            Position = position;
        }
    }
}
