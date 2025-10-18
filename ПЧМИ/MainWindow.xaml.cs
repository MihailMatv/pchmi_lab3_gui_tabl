// MainWindow.xaml.cs
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FileManagerWPF
{
    public partial class MainWindow : Window
    {
        private string currentDirectory;

        public MainWindow()
        {
            InitializeComponent();
            currentDirectory = Directory.GetCurrentDirectory();
            CurrentDirectoryTextBox.Text = currentDirectory;
            PopulateDirectoryTree(currentDirectory);
            PopulateFilesList(currentDirectory);
        }

        private void PopulateDirectoryTree(string path)
        {
            DirectoryTreeView.Items.Clear();
            try
            {
                DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);
                TreeViewItem rootNode = CreateDirectoryNode(rootDirectoryInfo);
                DirectoryTreeView.Items.Add(rootNode);
                rootNode.IsExpanded = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки дерева каталогов: {ex.Message}");
            }
        }

        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            TreeViewItem node = new TreeViewItem() { Header = directoryInfo.Name, Tag = directoryInfo.FullName };
            try
            {
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    node.Items.Add(CreateDirectoryNode(dir));
                }
            }
            catch { }
            return node;
        }

        private void PopulateFilesList(string path)
        {
            FilesListBox.Items.Clear();
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                foreach (var file in dirInfo.GetFiles())
                {
                    FilesListBox.Items.Add(file.Name);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки списка файлов: {ex.Message}");
            }
        }

        private void DirectoryTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DirectoryTreeView.SelectedItem is TreeViewItem item)
            {
                string path = item.Tag as string;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    currentDirectory = path;
                    CurrentDirectoryTextBox.Text = currentDirectory;
                    PopulateFilesList(currentDirectory);
                    ClearFileInfo();
                }
            }
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesListBox.SelectedItem is string selectedFile)
            {
                string fullPath = System.IO.Path.Combine(currentDirectory, selectedFile);
                if (File.Exists(fullPath))
                {
                    DisplayFileInfo(fullPath);
                }
                else
                {
                    ClearFileInfo();
                }
            }
        }

        private void DisplayFileInfo(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                FileInfoTextBlock.Text = $"Имя файла: {fileInfo.Name}\n" +
                                         $"Дата создания: {fileInfo.CreationTime}\n" +
                                         $"Размер: {fileInfo.Length} байт\n" +
                                         $"Атрибуты: {fileInfo.Attributes}";

                // Установка чекбоксов по атрибутам
                ReadOnlyCheckBox.IsChecked = fileInfo.IsReadOnly;
                HiddenCheckBox.IsChecked = fileInfo.Attributes.HasFlag(FileAttributes.Hidden);
                ArchiveCheckBox.IsChecked = fileInfo.Attributes.HasFlag(FileAttributes.Archive);
                SystemCheckBox.IsChecked = fileInfo.Attributes.HasFlag(FileAttributes.System);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки информации о файле: {ex.Message}");
                ClearFileInfo();
            }
        }

        private void ClearFileInfo()
        {
            FileInfoTextBlock.Text = "";
            ReadOnlyCheckBox.IsChecked = false;
            HiddenCheckBox.IsChecked = false;
            ArchiveCheckBox.IsChecked = false;
            SystemCheckBox.IsChecked = false;
        }

        private void ApplyAttributesButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListBox.SelectedItem is string selectedFile)
            {
                string fullPath = System.IO.Path.Combine(currentDirectory, selectedFile);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(fullPath);

                        FileAttributes attributes = fileInfo.Attributes;

                        // Управление атрибутом "Только для чтения"
                        if (ReadOnlyCheckBox.IsChecked == true)
                            attributes |= FileAttributes.ReadOnly;
                        else
                            attributes &= ~FileAttributes.ReadOnly;

                        // Атрибут "Скрытый"
                        if (HiddenCheckBox.IsChecked == true)
                            attributes |= FileAttributes.Hidden;
                        else
                            attributes &= ~FileAttributes.Hidden;

                        // Атрибут "Архивный"
                        if (ArchiveCheckBox.IsChecked == true)
                            attributes |= FileAttributes.Archive;
                        else
                            attributes &= ~FileAttributes.Archive;

                        // Атрибут "Системный"
                        if (SystemCheckBox.IsChecked == true)
                            attributes |= FileAttributes.System;
                        else
                            attributes &= ~FileAttributes.System;

                        File.SetAttributes(fullPath, attributes);

                        DisplayFileInfo(fullPath);
                        System.Windows.MessageBox.Show("Атрибуты успешно применены.");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка применения атрибутов: {ex.Message}");
                    }
                }
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = currentDirectory;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && Directory.Exists(dlg.SelectedPath))
                {
                    currentDirectory = dlg.SelectedPath;
                    CurrentDirectoryTextBox.Text = currentDirectory;
                    PopulateDirectoryTree(currentDirectory);
                    PopulateFilesList(currentDirectory);
                    ClearFileInfo();
                }
            }
        }
    }
}
