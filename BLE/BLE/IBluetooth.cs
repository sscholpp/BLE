using System;
using System.Collections.Generic;
using System.Text;

namespace BLE
{
    public interface IBluetooth
    {

        void StartLeServer();
        void StartLeClient();
        void StartScan();

        event EventHandler OnLeScan;
    }
}
