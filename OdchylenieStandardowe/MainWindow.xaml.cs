////////////////////////////////////////////////////////
//
// Project: "Obliczanie odchylenia standardowego próby"
// Author: Szczepan Dwornicki
// Date: 5th term, 2022 / 2023
//
////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using HllDll;
using System.Threading.Tasks;
using System.IO;

namespace OdchylenieStandardowe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private float[] sample;
        private int numberOfElements;
        // Describes whether selected file contains valid data and deviation can be calculated
        private bool selectedFileValid = false;
        public string[] modes { get; set; }
        private float result;
        private int runNumber;
        private double time;

        public MainWindow()
        {
            InitializeComponent();
            modes = new string[] { "Asm", "C#" };
            DataContext = this;
            runNumber = 0;
        }

        [DllImport(@"..\AsmDll.dll")]
        static unsafe extern float standardDeviationAsm(float* sample, int numberOfElements);

        private async void calculateStandardDeviationButtonOnClick(object sender, RoutedEventArgs e)
        {
            // Disable buttons
            calculateStandardDeviationButton.IsEnabled = false;
            selectFileButton.IsEnabled = false;
            comboBox.IsEnabled = false;
            button.IsEnabled = false;
        

            String fileName = selectedFileTextBox.Text.Substring(selectedFileTextBox.Text.LastIndexOf('\\') + 1);
            resultTextBox.Text = "";

            int comboBoxSelectedIndex = comboBox.SelectedIndex;

            if (comboBoxSelectedIndex != 0 && comboBoxSelectedIndex != 1)    // No mode selected 
            {
                // Show info
                MessageBox.Show("Wybierz tryb obliczeń.",
                            "Wybierz tryb", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            if (selectedFileValid)
            {
                // Update status label
                statusLabel.Visibility = Visibility.Visible;
                statusLabel.Content = "Wykonywanie obliczeń...";

                // Create a copy of sample array, so Asm and Hll programs can be simplify
                float[] elements = new float[numberOfElements];

                // Shallow copy
                elements = sample.ToArray();

                await calculateStandardDeviation(comboBoxSelectedIndex, elements);

                resultTextBox.Text = result.ToString();

                if (comboBoxSelectedIndex == 0)   // Asm
                {
                    dataGrid.Items.Add(new DataGridItem(runNumber, "Asm", time, result, fileName));
                }
                else if (comboBoxSelectedIndex == 1)    // C#
                {
                    dataGrid.Items.Add(new DataGridItem(runNumber, "C#", time, result, fileName));
                }

                // Update status label
                statusLabel.Visibility = Visibility.Hidden;

            }
            else   // No proper file selected
            {
                // Show info
                MessageBox.Show("Wybierz plik z poprawnymi danymi.",
                            "Niepoprawne dane wejściowe", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Enable buttons
            calculateStandardDeviationButton.IsEnabled = true;
            selectFileButton.IsEnabled = true;
            comboBox.IsEnabled = true;
            button.IsEnabled = true;
        }

        private async Task calculateStandardDeviation(int comboBoxSelectedIndex, float[] elements)
        {
            await Task.Run(() =>
            {
                unsafe
                {
                    runNumber++;

                    fixed (float* elements_ptr = &elements[0]) // selectedFileValid is true, so there are valid elements
                        if (comboBoxSelectedIndex == 0)    // Asm
                        {
                            Stopwatch sw = new Stopwatch();

                            sw.Start();

                            result = standardDeviationAsm(elements_ptr, numberOfElements);

                            sw.Stop();

                            time = sw.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
                        }
                        else if (comboBoxSelectedIndex == 1)   // C#
                        {
                            Stopwatch sw = new Stopwatch();

                            sw.Start();

                            result = Hll.standardDeviationHll(elements_ptr, numberOfElements);

                            //System.Threading.Thread.Sleep(10);

                            sw.Stop();

                            time = sw.ElapsedTicks / (double)Stopwatch.Frequency * 1000.0;
                        }

                   // return true;
                }
            });
        }

        private void selectFileButtonOnClick(object sender, RoutedEventArgs e)
        {
            // Update status label
            statusLabel.Visibility = Visibility.Visible;
            statusLabel.Content = "Wybierz plik i poczekaj, aż się załaduje";           

            // Alignment of data in the array
            int alignment = 0;

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = "txt files (*.txt)|*.txt|csv files (*.csv)|*.csv"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();
            
            // Process open file dialog box results
            if (result == true)
            {
                // Open file
                string filename = dialog.FileName;

                // Update selected file name
                selectedFileTextBox.Text = filename;

                // Read each line (element of a sample) of the file
                string[] elements = System.IO.File.ReadAllLines(filename);

                // Make sure there is any data
                numberOfElements = elements.Length;
                alignment = numberOfElements % 8;

                if(alignment != 0)
                {
                    alignment = 8 - alignment;
                }

                if (numberOfElements > 0)
                {
                    // File has any content, so assume that selectedFileValid is true
                    selectedFileValid = true;
                    sample = new float[numberOfElements + alignment];

                    for(int i = 0; i < numberOfElements; i++)
                    {
                        if(!Single.TryParse(elements[i], out sample[i]))
                        {
                            selectedFileValid = false;

                            // Show info
                            MessageBox.Show("Wybrany plik zawiera niepoprawne dane. " +
                                "Upewnij się, że w każdej lini pliku znajduje się " +
                                "liczba dziesiętna.",
                                "Niepoprawne dane wejściowe", MessageBoxButton.OK, MessageBoxImage.Error);

                            break;
                        }
                    }

                    // Theoretically not needed (it can contain trash)
                    for (int i = numberOfElements; i < numberOfElements + alignment; i++)
                    {
                        sample[i] = 0f;
                    }
                }
                else
                {
                    selectedFileValid = false;

                    // Show info
                    MessageBox.Show("Wybrano pusty plik.",
                                "Niepoprawne dane wejściowe", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

            // Update status label
            statusLabel.Visibility = Visibility.Hidden;

        }

        // Set default value in comboBox
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox.SelectedIndex = 0;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.Items.Clear();
            runNumber = 0;
        }

        // Not used but needed to compile
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
