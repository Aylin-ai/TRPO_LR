using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.ViewModels;

namespace WpfApp1.Models
{
    public class Elevator : BaseViewModel
    {
        private int _currentFloor;
        public int CurrentFloor
        {
            get => _currentFloor;
            set => Set(ref _currentFloor, value);
        }
    }
}
