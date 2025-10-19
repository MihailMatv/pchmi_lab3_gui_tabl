using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace FileManagerWPF
{
    public partial class MainWindow : Window
    {
        private string currentDirectory;
        private ObservableCollection<FileItem> fileItems = new ObservableCollection<FileItem>();

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
                var rootDirectoryInfo = new DirectoryInfo(path);
                var rootNode = CreateDirectoryNode(rootDirectoryInfo);
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
            var node = new TreeViewItem() { Header = directoryInfo.Name, Tag = directoryInfo.FullName };
            try
            {
                foreach (var dir in directoryInfo.GetDirectories())
                    node.Items.Add(CreateDirectoryNode(dir));
            }
            catch { }
            return node;
        }

        private void PopulateFilesList(string path)
        {
            fileItems.Clear();
            try
            {
                var dirInfo = new DirectoryInfo(path);
                foreach (var file in dirInfo.GetFiles())
                {
                    var attr = file.Attributes;
                    fileItems.Add(new FileItem
                    {
                        FileName = file.Name,
                        IsReadOnly = attr.HasFlag(FileAttributes.ReadOnly),
                        IsHidden = attr.HasFlag(FileAttributes.Hidden),
                        IsArchive = attr.HasFlag(FileAttributes.Archive),
                        IsSystem = attr.HasFlag(FileAttributes.System)
                    });
                }
                FilesDataGrid.ItemsSource = fileItems;
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
                    PopulateDirectoryTree(currentDirectory);
                    PopulateFilesList(currentDirectory);
                }
            }
        }

        private void ApplyAttributesButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesDataGrid.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("Пожалуйста, выберите хотя бы один файл.");
                return;
            }

            foreach (FileItem item in FilesDataGrid.SelectedItems)
            {
                string fullPath = Path.Combine(currentDirectory, item.FileName);
                if (!File.Exists(fullPath)) continue;

                try
                {
                    var attr = File.GetAttributes(fullPath);

                    attr = UpdateAttribute(attr, FileAttributes.ReadOnly, item.IsReadOnly);
                    attr = UpdateAttribute(attr, FileAttributes.Hidden, item.IsHidden);
                    attr = UpdateAttribute(attr, FileAttributes.Archive, item.IsArchive);
                    attr = UpdateAttribute(attr, FileAttributes.System, item.IsSystem);

                    File.SetAttributes(fullPath, attr);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка применения атрибутов к файлу {item.FileName}: {ex.Message}");
                }
            }
            System.Windows.MessageBox.Show("Атрибуты успешно применены ко всем выбранным файлам.");
            PopulateFilesList(currentDirectory); // Обновить статус файлов после применения
        }

        private FileAttributes UpdateAttribute(FileAttributes original, FileAttributes flag, bool isChecked)
        {
            if (isChecked)
                return original | flag;
            else
                return original & ~flag;
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = currentDirectory;
                var result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && Directory.Exists(dlg.SelectedPath))
                {
                    currentDirectory = dlg.SelectedPath;
                    CurrentDirectoryTextBox.Text = currentDirectory;
                    PopulateDirectoryTree(currentDirectory);
                    PopulateFilesList(currentDirectory);
                }
            }
        }
    }

    public class FileItem
    {
        public string FileName { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsArchive { get; set; }
        public bool IsSystem { get; set; }
    }
}
