using LiteDB;

namespace EdlinSoftware.Safe.Storage.Model
{
    public abstract class Field
    {
        public string Name { get; set; }

        public abstract void Visit(IFieldVisitor visitor);

        public abstract T Visit<T>(IFieldVisitor<T> visitor);
    }

    public sealed class TextField : Field
    {
        [BsonField("Value")]
        public string Text { get; set; }

        public override void Visit(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Visit<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public sealed class PasswordField : Field
    {
        [BsonField("Value")]
        public string Password { get; set; }

        public override void Visit(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override T Visit<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}