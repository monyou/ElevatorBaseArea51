using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ElevatorBaseArea51
{
    class Person
    {
        public Rectangle ID { get; set; }
        public Thickness Margin { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Button OnFloor { get; set; }

        public Button ChoseFloor(List<Button> buttons,Button prevB)
        {
            Random rand = new Random();
            Button chosenB = buttons[rand.Next(0, buttons.Count)]; ;
            while (prevB == chosenB)
            {
                chosenB = buttons[rand.Next(0, buttons.Count)];
            }
            return chosenB;
        }
    }
}
