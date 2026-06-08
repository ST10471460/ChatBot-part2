/*
St10471460 Dazhaun Thorne Gr2
PROG6221 - Assignent Part2
This is c# console application that provides cybersecurity awareness information through a simple interactive interface.
*/

/* Referemces:
https://copilot.microsoft.com/ {used ai to get colour palettes}
https://github.com/copilot {used gtihub copilot to fix errors with the xaml file, had a error dealing with the assemblywhich it fixed} 
                           {used to make background of app dark by using dynamic resources}
https://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c 
                                                                                                {used to understand how to use async and await with the animation loop} 
https://learn.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2012/hh300224(v=vs.110)

*/

//imports
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cybersecurity_Awareness_GUI
{
    public partial class MainWindow : Window
    {
       
        // MenueTopicInterface provides prewritten topic content shown from the menu
        private readonly MenueTopicInterface menu = new MenueTopicInterface();

        // DictionaryResponces handles free-text response generation
        private readonly DictionaryResponces response = new DictionaryResponces()
            ;
        // BotAsciiArt returns ASCII art for the bot in greeting and main panels
        private readonly BotAsciiArt bot = new BotAsciiArt();

        // Current user's display name for this session
        private string userName = string.Empty;

        // Sound player used to play animation audio for the duration of the animation
        private System.Media.SoundPlayer? animationPlayer;

        //---------------------------------------------------------------------------------------------------------
        private void PlaySound() 
        {
            // play the intro ASCII animation sound
            try
            {
                string animSound = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "final.wav");
                if (System.IO.File.Exists(animSound))
                {
                    // Dispose previous player if any
                    try { animationPlayer?.Stop(); } catch { }
                    animationPlayer = new System.Media.SoundPlayer(animSound);
                    // Play once asynchronously so the audio can play to completion
                    animationPlayer.Play();
                }
            }
            catch
            { }
        }

        //---------------------------------------------------------------------------------------------------------

        // initialize WPF components
        public MainWindow()
        {
            InitializeComponent();
        }

        //---------------------------------------------------------------------------------
        // Application startup flow (runs after Window.Loaded)
        // Transitions through three phases: animation then greeting into main UI
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // phase 1 wall animation
            // visibility of the other phases is toggled in XAML to ensure they don't show briefly during loading

            AnimationPhase.Visibility = Visibility.Visible;
            GreetingPhase.Visibility = Visibility.Collapsed;
            MainPhase.Visibility = Visibility.Collapsed;


            var wall = new ASCIIwallArtAnimation();
            // Start audio and run animation. Ensure audio is stopped when animation ends or fails.
            PlaySound();
            try
            {
                // AnimateASCIIArt writes to the AsciiOutput TextBlock
                await wall.AnimateASCIIArt(AsciiOutput);
            }
            catch { }
            finally
            {
                // Intentionally do not stop the sound here so the audio can play to completion
            }

            // pause before the greeting
            await Task.Delay(300);

            //---------------------------------------------------------------------------------
            // phase 2 
            // greeting phase shows the bot's ASCII art and prompts for the user's name

            AnimationPhase.Visibility = Visibility.Collapsed;
            GreetingPhase.Visibility = Visibility.Visible;

            BotArtGreeting.Text = bot.GetArt();
            GreetingMessage.Text = "Hi, I will be your cyber awareness assistant, CyberBot.\nWhat should I address you as?";
            // Set focus to the name input box for user convenience
            NameInput.Focus();
        }

        // Handle Enter in the name input box
        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ConfirmName();
        }


        // Handle the name submission button click 
        // On Enter key press this handler ensures the same method is called for both events.
        private void NameSubmit_Click(object sender, RoutedEventArgs e) => ConfirmName();

        // Trim the name input, store it show a confirmation message
        private async void ConfirmName()
        {
            // Validate the name input, if it's empty or whitespace, do nothing and wait for valid input
            string name = NameInput.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            userName = name;

            // Show confirmation message briefly
            GreetingMessage.Text = $"Nice to meet you, {userName}.\nPreparing your session…";
            await Task.Delay(1200);


            //---------------------------------------------------------------------------------
            // Phase 3
            // main interface with menu and free-text input

            GreetingPhase.Visibility = Visibility.Collapsed;
            MainPhase.Visibility = Visibility.Visible;

            BotArtMain.Text = bot.GetArt();
            HeaderLabel.Text = $"Cyberbot  //  Session: {userName}";
            BotResponse.Text = $"Welcome, {userName}. Select a topic from the menu or ask me anything.";
        }

        // Menu button handlers

        // show the free-text input 
        private void AskBot_Click(object sender, RoutedEventArgs e)
        {
            AskPanel.Visibility = Visibility.Visible;
            BotResponse.Text = "Type your question below and press SEND or Enter.";
            UserInput.Focus();
        }

        // a short topic provided by MenueTopicInterface
        private void Password_Click(object sender, RoutedEventArgs e)
        {
            AskPanel.Visibility = Visibility.Collapsed;
            BotResponse.Text = menu.PasswordInfo();
        }

        // a short topic provided by MenueTopicInterface
        private void Phishing_Click(object sender, RoutedEventArgs e)
        {
            AskPanel.Visibility = Visibility.Collapsed;
            BotResponse.Text = menu.PhishingInfo();
        }

        // a short topic provided by MenueTopicInterface
        private void Browsing_Click(object sender, RoutedEventArgs e)
        {
            AskPanel.Visibility = Visibility.Collapsed;
            BotResponse.Text = menu.BrowsingInfo();
        }

        // close the application
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            BotResponse.Text = $"It was great chatting with you, {userName}. Stay safe out there.\n Goodbye!";
            Application.Current.Shutdown();
        }

        // Free-text input handling, enter key or send button triggers processing
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessUserInput();
        }

        private void Send_Click(object sender, RoutedEventArgs e) => ProcessUserInput();

      // Process the user's free-text input
        private void ProcessUserInput()
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            string reply = response.HandleFreeText(input);
            BotResponse.Text = reply;

            // Clear the input for the next question
            UserInput.Clear();
            // Set focus back to the input box for convenience
            UserInput.Focus();
        }


    }
}
        