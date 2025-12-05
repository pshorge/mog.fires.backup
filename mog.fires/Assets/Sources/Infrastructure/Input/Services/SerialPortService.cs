using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using Sources.Infrastructure.Configuration;

namespace Sources.Infrastructure.Input.Services
{
    /// <summary>
    /// Handles physical connection to the Serial Port.
    /// Runs a background thread for reading and marshals events to the Main Thread.
    /// </summary>
    public class SerialPortService : IStartable, IDisposable
    {
        public event Action<string> OnMessageReceived;

        private readonly AppConfig _config;
        private SerialPort _serialPort;
        private Thread _readThread;
        private bool _isRunning;
        private SynchronizationContext _mainThreadContext;

        public SerialPortService(AppConfig config)
        {
            _config = config;
        }

        public void Start()
        {
            if (!_config.Input.SerialEnabled)
            {
                Debug.Log("[SerialPortService] Disabled in config.");
                return;
            }

            // Capture main thread context to safely invoke events later
            _mainThreadContext = SynchronizationContext.Current;
            if (_mainThreadContext == null)
            {
                Debug.LogWarning("[SerialPortService] No SynchronizationContext found! Events might run on background thread.");
            }

            OpenPort();
        }

        private void OpenPort()
        {
            try
            {
                string portName = _config.Input.PortName;
                _serialPort = new SerialPort(portName, _config.Input.BaudRate)
                {
                    ReadTimeout = 100, // ms
                    NewLine = "\n",     // Standard arduino println
                };

                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                
                _isRunning = true;
                _readThread = new Thread(ReadLoop) { IsBackground = true };
                _readThread.Start();

                Debug.Log($"[SerialPortService] Connected to {portName} at {_config.Input.BaudRate}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SerialPortService] Failed to open port {_config.Input.PortName}: {e.Message}");
            }
        }

        private void ReadLoop()
        {
            while (_isRunning && _serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    // Blocking call (with timeout)
                    string message = _serialPort.ReadLine();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string cleanMessage = message.Trim();

                        // Marshal to Main Thread
                        if (_mainThreadContext != null)
                        {
                            _mainThreadContext.Post(_ => OnMessageReceived?.Invoke(cleanMessage), null);
                        }
                        else
                        {
                            // Fallback (dangerous for Unity API)
                            //OnMessageReceived?.Invoke(cleanMessage);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    // Normal behavior, just loop again
                }
                catch (Exception e)
                {
                    // Handle disconnection or other I/O errors
                    if (_isRunning) // Only log if we didn't intentionally close it
                        Debug.LogWarning($"[SerialPortService] Read error: {e.Message}");
                }
            }
        }

        public void Dispose()
        {
            _isRunning = false;

            // Give thread a moment to finish
            if (_readThread != null && _readThread.IsAlive)
            {
                _readThread.Join(200);
            }

            if (_serialPort != null && _serialPort.IsOpen)
            {
                try { _serialPort.Close(); } catch {}
                _serialPort.Dispose();
            }
            _serialPort = null;
        }
    }
}
