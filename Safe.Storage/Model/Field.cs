namespace EdlinSoftware.Safe.Storage.Model
{
    public abstract class Field
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string Name { get; set; }
    }

    public sealed class TextField : Field
    {
        public string Text { get; set; }
    }

    public sealed class PasswordField : Field
    {
        public string Password { get; set; }
    }
}