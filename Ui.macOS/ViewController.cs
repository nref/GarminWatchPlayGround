using System.Diagnostics;
using CoreBluetooth;
using CoreFoundation;
using Garmin.Ble.Lib;
using Garmin.Ble.Lib.Messages;
using ObjCRuntime;

namespace Ui.macOS;

public partial class ViewController : NSViewController {
    protected ViewController (NativeHandle handle) : base (handle)
    {
        // This constructor is required if the view controller is loaded from a xib or a storyboard.
        // Do not put any initialization here, use ViewDidLoad instead.
    }

    public override void ViewDidLoad ()
    {
        base.ViewDidLoad();

        IBleCentralManagerDelegate delgat = new BleCentralManagerDelegate();
        CBCentralManager mgr = null!;

        CBUUID serviceUuid = CBUUID.FromString(GarminConstants.UUID_SERVICE_GARMIN_3.ToString());
        CBUUID inCharsticUuid = CBUUID.FromString(GarminConstants.UUID_CHARACTERISTIC_GARMINFORERUNNER945LTE_GFDI_RECEIVE.ToString());
        CBUUID outCharsticUuid = CBUUID.FromString(GarminConstants.UUID_CHARACTERISTIC_GARMINFORERUNNER945LTE_GFDI_SEND.ToString());
        
        delgat.UpdatedState += (sender, args) =>
        {
            Debug.WriteLine($"Updated state to {args.Manager.State}");
            if (args.Manager.State != CBManagerState.PoweredOn) { return; }

            // On macOS and iOS, device MAC address is hidden. You can only scan by advertised service UUIDs.
            // Apple provides a temporary UUID to identify the device. It's meaningless outside the app.
            args.Manager.ScanForPeripherals(serviceUuid);
        };
        
        delgat.DiscoveredPeripheral += (sender, args) =>
        {
            if (mgr == null) { return; }
            
            Debug.WriteLine($"Discovered {args.Peripheral.Name} ({args.Peripheral.Identifier})");
            mgr.ConnectPeripheral(args.Peripheral, new PeripheralConnectionOptions
            {
                NotifyOnDisconnection = true,
            });
        };
        delgat.ConnectedPeripheral += (sender, args) =>
        {
            Debug.WriteLine($"Connected to {args.Peripheral.Name} ({args.Peripheral.Identifier})");

            args.Peripheral.UpdatedNotificationState += (_, cArgs) =>
            {
                Debug.WriteLine($"Notification state for characteristic {cArgs.Characteristic.UUID} is now {cArgs.Characteristic.IsNotifying}");
            };

            args.Peripheral.UpdatedValue += (sender, descriptorArgs) =>
            {
                if (descriptorArgs.Descriptor.Characteristic?.Value == null) { return; } 
                Debug.WriteLine($"Value changed to {string.Join(" ", descriptorArgs.Descriptor.Characteristic.Value.ToArray())}");
            };
            args.Peripheral.DiscoveredCharacteristics += (_, cArgs) =>
            {
                Debug.WriteLine($"Discovered characteristics for {cArgs.Service.UUID}");

                if (cArgs.Service.Characteristics == null) { return; }
                foreach (CBCharacteristic charstic in cArgs.Service.Characteristics)
                {
                    Debug.WriteLine($"  {charstic.UUID}");

                    if (charstic.UUID == inCharsticUuid)
                    {
                        Debug.WriteLine($"Requesting notifications for characteristic {charstic.UUID}");
                        args.Peripheral.SetNotifyValue(true, charstic);
                    }

                    if (charstic.UUID == outCharsticUuid)
                    {
                        args.Peripheral.WriteValue(NSData.FromArray(new InitConnectionMessage().Packet), charstic, CBCharacteristicWriteType.WithResponse);
                    }
                }
            };
            
            args.Peripheral.DiscoveredService += (_, _) =>
            {
                Debug.WriteLine($"Discovered services for {args.Peripheral.Name}");

                CBService? service = args.Peripheral.Services?.FirstOrDefault(s => s.UUID == serviceUuid);
                if (service == null) { return; }
                
                args.Peripheral.DiscoverCharacteristics(service);
            };
        };
        delgat.FailedToConnectPeripheral += (sender, args) => Debug.WriteLine($"Failed to connect to {args.Peripheral.Name} ({args.Peripheral.Identifier})");
        delgat.DisconnectedPeripheral += (sender, args) => Debug.WriteLine($"Disconnected from {args.Peripheral.Name} ({args.Peripheral.Identifier})");
        
        mgr = new CBCentralManager(delgat as BleCentralManagerDelegate, null);
        
        // Do any additional setup after loading the view.
    }

