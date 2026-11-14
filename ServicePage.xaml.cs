using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _2проект_
{
    /// <summary>
    /// Логика взаимодействия для ServicePage.xaml
    /// </summary>
    public partial class ServicePage : Page
    {
        private List<Product> selectedProducts = new List<Product>();
        private List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        public ServicePage(User user)
        {
            InitializeComponent();
            if (user != null)
            {
                FIOTB.Text = "Вы авторизовались как " + user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;
                switch (user.UserRole)
                {
                    case 1:
                        RoleTB.Text = "Роль: Клиент"; break;
                    case 2:
                        RoleTB.Text = "Роль: Менеджер"; break;
                    case 3:
                        RoleTB.Text = "Роль: Администратор"; break;
                }
            }
            else
            {
                FIOTB.Text = "Гость";
                RoleTB.Text = "Не авторизован";
            }

            var currentServices = Shtein41Entities.GetContext().Product.ToList();
            ServiceListView.ItemsSource = currentServices;
            ComboType.SelectedIndex = 0;
            UpdateServices();
        }
        private void UpdateServices()
        {
            int totalCount = Shtein41Entities.GetContext().Product.ToList().Count;
            var currentServices = Shtein41Entities.GetContext().Product.ToList();
            if (ComboType.SelectedIndex ==0)
            {
                currentServices = currentServices.Where(p=>(p.ProductDiscountAmount >=0 && p.ProductDiscountAmount<=100)).ToList();
            }
            if (ComboType.SelectedIndex == 1)
            {
                currentServices = currentServices.Where(p => (p.ProductDiscountAmount >= 0 && p.ProductDiscountAmount <= 9.99)).ToList();
            }
            if (ComboType.SelectedIndex == 2)
            {
                currentServices = currentServices.Where(p => (p.ProductDiscountAmount >= 10 && p.ProductDiscountAmount <= 14.99)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentServices = currentServices.Where(p => (p.ProductDiscountAmount >= 15 && p.ProductDiscountAmount <= 100)).ToList();
            }
            currentServices = currentServices.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();
            if (RDButtonDown.IsChecked.Value)
            {
                currentServices = currentServices.OrderByDescending(p => p.ProductCost).ToList();
            }
            if (RDButtonUp.IsChecked.Value)
            {
                currentServices = currentServices.OrderBy(p => p.ProductCost).ToList();
            }
            int filteredCount = currentServices.Count;
            TBCount.Text = $"{filteredCount} из {totalCount}";
            ServiceListView.ItemsSource = currentServices;
           
        }


        private void RDButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void RDButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateServices();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServices();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServices();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceListView.SelectedIndex >= 0)
            {
                var prod = ServiceListView.SelectedItem as Product;
                selectedProducts.Add(prod);

                var newOrderProd = new OrderProduct();
                newOrderProd.ProductArticleNumber = prod.ProductArticleNumber;
                newOrderProd.Quantity = "1"; // В БД это строка

                var selOP = selectedOrderProducts.Where(p => p.ProductArticleNumber == prod.ProductArticleNumber);

                if (selOP.Count() == 0)
                {
                    selectedOrderProducts.Add(newOrderProd);
                }
                else
                {
                    foreach (OrderProduct p in selectedOrderProducts)
                    {
                        if (p.ProductArticleNumber == prod.ProductArticleNumber)
                        {
                            // Преобразуем строку в число, прибавляем 1 и возвращаем в строку
                            p.Quantity = (Convert.ToInt32(p.Quantity) + 1).ToString();
                        }
                    }
                }

                OrderBtn.Visibility = Visibility.Visible;
                ServiceListView.SelectedIndex = -1;
            }
        }
        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedProducts = selectedProducts.Distinct().ToList();
            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, FIOTB.Text);
            orderWindow.ShowDialog();
        }
    }
}