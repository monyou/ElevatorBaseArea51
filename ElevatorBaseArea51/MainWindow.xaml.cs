using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ElevatorBaseArea51
{

    public partial class MainWindow : Window
    {
        #region GlobalFields
        List<Button> floorButtons = new List<Button>();
        AutoResetEvent startCycle = new AutoResetEvent(false);
        AutoResetEvent moveElevator = new AutoResetEvent(false);
        AutoResetEvent moveConfidention = new AutoResetEvent(false);
        AutoResetEvent moveSecret = new AutoResetEvent(false);
        AutoResetEvent moveTopSecret = new AutoResetEvent(false);
        
        object elevatorLock = new object();
        object confidentionLock = new object();
        object secretLock = new object();
        object topsecretLock = new object();
        Button chosenFloor;
        Rectangle chosenPerson = new Rectangle();
        volatile bool onFloor;
        volatile bool isDoorOpened;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            floorButtons.AddRange(new List<Button> { btnPp,btnPe,btnGe,btnGf,btnSe,btnSf,btnT1e,btnT1f,btnT2e,btnT2f });

            Task elevatorT = Task.Factory.StartNew(() => Elevator(),TaskCreationOptions.LongRunning);
            Task confidentionT = Task.Factory.StartNew(() => Confidention(), TaskCreationOptions.LongRunning);
            //Task secretT = Task.Factory.StartNew(() => Secret(), TaskCreationOptions.LongRunning);
            //Task topsecretT = Task.Factory.StartNew(() => TopSecret(), TaskCreationOptions.LongRunning);

            openElevatorDoors();
            startCycle.Set();
        }

        void Elevator()
        {
            while (true)
            {
                moveElevator.WaitOne();
                Action Logic = () =>
                {
                    lock (elevatorLock)
                    {
                        if (!onFloor)
                        {
                            LockingButtonsExcept(chosenFloor.Name);
                            closeElevatorDoors();
                            if (chosenFloor.Name == "btnPp" || chosenFloor.Name == "btnPe") gotoFloor(street);
                            if (chosenFloor.Name == "btnGf" || chosenFloor.Name == "btnGe") gotoFloor(firstFloor);
                            if (chosenFloor.Name == "btnSf" || chosenFloor.Name == "btnSe") gotoFloor(secondFloor);
                            if (chosenFloor.Name == "btnT1f" || chosenFloor.Name == "btnT1e") gotoFloor(thirdFloor);
                            if (chosenFloor.Name == "btnT2f" || chosenFloor.Name == "btnT2e") gotoFloor(fourthFloor);
                        }
                        else
                        {
                            if(elevatorPanel.Children.Contains(confidentionPerson) && (chosenFloor==btnSf || chosenFloor==btnT1f || chosenFloor == btnT2f || chosenFloor == btnSe || chosenFloor == btnT1e || chosenFloor == btnT2e))
                            {
                                lblResctricted.Visibility = Visibility.Visible;
                                moveConfidention.Set();
                            }
                            else if(elevatorPanel.Children.Contains(secretPerson) && (chosenFloor == btnT1f || chosenFloor == btnT2f || chosenFloor == btnT1e || chosenFloor == btnT2e))
                            {
                                lblResctricted.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                lblResctricted.Visibility = Visibility.Hidden;
                                openElevatorDoors();
                                moveConfidention.Set();
                            }
                        }
                    }
                };
                Dispatcher.Invoke(Logic);
            }
        }

        void Confidention()
        {
            while (true)
            {
                startCycle.WaitOne();
                chosenPerson = confidentionPerson;
                lock (confidentionLock)
                {
                    chosenFloor = btnGf;
                    onFloor = false;
                }
                moveElevator.Set();
                moveConfidention.WaitOne();
                Dispatcher.Invoke(() =>
                {
                    moveINelevator(chosenPerson);
                });
                isDoorOpened = false;
                while (!isDoorOpened)
                {
                    lock (confidentionLock)
                    {
                        pickRandomFloor();
                    }
                    moveElevator.Set();
                    moveConfidention.WaitOne();
                }
                Dispatcher.Invoke(() =>
                {
                    moveOUTelevator(chosenPerson);
                });
            }
        }

        void Secret()
        {
            while (true)
            {
                moveSecret.WaitOne();
                Action Logic = () =>
                {
                    lock(secretLock)
                    {
                        
                    }
                };
                Dispatcher.Invoke(Logic);
            }
        }

        void TopSecret()
        {
            while (true)
            {
                moveTopSecret.WaitOne();
                Action Logic = () =>
                {
                    lock (topsecretLock)
                    {
                        
                    }
                };
                Dispatcher.Invoke(Logic);
            }
        }

        //Need to implement this
        private void CheckingFloorT_Tick(object sender, EventArgs e)
        {
            if(elevator.Margin.Top == street.Margin.Top)
            {
                btnPe.IsEnabled = false;
                btnPp.IsEnabled = false;
            }
            else
            {
                btnPe.IsEnabled = true;
                btnPp.IsEnabled = true;
            }
            if (elevator.Margin.Top == firstFloor.Margin.Top)
            {
                firstF.Foreground = Brushes.LightGreen;
                btnGe.IsEnabled = false;
                btnGf.IsEnabled = false;
            }
            else
            {
                firstF.Foreground = Brushes.White;
                btnGe.IsEnabled = true;
                btnGf.IsEnabled = true;
            }

            if (elevator.Margin.Top == secondFloor.Margin.Top)
            {
                secondF.Foreground = Brushes.LightGreen;
                btnSe.IsEnabled = false;
                btnSf.IsEnabled = false;
            }
            else
            {
                secondF.Foreground = Brushes.White;
                btnSe.IsEnabled = true;
                btnSf.IsEnabled = true;
            }

            if (elevator.Margin.Top == thirdFloor.Margin.Top)
            {
                thirdF.Foreground = Brushes.LightGreen;
                btnT1e.IsEnabled = false;
                btnT1f.IsEnabled = false;
            }
            else
            {
                thirdF.Foreground = Brushes.White;
                btnT1e.IsEnabled = true;
                btnT1f.IsEnabled = true;
            }

            if (elevator.Margin.Top == fourthFloor.Margin.Top)
            {
                fourthF.Foreground = Brushes.LightGreen;
                btnT2e.IsEnabled = false;
                btnT2f.IsEnabled = false;
            }
            else
            {
                fourthF.Foreground = Brushes.White;
                btnT2e.IsEnabled = true;
                btnT2f.IsEnabled = true;
            }
        }

        void LockingButtonsExcept(string exptbtnName)
        {
            foreach (var item in floorButtons)
            {
                if (item.Name != exptbtnName) item.IsEnabled = false;
            }
        }

        void UnlockingButtons()
        {
            foreach (var item in floorButtons)
            {
                item.IsEnabled = true;
            }
        }

        void openElevatorDoors()
        {
            DoubleAnimation animationDL = new DoubleAnimation(3, new Duration(TimeSpan.FromMilliseconds(300)));
            ThicknessAnimation animationDR = new ThicknessAnimation(new Thickness(elevator.Width, elevatorDoorR.Margin.Top, 0, 0), new Duration(TimeSpan.FromMilliseconds(300)));
            elevatorDoorL.BeginAnimation(WidthProperty, animationDL);
            elevatorDoorR.BeginAnimation(MarginProperty, animationDR);
            //chosenPerson.Visibility = Visibility.Visible;
            isDoorOpened = true;
        }

        void closeElevatorDoors()
        {
            DoubleAnimation animationDL = new DoubleAnimation(65, new Duration(TimeSpan.FromMilliseconds(300)));
            ThicknessAnimation animationDR = new ThicknessAnimation(new Thickness(66, elevatorDoorR.Margin.Top, 0, 0), new Duration(TimeSpan.FromMilliseconds(300)));
            elevatorDoorL.BeginAnimation(WidthProperty, animationDL);
            elevatorDoorR.BeginAnimation(MarginProperty, animationDR);
            //chosenPerson.Visibility = Visibility.Hidden;
            isDoorOpened = false;
        }

        void gotoFloor(Rectangle floor)
        {
            ThicknessAnimation animationE = new ThicknessAnimation(new Thickness(elevator.Margin.Left, floor.Margin.Top, 0, 0), new TimeSpan(0, 0, 1));
            ThicknessAnimation animationEP = new ThicknessAnimation(new Thickness(elevatorPanel.Margin.Left, floor.Margin.Top, 0, 0), new TimeSpan(0, 0, 1));
            DoubleAnimation animationER = new DoubleAnimation(floor.Margin.Top - 20, new TimeSpan(0, 0, 1));
            elevatorRope.BeginAnimation(HeightProperty, animationER);
            animationEP.Completed += (sender, e) =>
            {
                onFloor = true;
                moveElevator.Set();
                UnlockingButtons();
            };
            elevator.BeginAnimation(MarginProperty, animationE);
            elevatorPanel.BeginAnimation(MarginProperty, animationEP);
        }

        void moveINelevator(Rectangle person)
        {
            switch (person.Name)
            {
                case "confidentionPerson":
                    lock (confidentionLock)
                    {
                        Frame.Children.Remove(confidentionPerson);
                        elevatorPanel.Children.Add(confidentionPerson);
                        confidentionPerson.Margin = new Thickness(30, 30, 0, 0);
                    }
                    break;
                case "secretPerson":
                    lock (secretLock)
                    {
                        Frame.Children.Remove(secretPerson);
                        elevatorPanel.Children.Add(secretPerson);
                        secretPerson.Margin = new Thickness(30, 30, 0, 0);
                    }
                    break;
                case "topsecretPerson":
                    lock (topsecretLock)
                    {
                        Frame.Children.Remove(topsecretPerson);
                        elevatorPanel.Children.Add(topsecretPerson);
                        topsecretPerson.Margin = new Thickness(30, 30, 0, 0);
                    }
                    break;
            }
        }

        void moveOUTelevator(Rectangle person)
        {
            if (chosenFloor == btnPe || chosenFloor == btnPp)
                movePersonLeftRight(-150, -74, person);
            else
                movePersonLeftRight(150, 74, person);
        }

        private void btnClick(object sender, RoutedEventArgs e)
        {
            Button chosenbtn = (Button)sender;
            chosenFloor = chosenbtn;
            onFloor = false;
            moveElevator.Set();
        }

        private void confidentionPerson_MouseDown(object sender, MouseButtonEventArgs e)
        {
            moveConfidention.Set();
        }

        void movePersonLeftRight(int dir, int dir2,Rectangle person)
        {
            switch (person.Name)
            {
                case "confidentionPerson":
                    lock (confidentionLock)
                    {
                        elevatorPanel.Children.Remove(confidentionPerson);
                        Frame.Children.Add(confidentionPerson);
                        if (confidentionPerson.Margin.Left == secretPerson.Margin.Left || confidentionPerson.Margin.Left == topsecretPerson.Margin.Left)
                        {
                            confidentionPerson.Margin = new Thickness(elevator.Margin.Left + (dir + dir2), elevator.Margin.Top + 30, 0, 0);
                            if (confidentionPerson.Margin.Left == secretPerson.Margin.Left || confidentionPerson.Margin.Left == topsecretPerson.Margin.Left)
                            {
                                confidentionPerson.Margin = new Thickness(confidentionPerson.Margin.Left + 74, confidentionPerson.Margin.Top, 0, 0);
                            }
                        }
                        else
                            confidentionPerson.Margin = new Thickness(elevator.Margin.Left + dir, elevator.Margin.Top + 30, 0, 0);
                    }
                    break;
                case "secretPerson":
                    lock (secretLock)
                    {
                        elevatorPanel.Children.Remove(secretPerson);
                        Frame.Children.Add(secretPerson);
                        if (secretPerson.Margin.Left == confidentionPerson.Margin.Left || secretPerson.Margin.Left == topsecretPerson.Margin.Left)
                        {
                            secretPerson.Margin = new Thickness(elevator.Margin.Left +(dir + dir2), elevator.Margin.Top + 30, 0, 0);
                            if (secretPerson.Margin.Left == confidentionPerson.Margin.Left || secretPerson.Margin.Left == topsecretPerson.Margin.Left)
                            {
                                secretPerson.Margin = new Thickness(secretPerson.Margin.Left + 74, secretPerson.Margin.Top, 0, 0);
                            }
                        }
                        else
                            secretPerson.Margin = new Thickness(elevator.Margin.Left + dir, elevator.Margin.Top + 30, 0, 0);
                    }
                    break;
                case "topsecretPerson":
                    lock (topsecretLock)
                    {
                        elevatorPanel.Children.Remove(topsecretPerson);
                        Frame.Children.Add(topsecretPerson);
                        if (topsecretPerson.Margin.Left == secretPerson.Margin.Left || topsecretPerson.Margin.Left == confidentionPerson.Margin.Left)
                        {
                            topsecretPerson.Margin = new Thickness(elevator.Margin.Left + (dir + dir2), elevator.Margin.Top + 30, 0, 0);
                            if (topsecretPerson.Margin.Left == secretPerson.Margin.Left || topsecretPerson.Margin.Left == confidentionPerson.Margin.Left)
                            {
                                topsecretPerson.Margin = new Thickness(topsecretPerson.Margin.Left + 74, topsecretPerson.Margin.Top, 0, 0);
                            }
                        }
                        else
                            topsecretPerson.Margin = new Thickness(elevator.Margin.Left + dir, elevator.Margin.Top + 30, 0, 0);
                    }
                    break;
            }
        }

        void pickRandomFloor()
        {
            Random rand = new Random();
            if (chosenFloor == floorButtons[rand.Next(0, floorButtons.Count)]) return;
            chosenFloor = floorButtons[rand.Next(0, floorButtons.Count)];
            onFloor = false;
        }
    }
}