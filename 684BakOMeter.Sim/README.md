# Bak-O-Meter Hardware Simulator

A Windows Forms test application for simulating the Bak-O-Meter hardware over a serial port.

## Purpose

This simulator acts as a fake hardware sender, allowing you to manually test the main Bak-O-Meter application without the real NFC reader and load cell setup.

## Features

### 1. Serial Connection
- Configure COM port and baud rate (defaults to COM3 @ 9600 baud)
- Connect/Disconnect controls
- Real-time connection status display
- Message log with timestamps

### 2. Simulated NFC Tags
- 10 preset NFC tag buttons (realistic example UIDs)
- Quick-send buttons for common test scenarios
- Custom NFC tag input for manual testing

### 3. Weight Presets
Five calibrated weight presets with realistic randomization:
- **Nothing**: Base value 34,413 (±100)
- **Empty Glass**: Base value 77,670 (±150)
- **Full Glass**: Base value 92,366 (±200)
- **Empty Pul**: Base value 12,680 (±80)
- **Full Pul**: Base value 17,882 (±100)

Each click sends a slightly randomized value around the base for realistic simulation.

### 4. Continuous Mode
- Send "Nothing" values continuously at 100ms intervals
- Useful for baseline testing
- Checkbox toggle for easy control

### 5. Manual Send Controls
- Custom weight value input (numeric)
- Custom NFC tag input (text)
- Send buttons for manual testing

### 6. Chug Simulation
Simulates a realistic chug sequence for both glass and pul:

1. **Stable start**: Sends stable full weight values
2. **Removal**: Simulates glass removal for 1-5 seconds (random)
3. **Hard impact**: Creates a noticeable spike when placed back hard (~1 second)
4. **Settling**: Returns to stable empty weight values

This helps test validation logic that should ignore hard impacts when the glass is placed back.

- **Glass Chug**: Full Glass → Nothing → Impact Spike → Empty Glass
- **Pul Chug**: Full Pul → Nothing → Impact Spike → Empty Pul
- **Stop**: Immediately halt any running simulation

### 7. Message Log
- Real-time display of all sent messages
- Timestamped entries (HH:mm:ss.fff)
- Color-coded message types
- Clear button for log management
- Auto-scroll to latest messages

## Protocol Format

Messages are sent in `PREFIX:VALUE` format:

- **NFC/RFID tags**: `RFID:04A1B2C3D4E5F0`
- **Weight values**: `SCALE1:77670`

This matches the protocol parser in the main application (`ProtocolParser.cs`).

## Configuration

All constants are centralized in `SimulatorConfig.cs`:

- Calibration base values
- Randomization margins
- Simulation timing parameters
- NFC tag UIDs
- Protocol message format

Easy to adjust for testing different scenarios.

## Usage

1. **Start the simulator**
2. **Configure serial connection** (COM port and baud rate)
3. **Click Connect**
4. **Use any control** to send test messages:
   - Click NFC buttons to simulate tag scans
   - Click weight presets for instant weight values
   - Run chug simulations for full sequences
   - Send custom values for specific testing

5. **Monitor the log** to see exactly what was sent
6. **Disconnect** when done

## Tips

- **Before simulating a chug**, ensure the main app is ready to receive data
- **Use continuous mode** to maintain a baseline weight reading
- **Stop simulations** before disconnecting to avoid serial errors
- **Check the log** if messages aren't being received (confirms they were sent)

## Technical Details

- Built with Windows Forms (.NET 10)
- Uses `System.IO.Ports.SerialPort` for communication
- Asynchronous chug simulation with `Task` and `CancellationToken`
- Random seed per session for varied test data
- Comprehensive error handling and logging

## Future Enhancements

Potential features for extended testing:
- Save/load custom test sequences
- Playback recorded hardware sessions
- Multiple simultaneous device simulation
- Serial port auto-detection
- Message history replay

## License

Part of the 684-bak-o-meter project.
