using System;
using System.Windows;
using System.Windows.Media;

namespace VoidwareDLLInjector
{
    public partial class SettingsWindow : Window
    {
        // Public properties to store the color selections
        public Color SelectedBackgroundColor { get; private set; }
        public Color SelectedButtonColor { get; private set; }
        public Color SelectedSecondaryColor { get; private set; }
        public Color SelectedFontColor { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            // Kicking off with our default vibes. Ready to paint the app in colors that feel right.
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Mixing the perfect shade from our RGB sliders. This is where we bring the magic to life.
            SelectedBackgroundColor = Color.FromRgb(
                (byte)BgRedSlider.Value,
                (byte)BgGreenSlider.Value,
                (byte)BgBlueSlider.Value);

            SelectedButtonColor = Color.FromRgb(
                (byte)BtnRedSlider.Value,
                (byte)BtnGreenSlider.Value,
                (byte)BtnBlueSlider.Value);

            SelectedSecondaryColor = Color.FromRgb(
                (byte)SecRedSlider.Value,
                (byte)SecGreenSlider.Value,
                (byte)SecBlueSlider.Value);

            SelectedFontColor = Color.FromRgb(
                (byte)FontRedSlider.Value,
                (byte)FontGreenSlider.Value,
                (byte)FontBlueSlider.Value);

            // Settings locked in. Time to make these colors dance across the app.
            this.DialogResult = true;
            this.Close();

            // And just like that, we're done here. Finally.
            // Celebratory vibes only from here on out. 💨🍃
        }
    }
}
