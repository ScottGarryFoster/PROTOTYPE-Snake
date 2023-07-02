﻿using TestProjectCreator;

namespace EngineerTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            DataContext = new ProjectCreatorViewModel();
            InitializeComponent();
        }
    }
}