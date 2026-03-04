using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AdvGenPriceComparer.WPF.Chat.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.WPF.Chat
{
    public partial class PriceChatWindow : Window
    {
        private ChatViewModel? _viewModel;

        public PriceChatWindow()
        {
            InitializeComponent();
            Loaded += PriceChatWindow_Loaded;
        }

        private void PriceChatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get ViewModel from DI container
                _viewModel = App.ServiceProvider?.GetService<ChatViewModel>();

                if (_viewModel == null)
                {
                    MessageBox.Show(
                        "Could not initialize chat service. Please restart the application.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Close();
                    return;
                }

                DataContext = _viewModel;

                // Subscribe to messages collection changes to auto-scroll
                _viewModel.Messages.CollectionChanged += (s, args) =>
                {
                    Dispatcher.BeginInvoke(() => ScrollToBottom(), DispatcherPriority.Background);
                };

                // Add welcome message
                if (_viewModel.Messages.Count == 0)
                {
                    _viewModel.Messages.Add(new Models.ChatMessage
                    {
                        Role = Models.MessageRole.Assistant,
                        Content = "Hello! I'm your AI grocery price assistant. \n\nI can help you with:\n• Finding product prices\n• Comparing prices between stores\n• Discovering the best deals\n• Showing items by category\n• Answering general questions\n\nWhat would you like to know?"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing chat: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ScrollToBottom()
        {
            if (MessagesScrollViewer != null)
            {
                MessagesScrollViewer.ScrollToEnd();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MessageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftShift))
            {
                e.Handled = true;
                if (_viewModel?.SendMessageCommand.CanExecute(null) == true)
                {
                    _viewModel.SendMessageCommand.Execute(null);
                }
            }
        }
    }
}
