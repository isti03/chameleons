using ChameleonGame.Model;
using System.Collections.ObjectModel;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace ChameleonGame.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ChameleonModel model;

    private Field? _selectedField;

    // Only allows a single field to have isSelected = true
    private Field? selectedField
    {
        get => _selectedField;
        set
        {
            if (_selectedField != null)
                _selectedField.IsSelected = false;
            if (value != null)
                value.IsSelected = (value != null);
            _selectedField = value;
        }
    }

    [ObservableProperty]
    private int _boardSize;

    [ObservableProperty]
    private string _currentPlayer;

    public RelayCommand<int> NewGameCommand { get; private set; }
    public RelayCommand LoadGameCommand { get; private set; }
    public RelayCommand SaveGameCommand { get; private set; }
    public RelayCommand ExitCommand { get; private set; }

    private int newGameSize = 5;
    public int NewGameSize
    {
        get => newGameSize;
        set
        {
            if (newGameSize == value)
                return;
            
            newGameSize = value;
            OnPropertyChanged(nameof(NewGameSize));
            OnPropertyChanged(nameof(NewGameEasy));
            OnPropertyChanged(nameof(NewGameMedium));
            OnPropertyChanged(nameof(NewGameHard));
        }
    }

    public bool NewGameEasy
    {
        get => NewGameSize == 3;
        set => NewGameSize = 3;
    }
    public bool NewGameMedium
    {
        get => NewGameSize == 5;
        set => NewGameSize = 5;
    }

    public bool NewGameHard
    {
        get => NewGameSize == 7;
        set => NewGameSize = 7;
    }

    public ObservableCollection<Field> Fields { get; set; }

    public event EventHandler? LoadGame;
    public event EventHandler? SaveGame;
    public event EventHandler? ExitGame;
    public event EventHandler<GameOverEventArgs>? GameOver;
    public event EventHandler? IllegalStep;

    public MainViewModel(ChameleonModel model)
    {
        this.model = model;

        model.GameLoaded += Model_GameLoaded;
        model.FieldChanged += Model_FieldChanged;
        model.CurrentPlayerChanged += Model_CurrentPlayerChanged;
        model.GameOver += Model_GameOver;

        Fields = new ObservableCollection<Field>();
        BoardSize = model.boardSize;
        CurrentPlayer = model.currentPlayer.ToString();

        NewGameCommand = new RelayCommand<int>(param => model.NewGame(param));
        LoadGameCommand = new RelayCommand((() => LoadGame?.Invoke(this, EventArgs.Empty)));
        SaveGameCommand = new RelayCommand(() => SaveGame?.Invoke(this, EventArgs.Empty));
        ExitCommand = new RelayCommand(() => ExitGame?.Invoke(this, EventArgs.Empty));
    }

    private void Model_CurrentPlayerChanged(object? sender, EventArgs e)
    {
        CurrentPlayer = model.currentPlayer.ToString();
    }

    private void Model_FieldChanged(object? sender, FieldChangedEventArgs e)
    {
        Fields.First(field => field.X == e.X && field.Y == e.Y).Player = e.ChameleonColor.ToString();
    }

    private void Model_GameLoaded(object? sender, EventArgs e)
    {
        Fields.Clear();
        for (int i = 0; i < model.boardSize; i++)
        {
            for (int j = 0; j < model.boardSize; j++)
            {
                Field field = new Field(i, j, model.GetField(i, j)!.color.ToString(), model.GetField(i, j)!.chameleon.ToString());
                field.FieldClickCommand = new RelayCommand(() => OnFieldClick(field));
                Fields.Add(field);
            }
        }

        BoardSize = model.boardSize;
        CurrentPlayer = model.currentPlayer.ToString();
    }

    private void Model_GameOver(object? sender, GameOverEventArgs e)
    {
        GameOver?.Invoke(this, e);
    }

    private void OnFieldClick(object? param)
    {
        Field field = (param as Field)!;
        if (field.Player == CurrentPlayer)
        {
            // Modify selection or de-select selected field
            selectedField = field.IsSelected ? null : field;
        }
        else if (selectedField is null)
        {
            return;
        }
        else
        {
            try
            {
                model.Step(selectedField.coords, field.coords);
                selectedField = null;
            }
            catch
            {
                IllegalStep?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
