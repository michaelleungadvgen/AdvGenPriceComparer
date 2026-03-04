using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Chat.Models;
using AdvGenPriceComparer.WPF.Chat.Services;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.Chat.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly IOllamaService _ollamaService;
        private readonly IQueryRouterService _queryRouter;
        private readonly ILoggerService _logger;

        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ObservableCollection<string> SuggestedQuestions { get; } = new();

        private string _userInput = string.Empty;
        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged();
            }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
            }
        }

        private bool _isOllamaAvailable;
        public bool IsOllamaAvailable
        {
            get => _isOllamaAvailable;
            set
            {
                _isOllamaAvailable = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendMessageCommand { get; }
        public ICommand ClearChatCommand { get; }
        public ICommand UseSuggestionCommand { get; }

        public ChatViewModel(
            IOllamaService ollamaService,
            IQueryRouterService queryRouter,
            ILoggerService logger)
        {
            _ollamaService = ollamaService;
            _queryRouter = queryRouter;
            _logger = logger;

            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), () => !IsProcessing && !string.IsNullOrWhiteSpace(UserInput));
            ClearChatCommand = new RelayCommand(ClearChat);
            UseSuggestionCommand = new RelayCommand<string>(async (suggestion) =>
            {
                UserInput = suggestion;
                await SendMessageAsync();
            });

            InitializeSuggestedQuestions();
            _ = CheckOllamaAvailabilityAsync();
        }

        private void InitializeSuggestedQuestions()
        {
            SuggestedQuestions.Add("What's the price of milk?");
            SuggestedQuestions.Add("Find the cheapest bread");
            SuggestedQuestions.Add("Show me items on sale");
            SuggestedQuestions.Add("Compare egg prices between stores");
            SuggestedQuestions.Add("What are the best deals this week?");
            SuggestedQuestions.Add("Show me dairy products");
        }

        private async Task CheckOllamaAvailabilityAsync()
        {
            try
            {
                IsOllamaAvailable = await _ollamaService.IsAvailableAsync();
                StatusMessage = IsOllamaAvailable
                    ? "AI Assistant is ready"
                    : "Ollama is not running. Please start Ollama to use the chat feature.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking Ollama availability: {ex.Message}");
                IsOllamaAvailable = false;
                StatusMessage = "Could not connect to Ollama. Please ensure it's installed and running.";
            }
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(UserInput))
                return;

            var userMessage = UserInput.Trim();
            UserInput = string.Empty;

            // Add user message to chat
            Messages.Add(new ChatMessage
            {
                Role = MessageRole.User,
                Content = userMessage
            });

            IsProcessing = true;

            try
            {
                // Show thinking indicator
                var thinkingMessage = new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    Content = "Thinking...",
                    IsThinking = true
                };
                Messages.Add(thinkingMessage);

                // Extract intent
                var intent = await _ollamaService.ExtractIntentAsync(userMessage);
                _logger.LogInfo($"Detected intent: {intent.Type}");

                // Execute query
                var queryResult = await _queryRouter.ExecuteQueryAsync(intent);

                // Generate response
                string responseText;
                if (intent.Type == QueryType.GeneralChat)
                {
                    responseText = await _ollamaService.ChatAsync(userMessage,
                        "You are a helpful grocery price assistant. Answer general questions in a friendly manner.");
                }
                else
                {
                    responseText = await _ollamaService.GenerateResponseAsync(intent, queryResult);
                }

                // Remove thinking message
                Messages.Remove(thinkingMessage);

                // Add assistant response
                Messages.Add(new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    Content = responseText,
                    AttachedItems = queryResult.RelatedItems,
                    AttachedPrices = queryResult.RelatedPrices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Chat error: {ex.Message}");

                // Remove thinking message if present
                var thinkingMsg = Messages.FirstOrDefault(m => m.IsThinking);
                if (thinkingMsg != null)
                    Messages.Remove(thinkingMsg);

                Messages.Add(new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    Content = "I'm sorry, I encountered an error. Please try again or rephrase your question.",
                    IsError = true
                });
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearChat()
        {
            Messages.Clear();
            _ollamaService.ClearHistory();

            // Add welcome message
            Messages.Add(new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "Hello! I'm your grocery price assistant. Ask me about prices, deals, products, or stores. For example:\n\n• \"What's the price of milk?\"\n• \"Find the cheapest bread\"\n• \"Show me items on sale\"\n• \"What are the best deals?\""
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
