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
            moveConfidention.Set();
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
                            if(elevatorPanel.Children.Contains(confidentionPerson) && (chosenFloor==btnSf || chosenFloor==btnT1f || chosenFloor == btnT2f))
                            {
                                lblResctricted.Visibility = Visibility.Visible;
                            }
                            else if(elevatorPanel.Children.Contains(secretPerson) && (chosenFloor == btnT1f || chosenFloor == btnT2f))
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
                moveConfidention.WaitOne();
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
                    lock (confidentionPerson)
                    {
                        Frame.Children.Remove(confidentionPerson);
                        elevatorPanel.Children.Add(confidentionPerson);
                        confidentionPerson.Margin = new Thickness(30, 30, 0, 0);
                        chosenFloor = btnT1f;
                        onFloor = false;
                    }
                });
                moveElevator.Set();
                //Dispatcher.Invoke(() => {
                //    lock (confidentionLock)
                //    {
                //        chosenFloor = btnGf;
                //        onFloor = false;
                //    }
                //});
                //moveElevator.Set();
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
            chosenPerson.Visibility = Visibility.Visible;
        }

        void closeElevatorDoors()
        {
            DoubleAnimation animationDL = new DoubleAnimation(65, new Duration(TimeSpan.FromMilliseconds(300)));
            ThicknessAnimation animationDR = new ThicknessAnimation(new Thickness(66, elevatorDoorR.Margin.Top, 0, 0), new Duration(TimeSpan.FromMilliseconds(300)));
            elevatorDoorL.BeginAnimation(WidthProperty, animationDL);
            elevatorDoorR.BeginAnimation(MarginProperty, animationDR);
            chosenPerson.Visibility = Visibility.Hidden;
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

        void moveIN(Rectangle person)
        {
            ThicknessAnimation animation = new ThicknessAnimation(new Thickness(elevator.Margin.Left + 30, elevator.Margin.Top + 30, 0, 0), new TimeSpan(0, 0, 1));
            animation.Completed += (sender, e) =>
            {
            };
            person.BeginAnimation(MarginProperty, animation);
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
    }
}