using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BLE
{
  public partial class MainPage : ContentPage
  {
    public MainPage()
    {
      InitializeComponent();
    }

    private void Server_Clicked(object sender, EventArgs e)
    {
      DependencyService.Get<IBluetooth>().StartLeServer();
    }
    private void Client_Clicked(object sender, EventArgs e)
    {
      DependencyService.Get<IBluetooth>().StartLeClient();
    }
    private void Scan_Clicked(object sender, EventArgs e)
    {
      DependencyService.Get<IBluetooth>().OnLeScan += MainPage_OnLeScan;
      DependencyService.Get<IBluetooth>().StartScan();
    }


    private void MainPage_OnLeScan(object sender, EventArgs e)
    {
      ResultLabel.Text = "new scan! ";
    }
  }
}
