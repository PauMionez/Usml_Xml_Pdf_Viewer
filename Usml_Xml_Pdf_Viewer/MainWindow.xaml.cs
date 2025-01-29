using System;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Mvvm;
using Usml_Xml_Pdf_Viewer.ViewModel;
using CefSharp.Wpf;

namespace Usml_Xml_Pdf_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
       
        public MainWindow()
        {
            InitializeComponent();
        }

        //private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var webBrowser = sender as WebBrowser;
        //    if (webBrowser != null)
        //    {
        //        // Execute JavaScript to get the visible portion of the document
        //        string visibleContent = webBrowser.InvokeScript("eval", @"
        //                    var visibleContent = document.documentElement;
        //                    var visibleHeight = window.innerHeight;
        //                    var scrollTop = window.scrollY;
        //                    var visibleContentPortion = visibleContent.innerHTML.slice(scrollTop, scrollTop + visibleHeight);
        //                    return visibleContentPortion;
        //                ").ToString();

        //        // Now visibleContent will hold the HTML portion that is currently visible
        //        Console.WriteLine(visibleContent);
        //    }
        //}

    }
}
