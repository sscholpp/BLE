using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BLE.Droid;
using Java.Util;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidBluetooth))]
namespace BLE.Droid
{

    class LeScanCallback : ScanCallback
    {
        public event EventHandler OnScanFinished;
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            OnScanFinished?.Invoke(this, new EventArgs());
        }
    }

    class LeServerCallback : BluetoothGattServerCallback
    {
        public delegate void OnDeviceAddedHandler(BluetoothDevice device);
        public event OnDeviceAddedHandler OnDeviceAdded;

        public delegate void OnDescriptorReadRequestHandler (BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor);
        public event OnDescriptorReadRequestHandler OnReadRequest;

        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            if (status == ProfileState.Connected)
                OnDeviceAdded?.Invoke(device);

            base.OnConnectionStateChange(device, status, newState);
        }

        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            OnReadRequest?.Invoke(device,requestId,offset,descriptor);
        }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            Console.WriteLine(device.Name);
        }
    }

    public class LeAdvertiseCallback : AdvertiseCallback
    {

    }


    class AndroidBluetooth : IBluetooth
    {
        BluetoothLeScanner bluetoothLeScanner;
        BluetoothManager bluetoothManager;
        BluetoothGattServer gattServer;

        public event EventHandler OnLeScan;

        LeServerCallback gattServerCallback;
        LeScanCallback scanCallback;

        /* Current Time Service UUID */
        public static UUID TIME_SERVICE = UUID.FromString("00001805-0000-1000-8000-00805f9b34fb");
        /* Mandatory Current Time Information Characteristic */
        public static UUID CURRENT_TIME = UUID.FromString("00002a2b-0000-1000-8000-00805f9b34fb");
        /* Optional Local Time Information Characteristic */
        public static UUID LOCAL_TIME_INFO = UUID.FromString("00002a0f-0000-1000-8000-00805f9b34fb");
        /* Mandatory Client Characteristic Config Descriptor */
        public static UUID CLIENT_CONFIG = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

        /* Bluetooth Weekday Codes */
        private const byte DAY_UNKNOWN = 0;
        private const byte DAY_MONDAY = 1;
        private const byte DAY_TUESDAY = 2;
        private const byte DAY_WEDNESDAY = 3;
        private const byte DAY_THURSDAY = 4;
        private const byte DAY_FRIDAY = 5;
        private const byte DAY_SATURDAY = 6;
        private const byte DAY_SUNDAY = 7;

        public AndroidBluetooth()
        {
            bluetoothManager = Xamarin.Forms.Forms.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            bluetoothLeScanner = bluetoothManager.Adapter.BluetoothLeScanner;


            gattServerCallback = new LeServerCallback();
            gattServerCallback.OnDeviceAdded += GattServerCallback_OnDeviceAdded;
            gattServerCallback.OnReadRequest += GattServerCallback_OnReadRequest;

            scanCallback = new LeScanCallback();
            scanCallback.OnScanFinished += Callback_OnScanFinished;
        }

        private void GattServerCallback_OnReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            if(gattServer != null)
            {
                gattServer.SendResponse(device, requestId, BluetoothGatt.GattSuccess, offset, null);
            }
        }

        private void GattServerCallback_OnDeviceAdded(BluetoothDevice device)
        {
            if(gattServer != null)
            {
                BluetoothGattCharacteristic timeCharacteristic = gattServer.Services.Where(s => s.Uuid == TIME_SERVICE).First().GetCharacteristic(CURRENT_TIME);
                timeCharacteristic.SetValue(GetExactTime());

                gattServer.NotifyCharacteristicChanged(device, timeCharacteristic, false);
            }
        }

        private byte[] GetExactTime()
        {
            Calendar time = Calendar.Instance;
            time.TimeInMillis = Java.Lang.JavaSystem.CurrentTimeMillis();

            byte[] field = new byte[10];

            // Year
            int year = time.Get(Calendar.Year);
            field[0] = (byte)(year & 0xFF);
            field[1] = (byte)((year >> 8) & 0xFF);
            // Month
            field[2] = (byte)(time.Get(Calendar.Month) + 1);
            // Day
            field[3] = (byte)time.Get(Calendar.Date);
            // Hours
            field[4] = (byte)time.Get(Calendar.HourOfDay);
            // Minutes
            field[5] = (byte)time.Get(Calendar.Minute);
            // Seconds
            field[6] = (byte)time.Get(Calendar.Second);
            // Day of Week (1-7)
            field[7] = GetDayOfWeekCode(time.Get(Calendar.DayOfWeek));
            // Fractions256
            field[8] = (byte)(time.Get(Calendar.Millisecond) / 256);
            // Reason is not used for now
            field[9] = 3;

            return field;
        }

        private byte GetDayOfWeekCode(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case Calendar.Monday:
                    return DAY_MONDAY;
                case Calendar.Tuesday:
                    return DAY_TUESDAY;
                case Calendar.Wednesday:
                    return DAY_WEDNESDAY;
                case Calendar.Thursday:
                    return DAY_THURSDAY;
                case Calendar.Friday:
                    return DAY_FRIDAY;
                case Calendar.Saturday:
                    return DAY_SATURDAY;
                case Calendar.Sunday:
                    return DAY_SUNDAY;
                default:
                    return DAY_UNKNOWN;
            }
        }

        private void Callback_OnScanFinished(object sender, EventArgs e)
        {
            OnLeScan?.Invoke(sender, e);
        }

        public void StartScan()
        {
            bluetoothLeScanner.StartScan(scanCallback);
        }

        public void StartLeServer()
        {
            gattServer = bluetoothManager.OpenGattServer(Xamarin.Forms.Forms.Context, gattServerCallback);
            if(gattServer != null)
            {
                gattServer.AddService(CreateTimeService());


                BluetoothLeAdvertiser leAdvertiser = bluetoothManager.Adapter.BluetoothLeAdvertiser;
                AdvertiseSettings settings = new AdvertiseSettings.Builder().SetAdvertiseMode(AdvertiseMode.Balanced)
                    .SetConnectable(true)
                    .SetTimeout(0)
                    .SetTxPowerLevel(AdvertiseTx.PowerHigh)
                    .Build();

                AdvertiseData data = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(true)
                    .SetIncludeTxPowerLevel(true)
                    .AddServiceUuid(new ParcelUuid(TIME_SERVICE))
                    .Build();

                leAdvertiser.StartAdvertising(settings, data, new LeAdvertiseCallback());
            }
        }

        public void StartLeClient()
        {
            
        }

        private BluetoothGattService CreateTimeService()
        {
            BluetoothGattService service = new BluetoothGattService(TIME_SERVICE, BluetoothGattService.ServiceTypePrimary);

            BluetoothGattCharacteristic currentTime = new BluetoothGattCharacteristic(CURRENT_TIME, GattProperty.Read | GattProperty.Notify, GattPermission.Read);
            BluetoothGattDescriptor configDescriptor = new BluetoothGattDescriptor(CLIENT_CONFIG, GattDescriptorPermission.Read | GattDescriptorPermission.Write);

            currentTime.AddDescriptor(configDescriptor);

            service.AddCharacteristic(currentTime);

            return service;

        }
    }
}