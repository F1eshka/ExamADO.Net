using EkzamenADO.DataAccess;
using EkzamenADO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EkzamenADO.Windows
{
    public partial class AdsWindow : Window
    {
        private readonly DbManager db;
        private readonly User currentUser;
        private List<AdWithCategory> myAds;
        private List<AdWithCategory> allAds;

        public AdsWindow(User user)
        {
            InitializeComponent();
            db = new DbManager();
            currentUser = user;
            LoadAds();
        }

        private void LoadAds()
        {
            myAds = db.GetAdsByUser(currentUser.Id);
            allAds = db.GetAllAdsExceptUser(currentUser.Id);

            MyAdsList.ItemsSource = myAds;
            AllAdsList.ItemsSource = allAds;
        }

        private void AddAd_Click(object sender, RoutedEventArgs e)
        {
            AddAdWindow addWindow = new AddAdWindow(currentUser);
            addWindow.ShowDialog();
            LoadAds();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = new ProfileWindow(currentUser);
            profileWindow.ShowDialog();
            LoadAds();
        }

        private void EditAd_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 0 && MyAdsList.SelectedItem is AdWithCategory selected)
            {
                EditAdWindow edit = new EditAdWindow(currentUser, selected.Id);
                edit.ShowDialog();
                LoadAds();
            }
            else
            {
                MessageBox.Show("Оберіть оголошення для редагування");
            }
        }

        private void DeleteAd_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 0 && MyAdsList.SelectedItem is AdWithCategory selected)
            {
                if (MessageBox.Show("Ви впевнені, що хочете видалити оголошення??", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.DeleteAd(selected.Id);
                    LoadAds();
                }
            }
            else
            {
                MessageBox.Show("Оберіть оголошення для видалення:");
            }
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.Trim().ToLower();

            if (MainTabControl.SelectedIndex == 0)
            {
                var filtered = myAds.Where(a => a.Title.ToLower().Contains(query)).ToList();
                MyAdsList.ItemsSource = filtered;
            }
            else if (MainTabControl.SelectedIndex == 1)
            {
                var filtered = allAds.Where(a => a.Title.ToLower().Contains(query)).ToList();
                AllAdsList.ItemsSource = filtered;
            }
        }
    }
}
