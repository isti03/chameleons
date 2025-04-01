using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChameleonGame.Avalonia.ViewModels
{
    public partial class Field : ViewModelBase
    {
        public int X { get; set; }
        public int Y { get; set; }

        public (int x, int y) coords { get => (X, Y); }

        private string _player;
        public string Player
        {
            get => _player;
            set
            {
                _player = value;
                OnPropertyChanged(nameof(HasGreenChameleon));
                OnPropertyChanged(nameof(HasRedChameleon));
                OnPropertyChanged(nameof(HasChameleon));
                OnPropertyChanged();
            }
        }

        public bool IsRedField { get; private set; }

        public bool IsGreenField { get; private set; }

        public bool HasRedChameleon { get => Player == "Red"; }

        public bool HasGreenChameleon { get => Player == "Green"; }

        public bool HasChameleon { get => (HasGreenChameleon || HasRedChameleon); }

        [ObservableProperty]
        private bool _isSelected;

        public Field(int X, int Y, string background, string chameleon)
        {
            this.X = X;
            this.Y = Y;
            IsRedField = background == "Red";
            IsGreenField = background == "Green";

            _player = chameleon;

            OnPropertyChanged();
        }
        public RelayCommand? FieldClickCommand { get; set; }
    }
}
