namespace tvdc.Plugin.Models
{
    public class User
    {

        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Id { get; private set; }

        public User(string name, string displayName, string id)
        {
            Name = name;
            DisplayName = displayName;
            Id = id;
        }

    }
}
