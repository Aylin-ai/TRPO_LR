using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace WpfApp1.Services
{
    public class AnimationService
    {
        private readonly FrameworkElement _element;

        public AnimationService(FrameworkElement element)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public void StartOpenDoorAnimation()
        {
            var openDoorStoryboard = (Storyboard)_element.FindResource("OpenDoorAnimation");
            if (openDoorStoryboard != null)
            {
                openDoorStoryboard.Begin();
            }
        }

        public void StartCloseDoorAnimation()
        {
            var closeDoorStoryboard = (Storyboard)_element.FindResource("CloseDoorAnimation");
            if (closeDoorStoryboard != null)
            {
                closeDoorStoryboard.Begin();
            }
        }
    }

}
