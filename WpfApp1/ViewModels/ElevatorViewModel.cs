using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Linq;
using WpfApp1.Commands;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.ViewModels
{
    public class ElevatorViewModel : BaseViewModel
    {
        private readonly AnimationService _animationService;

        public ElevatorViewModel(FrameworkElement element)
        {
            _animationService = new AnimationService(element);

            CallElevatorCommand = new LambdaCommand(CallElevator);
            OpenElevatorCommand = new LambdaCommand(OpenElevator);
            CloseElevatorCommand = new LambdaCommand(CloseElevator);
            MoveElevatorCommand = new LambdaCommand(MoveElevator);
            CallDispatcherCommand = new LambdaCommand(CallDispatcher);

            EnterElevatorCommand = new LambdaCommand(EnterElevator);
            LeaveElevatorCommand = new LambdaCommand(LeaveElevator);
        }

        public ObservableCollection<int> Floors { get; } = new ObservableCollection<int>(Enumerable.Range(1, 9));

        private int _selectedFloor = 1;
        public int SelectedFloor
        {
            get => _selectedFloor;
            set => Set(ref _selectedFloor, value);
        }

        private int _currentFloor = 1;
        public int CurrentFloor
        {
            get => _currentFloor;
            set
            {
                if (_currentFloor == value) return;
                AnimateFloorChange(_currentFloor, value);
                Set(ref _currentFloor, value);
            }
        }

        private bool _isOpenButtonEnabled = false;
        public bool IsOpenButtonEnabled
        {
            get => _isOpenButtonEnabled;
            set => Set(ref _isOpenButtonEnabled, value);    
        }

        private string _isHumanInElevator = "Нет";
        public string IsHumanInElevator
        {
            get => _isHumanInElevator;
            set 
            {
                Set(ref _isHumanInElevator, value);
                if (value == "Нет")
                {
                    IsButtonsEnabled = false;
                    IsOpenButtonEnabled = false;
                    IsCloseButtonEnabled = false;
                    IsCallDispatcherButtonEnabled = false;
                    IsCallButtonEnabled = true;
                    SelectedFloor = CurrentFloor;
                }
                else
                {
                    IsButtonsEnabled = true;
                    IsOpenButtonEnabled = true;
                    IsCloseButtonEnabled = true;
                    IsCallDispatcherButtonEnabled = true;
                    IsCallButtonEnabled = false;
                }
            }
        }

        #region Команды

        #region Команды человека

        public ICommand EnterElevatorCommand { get; }
        public ICommand LeaveElevatorCommand { get; }

        private void EnterElevator(object parameter)
        {
            IsHumanInElevator = "Да";
        }

        private void LeaveElevator(object parameter)
        {
            IsHumanInElevator = "Нет";
        }

        #endregion

        public ICommand CallElevatorCommand { get; }
        public ICommand OpenElevatorCommand { get; }
        public ICommand CloseElevatorCommand { get; }
        public ICommand MoveElevatorCommand { get; }
        public ICommand CallDispatcherCommand { get; }

        private void CallDispatcher(object parameter)
        {
            MessageBox.Show("Ожидайте...");
        }

        private void CallElevator(object parameter)
        {
            if (CurrentFloor == SelectedFloor)
            {
                _animationService.StartOpenDoorAnimation();
                return;
            }
            CurrentFloor = SelectedFloor;
        }

        private void OpenElevator(object parameter)
        {
            _animationService.StartOpenDoorAnimation();
        }

        private void CloseElevator(object parameter)
        {
            _animationService.StartCloseDoorAnimation();
        }

        private void MoveElevator(object parameter)
        {
            if (CurrentFloor == Convert.ToInt32(parameter) && IsHumanInElevator == "Да") return;
            if (CurrentFloor == Convert.ToInt32(parameter) && IsHumanInElevator == "Нет")
            {
                _animationService.StartCloseDoorAnimation();
                IsHumanButtonsEnabled = true;
                return;
            }
            if (IsHumanInElevator == "Нет")
            {
                return;
            }
            _animationService.StartOpenDoorAnimation();
            CurrentFloor = Convert.ToInt32(parameter);
            _animationService.StartCloseDoorAnimation();
        }

        #endregion


        private DispatcherTimer timer;
        private bool isAnimationStarted = false;

        private void AnimateFloorChange(int oldFloor, int newFloor)
        {
            IsHumanButtonsEnabled = false;
            IsOpenButtonEnabled = false;
            IsButtonsEnabled = false;
            IsCloseButtonEnabled = false;
            _animationService.StartCloseDoorAnimation();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2); // Минимальный интервал
            timer.Tick += (sender, e) => OnTimerTick(oldFloor, newFloor);
            timer.Start();
        }

        private async void OnTimerTick(int oldFloor, int newFloor)
        {
            if (!isAnimationStarted)
            {
                isAnimationStarted = true;

                var dispatcher = Application.Current.Dispatcher;
                var duration = TimeSpan.FromSeconds(Math.Abs(newFloor - oldFloor));
                var startTime = DateTime.Now;
                var currentTime = startTime;

                while (currentTime - startTime < duration)
                {
                    var progress = (currentTime - startTime).TotalMilliseconds / duration.TotalMilliseconds;
                    var currentOffset = oldFloor + (newFloor - oldFloor) * progress;

                    dispatcher.Invoke(() => FloorOffset = Convert.ToInt32(currentOffset));

                    await Task.Delay(16); // ~60 fps

                    currentTime = DateTime.Now;
                }

                // Ensure the final value is set
                dispatcher.Invoke(() => FloorOffset = newFloor);
                _animationService.StartOpenDoorAnimation();

                timer.Stop();
                isAnimationStarted = false;

                if (IsHumanInElevator == "Нет")
                {
                    IsHumanButtonsEnabled = true;
                    return;
                }
                IsOpenButtonEnabled = true;
                IsButtonsEnabled = true;
                IsCloseButtonEnabled = true;
                IsCallDispatcherButtonEnabled = true;
                IsHumanButtonsEnabled = true;
            }
        }


        private int _floorOffset = 1;
        public int FloorOffset
        {
            get => _floorOffset;
            set => Set(ref _floorOffset, value);
        }

        private bool _isButtonsEnabled = false;
        public bool IsButtonsEnabled
        {
            get => _isButtonsEnabled;
            set => Set(ref _isButtonsEnabled, value);
        }

        private bool _isCallButtonEnabled = true;
        public bool IsCallButtonEnabled
        {
            get => _isCallButtonEnabled;
            set => Set(ref _isCallButtonEnabled, value);
        }

        private bool _isCallDispatcherButtonEnabled = false;
        public bool IsCallDispatcherButtonEnabled
        {
            get => _isCallDispatcherButtonEnabled;
            set => Set(ref _isCallDispatcherButtonEnabled, value);
        }

        private bool _isCloseButtonEnabled = false;
        public bool IsCloseButtonEnabled
        {
            get => _isCloseButtonEnabled;
            set => Set(ref _isCloseButtonEnabled, value);
        }

        private bool _isHumanButtonsEnabled = false;
        public bool IsHumanButtonsEnabled
        {
            get => _isHumanButtonsEnabled;
            set => Set(ref _isHumanButtonsEnabled, value);
        }
    }
}
