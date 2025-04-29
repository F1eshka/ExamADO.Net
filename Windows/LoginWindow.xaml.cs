using System.Windows;
using EkzamenADO.DataAccess;
using EkzamenADO.Models;
using EkzamenADO.Windows;


namespace EkzamenADO
{
    public partial class LoginWindow : Window
    {
        private DbManager db;

        public LoginWindow()
        {
            InitializeComponent();
            db = new DbManager();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            var user = db.Login(email, password);

            if (user != null)
            {
                MessageBox.Show("Успішний вхід!!))", "Вітаю");

                AdsWindow adsWindow = new AdsWindow(user);
                adsWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль((");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text;
            string password = PasswordBox.Password;

            var user = new User
            {
                Name = "Користувач",
                Email = email,
                Phone = "0000000000"
            };

            bool success = db.RegisterUser(user, password);

            if (success)
                MessageBox.Show("Реєстрація успішна!!))");
            else
                MessageBox.Show("Цей email вже зареєстрований ((");
        }
    }
}
