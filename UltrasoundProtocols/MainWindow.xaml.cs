﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProtocolTemplateLib;
using System.Threading;
using NLog;

namespace UltrasoundProtocols
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DataBaseConnector Connector { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Surname.DisplayMemberBinding = new Binding("LastName");
            Name.DisplayMemberBinding = new Binding("FirstName");
            MiddleName.DisplayMemberBinding = new Binding("MiddleName");
            id.DisplayMemberBinding = new Binding("NumberAmbulatoryCard");
            Gender.DisplayMemberBinding = new Binding("Gender");
            Birthday.DisplayMemberBinding = new Binding("Date");
            presenter = new EditPatientPresenter();
        }

        EditPatientPresenter presenter;
        private void listView_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            logger.Debug("Loading patients");
            (new GuiAsyncTask<List<Patient>>(() => presenter.LoadPatientListFromDataBase(),
                (patientList) =>
                {
                    foreach (Patient patient in patientList)
                    {
                        listView.Items.Add(patient);
                    }
                    this.IsEnabled = true;
                },
                () => Environment.Exit(1),
                true, "Ошибка загрузки пациентов", Dispatcher, logger, "Loading Patients")).Run();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                Patient selectedItem = (Patient)e.AddedItems[0];
                showController.FirstNameTextBlock.Text = selectedItem.FirstName;
                // TODO
                PatientColumn.Width = new GridLength(9, GridUnitType.Star);
            }
            else
            {
                // TODO
            }

        }

        private Logger logger = LogManager.GetCurrentClassLogger();
    }
}
