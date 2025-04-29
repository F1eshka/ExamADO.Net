using System.Text.RegularExpressions;
using System.Windows;
using EkzamenADO.DataAccess;
using EkzamenADO.Models;

namespace EkzamenADO
{
    public partial class ProfileWindow : Window
    {
        private readonly DbManager db = new DbManager();
        private readonly User currentUser;

        public ProfileWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            NameBox.Text = currentUser.Name;
            EmailBlock.Text = currentUser.Email;
            PhoneBox.Text = currentUser.Phone;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePhone(PhoneBox.Text))
            {
                MessageBox.Show("Невірний формат телефону! Введіть лише цифри (наприклад 0987654321)", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentUser.Name = NameBox.Text;
            currentUser.Phone = PhoneBox.Text;
            db.UpdateUser(currentUser);

            MessageBox.Show("Дані успішно збережені!!", "Готово ))", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете видалити акаунт? Це неможливо буде скасувати", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                db.DeleteUser(currentUser.Id);
                MessageBox.Show("Ваш акаунт був видалений.", "Видалення акаунту", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }
        }

        private bool ValidatePhone(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{9,12}$"); 
        }
    }
}
