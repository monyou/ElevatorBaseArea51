using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ElevatorBaseArea51
{

    public partial class MainWindow : Window
    {
        #region GlobalFields
        List<Button> floorButtons = new List<Button>();

        AutoResetEvent startCycleC = new AutoResetEvent(false);
        AutoResetEvent startCycleS = new AutoResetEvent(false);
        AutoResetEvent startCycleTS = new AutoResetEvent(false);
        AutoResetEvent moveElevator = new AutoResetEvent(false);
        AutoResetEvent continuePerson = new AutoResetEvent(false);
        
        object elevatorLock = new object();
        object confidentionLock = new object();
        object secretLock = new object();
        object topsecretLock = new object();
        Button chosenFloor;
        Button cPersonFloor;
        Button sPersonFloor;
        Button tsPersonFloor;
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
            Task secretT = Task.Factory.StartNew(() => Secret(), TaskCreationOptions.LongRunning);
            Task topsecretT = Task.Factory.StartNew(() => TopSecret(), TaskCreationOptions.LongRunning);

            openElevatorDoors();
            cPersonFloor = btnGf;
            sPersonFloor = btnSf;
            tsPersonFloor = btnPp;
            startCycleC.Set();
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
                                lblResctricted.Margin = new Thickness(elevator.Margin.Left + 200, elevator.Margin.Top + 50, 0, 0);
                                lblResctricted.Visibility = Visibility.Visible;
                                continuePerson.Set();
                            }
                            else if(elevatorPanel.Children.Contains(secretPerson) && (chosenFloor == btnT1f || chosenFloor == btnT2f || chosenFloor == btnT1e || chosenFloor == btnT2e))
                            {
                                lblResctricted.Margin = new Thickness(elevator.Margin.Left + 200, elevator.Margin.Top + 50, 0, 0);
                                lblResctricted.Visibility = Visibility.Visible;
                                continuePerson.Set();
                            }
                            else
                            {
                                lblResctricted.Visibility = Visibility.Hidden;
                                openElevatorDoors();
                                continuePerson.Set();
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
                startCycleC.WaitOne();
                chosenPerson = confidentionPerson;
                lock (confidentionLock)
                {
                    chosenFloor = cPersonFloor;
                    onFloor = false;
                }
                moveElevator.Set();
                continuePerson.WaitOne();
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
                    continuePerson.WaitOne();
                }
                Dispatcher.Invoke(() =>
                {
                    moveOUTelevator(chosenPerson);
                });
                startCycleS.Set();
            }
        }

        void Secret()
        {
            while (true)
            {
                startCycleS.WaitOne();
                chosenPerson = secretPerson;
                lock (secretPerson)
                {
                    chosenFloor = sPersonFloor;
                    onFloor = false;
                }
                moveElevator.Set();
                continuePerson.WaitOne();
                Dispatcher.Invoke(() =>
                {
                    moveINelevator(chosenPerson);
                });
                isDoorOpened = false;
                while (!isDoorOpened)
                {
                    lock (secretLock)
                    {
                        pickRandomFloor();
                    }
                    moveElevator.Set();
                    continuePerson.WaitOne();
                }
                Dispatcher.Invoke(() =>
                {
                    moveOUTelevator(chosenPerson);
                });
                startCycleTS.Set();
            }
        }

        void TopSecret()
        {
            while (true)
            {
                startCycleTS.WaitOne();
                chosenPerson = topsecretPerson;
                lock (topsecretLock)
                {
                    chosenFloor = tsPersonFloor;
                    onFloor = false;
                }
                moveElevator.Set();
                continuePerson.WaitOne();
                Dispatcher.Invoke(() =>
                {
                    moveINelevator(chosenPerson);
                });
                isDoorOpened = false;
                while (!isDoorOpened)
                {
                    lock (topsecretLock)
                    {
                        pickRandomFloor();
                    }
                    moveElevator.Set();
                    continuePerson.WaitOne();
                }
                Dispatcher.Invoke(() =>
                {
                    moveOUTelevator(chosenPerson);
                });
                startCycleC.Set();
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
            animationDL.Completed += (sender, e) =>
            {
            };
            elevatorDoorL.BeginAnimation(WidthProperty, animationDL);
            elevatorDoorR.BeginAnimation(MarginProperty, animationDR);
            isDoorOpened = true;
        }

        void closeElevatorDoors()
        {
            DoubleAnimation animationDL = new DoubleAnimation(65, new Duration(TimeSpan.FromMilliseconds(300)));
            ThicknessAnimation animationDR = new ThicknessAnimation(new Thickness(66, elevatorDoorR.Margin.Top, 0, 0), new Duration(TimeSpan.FromMilliseconds(300)));
            animationDL.Completed += (sender, e) =>
            {
            };
            elevatorDoorL.BeginAnimation(WidthProperty, animationDL);
            elevatorDoorR.BeginAnimation(MarginProperty, animationDR);
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
            {
                if (person == confidentionPerson) cPersonFloor = chosenFloor;
                if (person == secretPerson) sPersonFloor = chosenFloor;
                if (person == topsecretPerson) tsPersonFloor = chosenFloor;
                movePersonLeftRight(-150, -74, person);
            }
            else
            {
                if (person == confidentionPerson) cPersonFloor = chosenFloor;
                if (person == secretPerson) sPersonFloor = chosenFloor;
                if (person == topsecretPerson) tsPersonFloor = chosenFloor;
                movePersonLeftRight(150, 74, person);
            }
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
                        confidentionPerson.Margin = new Thickness(elevator.Margin.Left + (dir + dir2), elevator.Margin.Top + 30, 0, 0);
                        if ((confidentionPerson.Margin.Left >= secretPerson.Margin.Left && confidentionPerson.Margin.Top >= secretPerson.Margin.Top) || (confidentionPerson.Margin.Left >= topsecretPerson.Margin.Left && confidentionPerson.Margin.Left >= topsecretPerson.Margin.Left))
                        {
                            if ((confidentionPerson.Margin.Left >= secretPerson.Margin.Left && confidentionPerson.Margin.Top >= secretPerson.Margin.Top) || (confidentionPerson.Margin.Left >= topsecretPerson.Margin.Left && confidentionPerson.Margin.Left >= topsecretPerson.Margin.Left))
                            {
                                confidentionPerson.Margin = new Thickness(confidentionPerson.Margin.Left + dir2, confidentionPerson.Margin.Top, 0, 0);
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
                        secretPerson.Margin = new Thickness(elevator.Margin.Left +(dir + dir2), elevator.Margin.Top + 30, 0, 0);
                        if ((secretPerson.Margin.Left >= confidentionPerson.Margin.Left && secretPerson.Margin.Top >= confidentionPerson.Margin.Top) || (secretPerson.Margin.Left >= topsecretPerson.Margin.Left && secretPerson.Margin.Left >= topsecretPerson.Margin.Left))
                        {
                            if ((secretPerson.Margin.Left >= confidentionPerson.Margin.Left && secretPerson.Margin.Top >= confidentionPerson.Margin.Top) || (secretPerson.Margin.Left >= topsecretPerson.Margin.Left && secretPerson.Margin.Left >= topsecretPerson.Margin.Left))
                            {
                                secretPerson.Margin = new Thickness(secretPerson.Margin.Left + dir2, secretPerson.Margin.Top, 0, 0);
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
                        topsecretPerson.Margin = new Thickness(elevator.Margin.Left + (dir + dir2), elevator.Margin.Top + 30, 0, 0);
                        if ((topsecretPerson.Margin.Left >= confidentionPerson.Margin.Left && topsecretPerson.Margin.Top >= confidentionPerson.Margin.Top) || (topsecretPerson.Margin.Left >= secretPerson.Margin.Left && topsecretPerson.Margin.Left >= secretPerson.Margin.Left))
                        {
                            if ((topsecretPerson.Margin.Left >= confidentionPerson.Margin.Left && topsecretPerson.Margin.Top >= confidentionPerson.Margin.Top) || (topsecretPerson.Margin.Left >= secretPerson.Margin.Left && topsecretPerson.Margin.Left >= secretPerson.Margin.Left))
                            {
                                topsecretPerson.Margin = new Thickness(topsecretPerson.Margin.Left + dir2, topsecretPerson.Margin.Top, 0, 0);
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