    public override NSObject RepresentedObject {
        get => base.RepresentedObject;
        set {
            base.RepresentedObject = value;

            // Update the view, if already loaded.
        }
    }
}
public interface IBleCentralManagerDelegate
{
    event EventHandler<CBWillRestoreEventArgs> WillRestoreState;
    event EventHandler<CBPeripheralErrorEventArgs> FailedToConnectPeripheral;
    event EventHandler<CBDiscoveredPeripheralEventArgs> DiscoveredPeripheral;
    event EventHandler<CBPeripheralErrorEventArgs> DisconnectedPeripheral;
    event EventHandler<CBCentralManagerEventArgs> UpdatedState;
    event EventHandler<CBPeripheralEventArgs> ConnectedPeripheral;
}

public class CBCentralManagerEventArgs : EventArgs
{
    public CBCentralManager Manager { get; set; }

    public CBCentralManagerEventArgs(CBCentralManager manager)
    {
        Manager = manager;
    }
}
public class BleCentralManagerDelegate : CBCentralManagerDelegate, IBleCentralManagerDelegate
{
    #region IBleCentralManagerDelegate events

    private event EventHandler<CBWillRestoreEventArgs> _willRestoreState;
    private event EventHandler<CBPeripheralErrorEventArgs> _failedToConnectPeripheral;
    private event EventHandler<CBDiscoveredPeripheralEventArgs> _discoveredPeripheral;
    private event EventHandler<CBPeripheralErrorEventArgs> _disconnectedPeripheral;
    private event EventHandler<CBCentralManagerEventArgs> _updatedState;
    private event EventHandler<CBPeripheralEventArgs> _connectedPeripheral;

    event EventHandler<CBWillRestoreEventArgs> IBleCentralManagerDelegate.WillRestoreState
    {
        add => _willRestoreState += value;
        remove => _willRestoreState -= value;
    }

    event EventHandler<CBPeripheralErrorEventArgs> IBleCentralManagerDelegate.FailedToConnectPeripheral
    {
        add => _failedToConnectPeripheral += value;
        remove => _failedToConnectPeripheral -= value;
    }

    event EventHandler<CBDiscoveredPeripheralEventArgs> IBleCentralManagerDelegate.DiscoveredPeripheral
    {
        add => _discoveredPeripheral += value;
        remove => _discoveredPeripheral -= value;
    }

    event EventHandler<CBPeripheralErrorEventArgs> IBleCentralManagerDelegate.DisconnectedPeripheral
    {
        add => _disconnectedPeripheral += value;
        remove => _disconnectedPeripheral -= value;
    }

    event EventHandler<CBCentralManagerEventArgs> IBleCentralManagerDelegate.UpdatedState
    {
        add => _updatedState += value;
        remove => _updatedState -= value;
    }

    event EventHandler<CBPeripheralEventArgs> IBleCentralManagerDelegate.ConnectedPeripheral
    {
        add => _connectedPeripheral += value;
        remove => _connectedPeripheral -= value;
    }

    #endregion

    #region Event wiring

    public override void WillRestoreState(CBCentralManager central, NSDictionary dict)
    {
        _willRestoreState?.Invoke(this, new CBWillRestoreEventArgs(dict));
    }

    public override void FailedToConnectPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
    {
        _failedToConnectPeripheral?.Invoke(this, new CBPeripheralErrorEventArgs(peripheral, error));
    }

    public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral,
        NSDictionary advertisementData, NSNumber RSSI)
    {
        _discoveredPeripheral?.Invoke(this,
            new CBDiscoveredPeripheralEventArgs(peripheral, advertisementData, RSSI));
    }

    public override void DisconnectedPeripheral(CBCentralManager central, CBPeripheral peripheral, NSError error)
    {
        _disconnectedPeripheral?.Invoke(this, new CBPeripheralErrorEventArgs(peripheral, error));
    }

    public override void UpdatedState(CBCentralManager central)
    {
        _updatedState?.Invoke(this, new CBCentralManagerEventArgs(central));
    }

    public override void ConnectedPeripheral(CBCentralManager central, CBPeripheral peripheral)
    {
        _connectedPeripheral?.Invoke(this, new CBPeripheralEventArgs(peripheral));
    }

    #endregion
}