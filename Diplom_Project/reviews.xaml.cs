using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Windows;

namespace Diplom_Project.Views
{
    public partial class ReviewsWindow : Window
    {
        private readonly int _currentUserId;

        public ReviewsWindow(UserModel user) : this(user.Id)
        {
            InitializeComponent();
            _currentUserId = user.Id;
            LoadUserReviews();
            LoadUserInfo();
        }

        public ReviewsWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadUserReviews();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            var user = DatabaseHelper.GetUserById(_currentUserId);
            if (user != null)
            {
                UserInfoText.Text = $"Вы вошли как: {user.FirstName}";
            }
        }

        private void LoadUserReviews()
        {
            var reviews = DatabaseHelper.GetUserReviews(_currentUserId);
            ReviewsListView.ItemsSource = reviews;
        }

        private void RatingComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно добавить логику при изменении оценки
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (RatingComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem &&
                int.TryParse(selectedItem.Content?.ToString(), out int rating))
            {
                var review = new Review
                {
                    UserId = _currentUserId,
                    Content = ReviewContentTextBox.Text,
                    Rating = rating
                };

                if (DatabaseHelper.AddReview(review))
                {
                    MessageBox.Show("Отзыв успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ReviewContentTextBox.Clear();
                    LoadUserReviews(); // Обновляем список отзывов
                }
                else
                {
                    MessageBox.Show("Не удалось добавить отзыв.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите оценку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}