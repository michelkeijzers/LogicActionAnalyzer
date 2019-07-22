
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LogicalActionAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-EN");

            radioButtonTextBox.IsChecked = true;
            buttonRestart.IsEnabled = false;
        }


        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            textBoxLaFile.Text = "";
            textBoxAnalysis.Text = "";

            if (IsRadioButtonSelected(radioButtonFolder))
            {
                Analyze();
            }
            else if (IsRadioButtonSelected(radioButtonTextBox))
            {
                textBoxLaFile.Focus();
            }
        }


        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBoxFolder.Text = dialog.FileName;
            }
        }


        private void RadioButtonFolder_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFolder.IsEnabled = true;
            buttonBrowse.IsEnabled = true;
            textBoxLaFile.IsEnabled = false;

            textBoxFolder.Focus();

            Analyze();
        }

        private void RadioButtonTextBox_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFolder.IsEnabled = false;
            buttonBrowse.IsEnabled = false;
            textBoxLaFile.IsEnabled = true;

            textBoxLaFile.Focus();

            Analyze();
        }


        private void TextBoxFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Analyze();
        }


        private void TextBoxLaFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Analyze();
        }


        private void CheckBoxLoopDetails_Click(object sender, RoutedEventArgs e)
        {
            Analyze();
        }


        private void Analyze()
        {
            int nrOfFiles = 0;
            StringBuilder output = new StringBuilder();
            Parser parser = new Parser(
                checkBoxLoopDetails.IsChecked.HasValue && checkBoxLoopDetails.IsChecked.Value);

            if (IsRadioButtonSelected(radioButtonFolder) &&
                !textBoxFolder.Text.Equals(""))
            {
                nrOfFiles = AnalyzeFolder(parser, output);
            }
            else if (IsRadioButtonSelected(radioButtonTextBox) && 
                !textBoxLaFile.Text.Equals(""))
            {
                nrOfFiles = 1;
                var lines = textBoxLaFile.Text.Split('\n');
                output.Append(parser.Parse(lines));
            }

            buttonRestart.IsEnabled = true;

            if (nrOfFiles > 0)
            {
                var header = CreateHeader(nrOfFiles);
                textBoxAnalysis.Text = header + output.ToString();
            }
            else
            {
                textBoxAnalysis.Text = "";
            }
        }


        string CreateHeader(int nrOfFiles)
        {
            var header = new StringBuilder();

            header.AppendLine("Logical Action Analyzer");
            header.AppendLine("=======================\n");
            header.AppendFormat("Date/Time: {0}, {1}\n", 
                DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());

            if (IsRadioButtonSelected(radioButtonFolder))
            {
                header.AppendFormat("Folder: {0}\n", textBoxFolder.Text);
            }

            header.AppendFormat("Number of Files: {0}\n\n\n", nrOfFiles);
            return header.ToString();
        }


        private int AnalyzeFolder(Parser parser, StringBuilder output)
        {
            int nrOfFiles = 0;

            try
            {
                using (new WaitCursor())
                {

                    Thread.Sleep(10);
                    foreach (var fileName in Directory.GetFiles(textBoxFolder.Text))
                    {
                        if (fileName.Contains("_LA_") && fileName.EndsWith(".c"))
                        {
                            nrOfFiles++;
                            output.AppendLine("File Name: " + fileName);

                            var lines = File.ReadAllText(fileName).Split('\n');
                            output.AppendLine(parser.Parse(lines));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return nrOfFiles;
        }


        private bool IsRadioButtonSelected(RadioButton radioButton)
        {
            return (radioButton.IsChecked.HasValue && 
                    radioButton.IsChecked.Value);
        }
    }
}
