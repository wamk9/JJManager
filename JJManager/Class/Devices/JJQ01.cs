using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace JJManager.Class.Devices
{
    internal class JJQ01
    {
        private JJManager.Class.Device _device = null;
        private InTheHand.Net.BluetoothAddress _macAddress = null;
        private BluetoothClient _connection = null;
        private BluetoothEndPoint _endPoint = null;
        private Stream _stream = null;
        private JJManager.Pages.OtherDevices.JJQ01 form = null;
        private JJManager.Class.DatabaseConnection _databaseConnection = new JJManager.Class.DatabaseConnection();

        public JJManager.Class.Device Device {  get { return _device; } }

        public InTheHand.Net.BluetoothAddress MacAddress
        {
            get { return _macAddress; }
        }

        public JJQ01(JJManager.Class.Device device) 
        {
            _device = device;
            InTheHand.Net.BluetoothAddress _macAddress = _device.BtDevice.DeviceAddress;
        }

        public bool Connect()
        {
            if (_connection != null && _connection.Connected)
            {
                return true;
            }


            return _connection.Connected;
        }

        public void SendMessage(string message)
        {
            byte[] messageBuffer = Encoding.ASCII.GetBytes(message);


            try
            {
                _connection = new BluetoothClient();
                _endPoint = new BluetoothEndPoint(_device.BtDevice.DeviceAddress, BluetoothService.SerialPort);
                _connection.Connect(_endPoint);

                // Convert the string to bytes and write it to the stream
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

                using (_stream = _connection.GetStream())
                {
                    // Write the data to the stream
                    _stream.Write(buffer, 0, buffer.Length);

                    byte[] receiveBuffer = new byte[1024];
                    int bytesReceived = 0;
                    while (bytesReceived == 0)
                    {
                        bytesReceived = _stream.Read(receiveBuffer, 0, receiveBuffer.Length);
                        Thread.Sleep(50);
                    }

                }

                //Console.WriteLine("Data sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                if (_stream != null)
                {
                    // Close the stream and disconnect from the device
                    _stream.Close();
                    _stream.Dispose();

                }

                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
            }
        }

        public bool Disconnect()
        {
            _connection.Close();

            if (_connection.Connected)
            {
                return false;
            }
            else
            {
                _connection.Dispose();
                return true;
            }
        }

        public List<string> LoadFrames()
        {
            List<string> frames = new List<string>();

            string sql = "SELECT num_order, delay, info FROM dbo.frames WHERE id_profile = '" + _device.ActiveProfile.Id + "'";

            using (JsonDocument Json = _databaseConnection.RunSQLWithResults(sql))
            {
                if (Json != null)
                {
                    int row = 0;
                    int col = 0;

                    for (int i = 0; i < Json.RootElement.GetArrayLength(); i++)
                    {
                        row = 0;
                        col = 0;

                        foreach (string pixel in Json.RootElement[i].GetProperty("info").ToString().Split('-'))
                        {
                            JsonObject jsonToSend = new JsonObject
                            {
                                { "mode", "SET-PIXEL" },
                                { "info", 
                                    new JsonObject {
                                        { "page", 0 },
                                        { "row", row },
                                        { "col", col },
                                        { "color", pixel },
                                    } 
                                }
                            };

                            if (col < 16) // Increment the column until end row
                            {
                                col++;
                            }
                            else // Change to another row
                            {
                                row++;
                                col = 0;
                            }

                            frames.Add(JsonSerializer.Serialize(jsonToSend));
                        }
                    }
                        
                }

                return frames;
            }
        }

        public void SaveFrame(string numOrder, string delay, string frame)
        {
            string sql = "INSERT INTO dbo.frames (num_order, delay, info, id_profile) VALUES (" + numOrder + ", " + delay + ", '" + frame + "', " + _device.ActiveProfile.Id + ")";

            if (!_databaseConnection.RunSQL(sql))
            {
                Log.Insert("BluetoothDevices", "Erro ao salvar o frame do JJQ-01");
            }
        }

        public void OpenForm ()
        {
            form = new JJManager.Pages.OtherDevices.JJQ01(_device);
            if (form.InvokeRequired)
            {
                form.BeginInvoke((MethodInvoker)delegate
                {
                    form = new JJManager.Pages.OtherDevices.JJQ01(_device);
                    form.Show();
                });
            }
            else
            {
                form = new JJManager.Pages.OtherDevices.JJQ01(_device);
                form.Show();
            }

        }
    }
}
