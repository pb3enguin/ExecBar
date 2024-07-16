using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;

namespace ExecBar
{
    public partial class MainWindow : Window
    {
        private List<ActionItem> actions;
        private static IKeyboardMouseEvents globalHook;
        private const string ActionsFilePath = "actions.json";
        private bool launchedViaHotkey = false; // Flag to track hotkey launch

        public MainWindow()
        {
            InitializeComponent();
            LoadActions();
            InitializeGlobalHotkey();
            InputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
            this.Loaded += MainWindow_Loaded;
        }

        private void LoadActions()
        {
            if (File.Exists(ActionsFilePath))
            {
                var json = File.ReadAllText(ActionsFilePath);
                actions = JsonSerializer.Deserialize<List<ActionItem>>(json) ?? new List<ActionItem>();
            }
            else
            {
                actions = new List<ActionItem>();
            }
        }

        private void SaveActions()
        {
            var json = JsonSerializer.Serialize(actions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ActionsFilePath, json);
        }

        private void InitializeGlobalHotkey()
        {
            if (globalHook == null)
            {
                globalHook = Hook.GlobalEvents();
                globalHook.KeyDown += GlobalHook_KeyDown;
            }
        }

        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == System.Windows.Forms.Keys.Space)
            {
                launchedViaHotkey = true; // Set flag to true when launched via hotkey
                if (this.Visibility != Visibility.Visible)
                {
                    this.Show();
                    this.Activate();
                    InputTextBox.Focus(); // Ensure the input textbox is focused
                }
            }
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (launchedViaHotkey && e.Key == Key.Space)
            {
                // Ignore the space key event triggered by Ctrl+Space
                e.Handled = true;
                launchedViaHotkey = false; // Reset the flag after handling
                return;
            }

            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
            else if (e.Key == Key.Down)
            {
                MoveSelection(1);
                e.Handled = true; // Prevent default handling
            }
            else if (e.Key == Key.Up)
            {
                MoveSelection(-1);
                e.Handled = true; // Prevent default handling
            }
            else if (e.Key == Key.Enter)
            {
                ExecuteTopSuggestion();
                e.Handled = true; // Prevent default handling
            }
        }

        private void InputTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string query = InputTextBox.Text;
            var suggestions = actions
                .Where(a => a.ActionName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.ActionName)
                .ToList();

            SuggestionsListBox.ItemsSource = suggestions;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus(); // Set focus to the input textbox when the window is loaded
        }

        private void MoveSelection(int direction)
        {
            int newIndex = SuggestionsListBox.SelectedIndex + direction;
            if (newIndex >= 0 && newIndex < SuggestionsListBox.Items.Count)
            {
                SuggestionsListBox.SelectedIndex = newIndex;
                SuggestionsListBox.ScrollIntoView(SuggestionsListBox.SelectedItem);
            }
        }

        private void ExecuteTopSuggestion()
        {
            if (SuggestionsListBox.Items.Count > 0)
            {
                var action = (ActionItem)SuggestionsListBox.SelectedItem ?? (ActionItem)SuggestionsListBox.Items[0];
                ExecuteAction(action);
            }
        }

        private void ExecuteAction(ActionItem action)
        {
            try
            {
                if (Directory.Exists(action.Path))
                {
                    Process.Start("explorer.exe", action.Path);
                }
                else if (File.Exists(action.Path))
                {
                    Process.Start(new ProcessStartInfo(action.Path) { UseShellExecute = true });
                }
                else
                {
                    throw new FileNotFoundException("The specified file or folder does not exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Clear the input text after executing the action
                InputTextBox.Text = string.Empty;
                SuggestionsListBox.ItemsSource = null;
                this.Hide();
            }
        }

        private void SuggestionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is ActionItem action)
            {
                ExecuteAction(action);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addActionWindow = new AddActionWindow();
            if (addActionWindow.ShowDialog() == true)
            {
                actions.Add(addActionWindow.ActionItem);
                SaveActions();
                InputTextBox_KeyUp(null, null);
            }
        }

        private void ModifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is ActionItem selectedAction)
            {
                var modifyActionWindow = new AddActionWindow(selectedAction);
                if (modifyActionWindow.ShowDialog() == true)
                {
                    var index = actions.FindIndex(a => a.ActionName == selectedAction.ActionName && a.Path == selectedAction.Path);
                    if (index >= 0)
                    {
                        actions[index] = modifyActionWindow.ActionItem;
                        SaveActions();
                        InputTextBox_KeyUp(null, null);
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem is ActionItem selectedAction)
            {
                actions.Remove(selectedAction);
                SaveActions();
                InputTextBox_KeyUp(null, null);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            globalHook.KeyDown -= GlobalHook_KeyDown;
            globalHook.Dispose();
            base.OnClosed(e);
        }
    }
}
