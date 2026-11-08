using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace MyAvaloniaApp.Views;

public partial class MainWindow : Window
{
    private readonly List<string> _imageFiles = new List<string>();
    private int _currentIndex = -1;
    
    // Масив підтримуваних графічних форматів
    private readonly string[] _supportedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

    public MainWindow()
    {
        InitializeComponent();

        // Програмне підключення подій (компілятор автоматично бачить змінні з XAML)
        if (LoadButton != null) LoadButton.Click += OnLoadClick;
        if (CloseButton != null) CloseButton.Click += OnCloseClick;
        if (PrevButton != null) PrevButton.Click += OnPrevClick;
        if (NextButton != null) NextButton.Click += OnNextClick;
    }

    private void OnLoadClick(object? sender, RoutedEventArgs e)
    {
        string? input = PathsTextBox?.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            if (InfoTextBlock != null) InfoTextBlock.Text = "Помилка: Рядок із шляхами порожній!";
            return;
        }

        // ВИПРАВЛЕНО: Коректне передавання символу '@' у масиві чарів
        string[] paths = input.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
        _imageFiles.Clear();
        List<string> errors = new List<string>();

        foreach (var path in paths)
        {
            string cleanPath = path.Trim();
            if (Directory.Exists(cleanPath))
            {
                try
                {
                    var files = Directory.GetFiles(cleanPath)
                        .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                        .ToList();
                    _imageFiles.AddRange(files);
                }
                catch (Exception ex)
                {
                    errors.Add($"Не вдалося прочитати {cleanPath}: {ex.Message}");
                }
            }
            else
            {
                errors.Add($"Каталог не існує: {cleanPath}");
            }
        }

        if (_imageFiles.Count > 0)
        {
            _currentIndex = 0;
            DisplayCurrentImage();
        }
        else
        {
            _currentIndex = -1;
            if (ImageDisplay != null) ImageDisplay.Source = null;
            string errorSummary = errors.Count > 0 ? "\n" + string.Join("\n", errors) : "";
            if (InfoTextBlock != null) InfoTextBlock.Text = $"Зображень не знайдено в указаних каталогах.{errorSummary}";
        }
    }

    private void DisplayCurrentImage()
    {
        if (_currentIndex < 0 || _currentIndex >= _imageFiles.Count)
        {
            if (ImageDisplay != null) ImageDisplay.Source = null;
            if (InfoTextBlock != null) InfoTextBlock.Text = "Немає доступних зображень для відображення.";
            return;
        }

        string filePath = _imageFiles[_currentIndex];

        try
        {
            using (var stream = File.OpenRead(filePath))
            {
                var bitmap = new Bitmap(stream);
                if (ImageDisplay != null) ImageDisplay.Source = bitmap;

                FileInfo fileInfo = new FileInfo(filePath);
                double fileSizeKb = fileInfo.Length / 1024.0;
                string sizeText = fileSizeKb > 1024 
                    ? $"{Math.Round(fileSizeKb / 1024.0, 2)} МБ" 
                    : $"{Math.Round(fileSizeKb, 2)} КБ";

                if (InfoTextBlock != null)
                {
                    InfoTextBlock.Text = $"[Зображення {_currentIndex + 1} із {_imageFiles.Count}]\n" +
                                         $"Ім'я файлу: {fileInfo.Name}\n" +
                                         $"Шлях: {fileInfo.FullName}\n" +
                                         $"Дата створення: {fileInfo.CreationTime:dd.MM.yyyy HH:mm:ss}\n" +
                                         $"Розмір файлу: {sizeText}\n" +
                                         $"Роздільна здатність: {bitmap.PixelSize.Width} x {bitmap.PixelSize.Height} px\n" +
                                         $"Формат: {fileInfo.Extension.ToUpper().Replace(".", "")}";
                }
            }
        }
        catch (Exception ex)
        {
            if (InfoTextBlock != null) InfoTextBlock.Text = $"Помилка завантаження файлу {Path.GetFileName(filePath)}: {ex.Message}";
            if (ImageDisplay != null) ImageDisplay.Source = null;
        }
    }

    private void OnPrevClick(object? sender, RoutedEventArgs e)
    {
        if (_imageFiles.Count == 0) return;

        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = _imageFiles.Count - 1;
        }
        DisplayCurrentImage();
    }

    private void OnNextClick(object? sender, RoutedEventArgs e)
    {
        if (_imageFiles.Count == 0) return;

        _currentIndex++;
        if (_currentIndex >= _imageFiles.Count)
        {
            _currentIndex = 0;
        }
        DisplayCurrentImage();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
