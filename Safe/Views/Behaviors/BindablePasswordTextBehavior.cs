using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace EdlinSoftware.Safe.Views.Behaviors
{
    public class BindablePasswordTextBehavior : Behavior<PasswordBox>
    {
        private bool _isUpdating;

        #region SelectedItem Property

        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text), 
                typeof(string), 
                typeof(BindablePasswordTextBehavior), 
                new FrameworkPropertyMetadata(string.Empty, OnPasswordTextChanged));

        private static void OnPasswordTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string password)
            {
                var behaviour = (BindablePasswordTextBehavior)sender;

                if (!behaviour._isUpdating)
                {
                    var passwordBox = behaviour.AssociatedObject;

                    if (passwordBox != null)
                    {
                        passwordBox.PasswordChanged -= behaviour.OnPasswordChanged;

                        passwordBox.Password = password;

                        passwordBox.PasswordChanged += behaviour.OnPasswordChanged;
                    }
                }
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PasswordChanged += OnPasswordChanged;

            AssociatedObject.Password = (string) Text;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.PasswordChanged -= OnPasswordChanged;
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            _isUpdating = true;
            Text = AssociatedObject.Password;
            _isUpdating = false;
        }
    }
}
