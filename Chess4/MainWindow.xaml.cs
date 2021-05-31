using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Globalization;

namespace Chess4
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        
    }
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
    public enum State
    {
        Empty,       // пусто
        WhiteBishop // слон
    }
    public class Cell : NotifyPropertyChanged
    {
        private State _state;
        private bool _active;

        public State State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(); // сообщить интерфейсу, что значение поменялось, чтобы интефейс перерисовался в этом месте
            }
        }
        public bool Active // это будет показывать, что ячейка выделена пользователем
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }
    }
    public class Board : IEnumerable<Cell>
    {
        private readonly Cell[,] _area;

        public State this[int row, int column]
        {
            get => _area[row, column].State;
            set => _area[row, column].State = value;
        }

        public Board()
        {
            _area = new Cell[8, 8];
            for (int i = 0; i < _area.GetLength(0); i++)
                for (int j = 0; j < _area.GetLength(1); j++)
                    _area[i, j] = new Cell();
        }

        public IEnumerator<Cell> GetEnumerator()
            => _area.Cast<Cell>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _area.GetEnumerator();
    }
    public class MainViewModel : NotifyPropertyChanged
    {
        private Board _board = new Board();
        private ICommand _newGameCommand;
        private ICommand _clearCommand;
        private ICommand _cellCommand;

        public IEnumerable<char> Numbers => "87654321";
        public IEnumerable<char> Letters => "ABCDEFGH";

        public Board Board
        {
            get => _board;
            set
            {
                _board = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewGameCommand => _newGameCommand = new RelayCommand(parameter =>
        {
            SetupBoard();
        });

        public ICommand ClearCommand => _clearCommand = new RelayCommand(parameter =>
        {
            Board = new Board();
        });

        public ICommand CellCommand => _cellCommand = new RelayCommand(parameter =>
        {
            Cell cell = (Cell)parameter;
            Cell activeCell = Board.FirstOrDefault(x => x.Active);
            if (cell.State != State.Empty)
            {
                if (!cell.Active && activeCell != null)
                    activeCell.Active = false;
                cell.Active = !cell.Active;
            }
            else if (activeCell != null)
            {
                activeCell.Active = false;
                cell.State = activeCell.State;
                activeCell.State = State.Empty;
            }
        }, parameter => parameter is Cell cell && (Board.Any(x => x.Active) || cell.State != State.Empty));

        private void SetupBoard()
        {
            Board board = new Board();
            board[7, 2] = State.WhiteBishop;
            board[7, 5] = State.WhiteBishop;
            Board = board;
        }

        public MainViewModel()
        {
        }
    }
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int v && (v % 2 == 0 ^ v / 8 % 2 == 0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}
