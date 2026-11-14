using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // Обязательно для таймера

namespace _2проект_
{
    public partial class AuthPage : Page
    {
        private string _currentCaptchaCode = "";

        // Переменные для таймера блокировки
        private DispatcherTimer _blockTimer;
        private int _secondsLeft;

        public AuthPage()
        {
            InitializeComponent();
            GenerateCaptcha();
            this.IsVisibleChanged += AuthPage_IsVisibleChanged;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверка капчи
            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(CaptchaTB.Text) || CaptchaTB.Text.Trim().ToUpper() != _currentCaptchaCode)
                {
                    MessageBox.Show("Капча введена неверно! Вход заблокирован на 10 секунд.");
                    StartBlockTimer();
                    return; //Выходим, чтобы не проверять пароль
                }
            }

            //код проверки логина и пароля
            string login = LoginTB.Text;
            string password = PassTB.Text;

            if (login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            User user = Shtein41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {

                Manager.MainFrame.Navigate(new ServicePage(user));
                LoginTB.Text = "";
                PassTB.Text = "";
                CaptchaPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Введены неверные данные");
                CaptchaPanel.Visibility = Visibility.Visible; // Показываем капчу
                GenerateCaptcha();
                CaptchaTB.Text = "";
            }
        }


        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ServicePage(null));
        }





        // генератор капчи
        private void GenerateCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQSTUVWXY23456789";
            Random random = new Random();

            string code = "";
            for (int i = 0; i < 4; i++)
            {
                code += chars[random.Next(chars.Length)];
            }
            _currentCaptchaCode = code;

            captchaOneWord.Text = code[0].ToString();
            captchaTwoWord.Text = code[1].ToString();
            captchaThreeWord.Text = code[2].ToString();
            captchaFourWord.Text = code[3].ToString();
        }




        // Кнопка обновления капчи
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
            CaptchaTB.Text = "";
        }







        // Блокировка на 10 секунд через DispatcherTimer
        private void StartBlockTimer()
        {
            _secondsLeft = 10;
            LoginBtn.IsEnabled = false;
            GuestBtn.IsEnabled = false;
            CaptchaTB.IsEnabled = false;

            _blockTimer = new DispatcherTimer();
            _blockTimer.Interval = TimeSpan.FromSeconds(1);

            _blockTimer.Tick += (s, args) =>
            {
                _secondsLeft--;
                LoginBtn.Content = $"Ждите {_secondsLeft}..."; // Меняем текст на кнопке

                if (_secondsLeft <= 0)
                {
                    _blockTimer.Stop();
                    LoginBtn.Content = "Войти"; // Возвращаем текст
                    LoginBtn.IsEnabled = true;
                    GuestBtn.IsEnabled = true;
                    CaptchaTB.IsEnabled = true;

                    GenerateCaptcha(); // Генерируем новую капчу
                    CaptchaTB.Text = "";
                }
            };

            _blockTimer.Start();
        }
        private void AuthPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Если страница стала видимой (мы на неё вернулись)
            if ((bool)e.NewValue == true)
            {
                // 1. Прячем капчу
                CaptchaPanel.Visibility = Visibility.Collapsed;

                // 2. Очищаем поля ввода
                LoginTB.Text = "";
                PassTB.Text = "";
                CaptchaTB.Text = "";


                // 3. Если таймер блокировки был запущен, его нужно остановить
                if (_blockTimer != null && _blockTimer.IsEnabled)
                {
                    _blockTimer.Stop();
                    LoginBtn.Content = "Войти";
                    LoginBtn.IsEnabled = true;
                    GuestBtn.IsEnabled = true;
                    CaptchaTB.IsEnabled = true;
                }
            }
        }
    }
}