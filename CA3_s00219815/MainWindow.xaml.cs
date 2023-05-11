using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace CA3_s00219815
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        public Booking CurrentBooking { get; set; }

        GameConsoleDB db = new GameConsoleDB();

        public MainWindow()
        {
            InitializeComponent();

        }


        private void Window_Load(object sender, RoutedEventArgs e)
        {
            var cbx = from c in db.Consoles
                      select c.Type;

            cbxConsoleType.ItemsSource = cbx.ToList().Distinct();

            DisplayAllConsoles();
            DisplayAllBookings();
        }

        private void DisplayAllConsoles()
        {
            var query = from c in db.Consoles
                        select c;
            dgCustomersEx2.ItemsSource = query.ToList();
        }

        private void DisplayAllBookings()
        {
            var query = from b in db.Bookings
                        select new { b.ID, b.StartDate, b.EndDate, ConsoleBrand = b.Console.Brand, ConsoleModel = b.Console.Model, ConsoleType = b.Console.Type };
            dgCustomersEx3.ItemsSource = query.ToList();
        }



        private void ShowAvailableConsoles()
        {

            string selectedConsoleType = cbxConsoleType.SelectedItem as string;
            DateTime startDate = dpStartDate.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = dpEndDate.SelectedDate ?? DateTime.MaxValue;

            var query = from c in db.Consoles
                        where c.Type.Equals(selectedConsoleType)
                        && !c.Bookings.Any(b => (b.StartDate <= endDate && b.EndDate >= startDate))
                        orderby c.Type descending
                        select new ConsoleInfo
                        {
                            ID = c.ID,
                            Brand = c.Brand,
                            Model = c.Model,
                            Type = c.Type
                        };

            dgConsolesAvailable.ItemsSource = query.ToList();
        }


        private void dgConsolesAvailable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgConsolesAvailable.SelectedItem is ConsoleInfo selectedConsole)
            {
                tblkSelectedConsole.Text = $"{selectedConsole.Brand}, {selectedConsole.Model} ({selectedConsole.Type})";
                CurrentBooking = new Booking { ConsoleID = selectedConsole.ID };

                // Load the image of the selected console
                string imagePath = $"Images/{selectedConsole.Model}.jpg";
                if (File.Exists(imagePath))
                {
                    imgSelectedConsole.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                }
                else
                {
                    imgSelectedConsole.Source = null;
                }
            }
        }


        private void cbxConsoleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAvailableConsoles();
        }

        private void dpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
            {
                if (dpStartDate.SelectedDate.Value.Date >= dpEndDate.SelectedDate.Value.Date)
                {
                    MessageBox.Show("Start date must be before end date.");
                    dpStartDate.SelectedDate = null;
                    return;
                }
            }

            if (dpStartDate.SelectedDate.HasValue)
            {
                if (CurrentBooking == null)
                    CurrentBooking = new Booking();

                CurrentBooking.StartDate = dpStartDate.SelectedDate.Value;
            }
        }

        private void dpEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpEndDate.SelectedDate.HasValue && dpStartDate.SelectedDate.HasValue)
            {
                if (dpEndDate.SelectedDate.Value.Date <= dpStartDate.SelectedDate.Value.Date)
                {
                    MessageBox.Show("End date must be after start date.");
                    dpEndDate.SelectedDate = null;
                    return;
                }
            }

            if (dpEndDate.SelectedDate.HasValue)
            {
                if (CurrentBooking == null)
                    CurrentBooking = new Booking();

                CurrentBooking.EndDate = dpEndDate.SelectedDate.Value;
            }
        }


        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBooking != null)
            {
                db.Bookings.Add(CurrentBooking);
                db.SaveChanges();
                DisplayAllBookings(); // refresh bookings data grid

                MessageBox.Show("The console has been booked!");
            }
            else
            {
                MessageBox.Show("Please select a console and date range first.");
            }
        }

        private void btnSearchConsoles_Click(object sender, RoutedEventArgs e)
        {
            ShowAvailableConsoles();
        }



    }
}

