using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLE.iOS;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(BluetoothIos))]
namespace BLE.iOS
{


    class BluetoothIos : IBluetooth
    {
        public event EventHandler OnLeScan;

        public void StartLeClient()
        {
            CustomCBCentralManagerDelegate cbCentralManagerDelegate = new CustomCBCentralManagerDelegate();
            var myManager = new CBCentralManager(cbCentralManagerDelegate, DispatchQueue.CurrentQueue);
        }

        public void StartLeServer()
        {
            throw new NotImplementedException();
        }

        public void StartScan()
        {
            throw new NotImplementedException();
        }
    }
}