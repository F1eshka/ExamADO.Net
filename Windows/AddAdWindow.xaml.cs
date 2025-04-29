using EkzamenADO.DataAccess;
using EkzamenADO.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace EkzamenADO
{
    public partial class AddAdWindow : Window
    {
        private readonly DbManager db = new DbManager();
        private readonly User currentUser;
        private string selectedImageFileName;

        public AddAdWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            CategoryBox.ItemsSource = db.GetAllCategories();
            CategoryBox.DisplayMemberPath = "Name";
            CategoryBox.SelectedValuePath = "Id";
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Оберіть зображення";
            dialog.Filter = "Зображення (*.jpg;*.png)|*.jpg;*.png";

            if (dialog.ShowDialog() == true)
            {
                string filename = Path.GetFileName(dialog.FileName);
                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string destinationPath = Path.Combine(imagesDir, filename);

                if (!File.Exists(destinationPath))
                    File.Copy(dialog.FileName, destinationPath);

                selectedImageFileName = filename;
                ImageLabel.Content = filename;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryBox.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("Оберіть категорію");
                return;
            }

            var ad = new Ad
            {
                Title = TitleBox.Text,
                Description = DescriptionBox.Text,
                Price = decimal.TryParse(PriceBox.Text, out decimal price) ? price : 0,
                CategoryId = selectedCategory.Id,
                ImageFileName = selectedImageFileName,
                UserId = currentUser.Id
            };

            db.AddAd(ad);
            MessageBox.Show("Оголошення додано");
            this.Close();
        }
    }
}
