using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wu.CommTool.Extensions
{
    /// <summary>
    /// 扩展PasswordBox以支持绑定  在前端需要引用该命名空间
    /// </summary>
    public class PasswordExtensions
    {
        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordExtensions), new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passWord = sender as PasswordBox;
            string password = (string)e.NewValue;

            if (passWord != null && passWord.Password != password)
                passWord.Password = password;
        }
    }

    /// <summary>
    /// 行为会附加在Password上
    /// </summary>
    public class PasswordBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            //添加密码修改事件
            AssociatedObject.PasswordChanged += AssociatedObject_PasswordChanged;
        }

        /// <summary>
        /// 密码修改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            string password = PasswordExtensions.GetPassword(passwordBox);

            if (passwordBox != null && passwordBox.Password != password)
                PasswordExtensions.SetPassword(passwordBox, passwordBox.Password);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            //注销事件
            AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;
        }
    }
}
