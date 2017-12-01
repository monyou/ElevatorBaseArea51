using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ElevatorBaseArea51
{
    public class Person
    {
        public Rectangle Id { get; set; }
        public Button OnFloor { get; set; }

        public Button ChoseFloor(List<Button> buttons,Button prevB)
        {
            Random rand = new Random();
            Button chosenB = buttons[rand.Next(0, buttons.Count)];

            while (prevB.Equals(chosenB))
            {
                chosenB = buttons[rand.Next(0, buttons.Count)];
            }
            return chosenB;
        }
    }
}
