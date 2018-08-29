using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreBluetooth;
using Foundation;
using UIKit;

namespace BLE.iOS
{
    public class CustomCBCentralManagerDelegate : CBCentralManagerDelegate
    {
        public override void UpdatedState(CBCentralManager central)
        {
            if(central.State == CBCentralManagerState.PoweredOn)
            {
                CBUUID[] cbuuids = null;
                central.ScanForPeripherals(cbuuids);
            }
        }

        public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI)
        {
            var data = advertisementData;
            foreach(var v in data)
            {
                Console.WriteLine(v.Key + " " + v.Value);
            }
        }
    }
}