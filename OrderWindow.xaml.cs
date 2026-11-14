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
using System.Windows.Shapes;

namespace _2проект_
{
    public partial class OrderWindow : Window
    {
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();

        public OrderWindow(List<OrderProduct> orderProducts, List<Product> products, string FIO)
        {
            InitializeComponent();

            this.selectedOrderProducts = orderProducts;
            this.selectedProducts = products;

            var currentPickups = Shtein41Entities.GetContext().PickupPoint.ToList();
            PickupCombo.ItemsSource = currentPickups;
            PickupCombo.SelectedIndex = 0;

            ClientTB.Text = FIO;

            int nextId = Shtein41Entities.GetContext().Order.Any()
                ? Shtein41Entities.GetContext().Order.Max(o => o.OrderID) + 1
                : 1;
            TBOrderID.Text = nextId.ToString();

            ShoeListView.ItemsSource = this.selectedOrderProducts;

            OrderDP.Text = DateTime.Now.ToString("dd.MM.yyyy");
            SetDeliveryDate();
        }

        // Метод для расчета даты доставки (добавлен, так как его не хватало)
        private void SetDeliveryDate()
        {
            bool allInStock = selectedProducts.All(p => p.ProductQuantityInStock > 3);
            int days = allInStock ? 3 : 6;
            DeliveryDP.SelectedDate = DateTime.Now.AddDays(days);
        }

        // Обработчик кнопки "+"
        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, на которую нажали
            var button = sender as Button;
            if (button == null) return;

            // DataContext в ListView у нас - это OrderProduct
            var selectedOP = button.DataContext as OrderProduct;

            if (selectedOP != null)
            {
                // Увеличиваем количество, преобразуя строку в число
                int currentQty = Convert.ToInt32(selectedOP.Quantity);
                selectedOP.Quantity = (currentQty + 1).ToString();

                SetDeliveryDate();
                ShoeListView.Items.Refresh();
            }
        }

        // Обработчик кнопки "-"
        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, на которую нажали
            var button = sender as Button;
            if (button == null) return;

            // Берем OrderProduct из DataContext
            var selectedOP = button.DataContext as OrderProduct;

            if (selectedOP != null)
            {
                // Уменьшаем количество
                int currentQty = Convert.ToInt32(selectedOP.Quantity) - 1;
                selectedOP.Quantity = currentQty.ToString();

                // Если количество стало 0 — удаляем товар из заказа
                if (currentQty <= 0)
                {
                    selectedOrderProducts.Remove(selectedOP);

                    // Также удаляем сам товар из списка selectedProducts
                    var prodToRemove = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == selectedOP.ProductArticleNumber);
                    if (prodToRemove != null)
                    {
                        selectedProducts.Remove(prodToRemove);
                    }
                }

                SetDeliveryDate();
                ShoeListView.Items.Refresh();
            }
        }

        // Обработчик кнопки "Сохранить"
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderProducts == null || selectedOrderProducts.Count == 0)
            {
                MessageBox.Show("В заказе нет товаров!");
                return;
            }

            try
            {
                Order newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    OrderDeliveryDate = DeliveryDP.SelectedDate ?? DateTime.Now.AddDays(3),
                    OrderPickupPoint = (PickupCombo.SelectedItem as PickupPoint)?.PickupPointID ?? 0,
                    OrderStatus = "Новый"
                };

                Shtein41Entities.GetContext().Order.Add(newOrder);
                Shtein41Entities.GetContext().SaveChanges();

                foreach (var op in selectedOrderProducts)
                {
                    op.OrderID = newOrder.OrderID;

                    if (!Shtein41Entities.GetContext().OrderProduct.Local.Contains(op))
                    {
                        Shtein41Entities.GetContext().OrderProduct.Add(op);
                    }
                }

                Shtein41Entities.GetContext().SaveChanges();
                MessageBox.Show("Заказ успешно оформлен!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }
    }
}