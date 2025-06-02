using Diplom_Project.Models;
using Diplom_Project.Services;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Diplom_Project.Views
{
    public partial class ReviewsWindow : Window
    {
        private readonly int _currentUserId;
        private readonly bool _isAdmin;

        public ReviewsWindow(UserModel user) : this(user.Id, user.Role == "admin")
        {
            InitializeComponent();
            _currentUserId = user.Id;
            _isAdmin = user.Role == "admin";
            LoadAllReviews();
            UpdateAverageRating();
        }

        public ReviewsWindow(int userId, bool isAdmin = false)
        {
            InitializeComponent();
            _currentUserId = userId;
            _isAdmin = isAdmin;
            LoadAllReviews();
            UpdateAverageRating();
        }

        private void LoadAllReviews()
        {
            List<Review> reviews = DatabaseHelper.GetAllReviews();
            ReviewsListView.ItemsSource = reviews;
        }

        private void UpdateAverageRating()
        {
            List<Review> reviews = DatabaseHelper.GetAllReviews();
            if (reviews.Any())
            {
                double average = reviews.Average(r => r.Rating);
                AverageRatingText.Text = $"{average:F1} (всего отзывов: {reviews.Count})";
            }
            else
            {
                AverageRatingText.Text = "нет отзывов";
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (RatingComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem &&
                int.TryParse(selectedItem.Content?.ToString()?.Split(' ')[0], out int rating))
            {
                UserModel currentUser = DatabaseHelper.GetUserById(_currentUserId);

                var review = new Review
                {
                    UserId = _currentUserId,
                    User = currentUser,
                    Content = ReviewContentTextBox.Text,
                    Rating = rating,
                    CreatedAt = DateTime.Now
                };

                if (DatabaseHelper.AddReview(review))
                {
                    MessageBox.Show("Отзыв успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ReviewContentTextBox.Clear();
                    LoadAllReviews();
                    UpdateAverageRating();
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

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}