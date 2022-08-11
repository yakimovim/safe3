namespace EdlinSoftware.Safe.Storage.Model
{
    public interface IFieldVisitor
    {
        void Visit(TextField textField);
        void Visit(PasswordField passwordField);
    }

    public interface IFieldVisitor<out T>
    {
        T Visit(TextField textField);
        T Visit(PasswordField passwordField);
    }
}