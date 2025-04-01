using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using ChameleonGame.Avalonia.ViewModels;
using ChameleonGame.Avalonia.Views;
using ChameleonGame.Model;
using ChameleonGame.Persistence;
using Avalonia.Platform.Storage;
using Avalonia.Platform;
using System.IO;

namespace ChameleonGame.Avalonia;

public partial class App : Application
{
    private IPersistence persistence = null!;
    private ChameleonModel model = null!;
    private MainViewModel viewModel = null!;

    private TopLevel? TopLevel
    {
        get => ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => TopLevel.GetTopLevel(desktop.MainWindow),
            ISingleViewApplicationLifetime singleViewPlatform => TopLevel.GetTopLevel(singleViewPlatform.MainView),
            _ => null
        };
    }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        persistence = new BinaryFilePersistence();
        model = new ChameleonModel(persistence);
        viewModel = new MainViewModel(model);

        viewModel.LoadGame += ViewModel_LoadGame;
        viewModel.SaveGame += ViewModel_SaveGame;
        viewModel.GameOver += ViewModel_GameOver;
        viewModel.IllegalStep += ViewModel_IllegalStep;

        model.NewGame(5);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };

            if (Application.Current?.TryGetFeature<IActivatableLifetime>() is { } activatableLifetime)
            {
                activatableLifetime.Activated += async (sender, args) =>
                {
                    if (args.Kind == ActivationKind.Background)
                    {
                        // load the suspended game if exists
                        try
                        {
                            await model.LoadGame(
                                Path.Combine(AppContext.BaseDirectory, "SuspendedGame"));
                        }
                        catch
                        {
                        }
                    }
                };
                activatableLifetime.Deactivated += async (sender, args) =>
                {
                    if (args.Kind == ActivationKind.Background)
                    {

                        // save the game to a suspended state
                        try
                        {
                            await model.SaveGame(Path.Combine(AppContext.BaseDirectory, "SuspendedGame"));
                        }
                        catch
                        {
                        }
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void ViewModel_IllegalStep(object? sender, System.EventArgs e)
    {
        await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    "Illegal step!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
    }

    private async void ViewModel_GameOver(object? sender, GameOverEventArgs e)
    {
        await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    $"{e.winner.ToString()} wins!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
    }

    private async void ViewModel_SaveGame(object? sender, System.EventArgs e)
    {
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    "File management is not supported!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try
        {
            var file = await TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save ChameleonGame board",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("ChameleonGame Savefile")
                    {
                        Patterns = new[] { "*.cgs" }
                    }
                }
            });

            if (file != null)
            {
                using (var stream = await file.OpenWriteAsync())
                {
                    await model.SaveGame(stream);
                }
            }
        }
        catch (Exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    "Failed to save board!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }

    private async void ViewModel_LoadGame(object? sender, System.EventArgs e)
    {
        if (TopLevel == null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    "File management is not supported!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
            return;
        }

        try
        {
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load ChameleonGame savefile",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("ChameleonGame Savefile")
                    {
                        Patterns = new[] { "*.cgs" }
                    }
                }
            });

            if (files.Count > 0)
            {
                using (var stream = await files[0].OpenReadAsync())
                {
                    await model.LoadGame(stream);
                }
            }
        }
        catch (Exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                    "ChameleonGame",
                    "Failed to load savefile!",
                    ButtonEnum.Ok, Icon.Error)
                .ShowAsync();
        }
    }
}
