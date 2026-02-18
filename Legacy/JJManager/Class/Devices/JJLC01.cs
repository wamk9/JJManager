using HidSharp;
using JJManager.Class.App.Input;
using System.Linq;
using System.Text.Json.Nodes;
using System;
using System.Threading.Tasks;
using HIDClass = JJManager.Class.Devices.Connections.HID;
using System.Threading;
using System.Collections.Generic;

namespace JJManager.Class.Devices
{
    public class JJLC01 : HIDClass
    {
        private volatile bool _requesting = false;
        private volatile bool _sending = false;
        private readonly int _connectionTimeoutLimit = 2;
        private int _actualConnectionTimeout = 0;
        private App.Profile.Profile _profileInDevice = null;

        public JJLC01(HidDevice hidDevice) : base(hidDevice)
        {
            _cmds = new HashSet<ushort>
            {
                0x0001, // Set Fine Offset
                0x0002, // Set ADC Points
                0x0003, // Calibrate LoadCell
                0x0004, // Request Data
                0x00FF  // Device Info
            };

            RestartClass();
        }

        public void RestartClass()
        {
            _actionReceivingData = () => { Task.Run(async () => await ActionMainLoop()); };
            _actionSendingData = null; // Não usado - tudo em ActionMainLoop
            _actionResetParams = () => { Task.Run(() => RestartClass()); };
        }

        private async Task ActionMainLoop()
        {
            try
            {
                // Inicialização: cria perfil temporário e carrega dados do dispositivo
                if (_isConnected)
                {
                    // Create temporary profile (not saved to database)
                    _profile = new App.Profile.Profile(this, "Perfil Ativo Ao Conectar", true, true);
                    await RequestData(true);
                }

                while (_isConnected)
                {
                    // Envia dados apenas quando UI alterou algo (NeedsUpdate = true)
                    if (_profile.NeedsUpdate)
                    {
                        _profile.Restart();
                        await SendData();
                        _profile.NeedsUpdate = false;
                        await Task.Delay(1000);
                    }

                    // Requisita dados do dispositivo (pot_percent, kg_pressed, raw, etc.)
                    await RequestData();

                    // Delay de 50ms entre iterações (20 iterações/segundo)
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJLC01", "Erro no loop principal", ex);
            }
            finally
            {
                // Clear temporary profile data when disconnecting
                if (_profileInDevice != null && _profileInDevice.Id.StartsWith("temp_"))
                {
                    _profileInDevice.Update(new System.Text.Json.Nodes.JsonObject
                    {
                        { "data", new System.Text.Json.Nodes.JsonObject() }
                    });
                }

                Pages.Devices.JJLC01.NotifyDisconnectedDevice(_connId);
            }
        }

        private async Task SendData()
        {
            try
            {
                _sending = true;

                // Get values from profile (temporário em memória)
                if (_profile.Data.ContainsKey("jjlc01_data") && _profile.Data["jjlc01_data"] is JsonObject jjlc01Data)
                {
                    // Send Fine Offset
                    if (jjlc01Data.ContainsKey("fine_offset"))
                    {
                        int fineOffset = jjlc01Data["fine_offset"].GetValue<int>();

                        List<byte> fineOffsetData = new List<byte>
                        {
                            0x00, 0x01,          // CMD: 0x0001 (Set Fine Offset)
                            (byte)fineOffset,    // Fine Offset value (0-255, with offset +150)
                        };
                        await SendHIDBytes(fineOffsetData, false, 20, 2000, 5);
                    }

                    // Send ADC Points (11 floats = 44 bytes)
                    if (jjlc01Data.ContainsKey("adc"))
                    {
                        JsonArray adcArray = jjlc01Data["adc"].AsArray();

                        if (adcArray != null && adcArray.Count == 11)
                        {
                            List<byte> adcData = new List<byte>
                            {
                                0x00, 0x02  // CMD: 0x0002 (Set ADC Points)
                            };

                            // Add 11 floats (44 bytes)
                            foreach (var adcValue in adcArray)
                            {
                                float value = (float)adcValue.GetValue<double>();
                                byte[] floatBytes = BitConverter.GetBytes(value);
                                adcData.AddRange(floatBytes);
                            }

                            await SendHIDBytes(adcData, false, 20, 2000, 5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJLC01", "Erro ao enviar dados", ex);
            }
            finally
            {
                _sending = false;
            }
        }

        public async Task RequestData(bool sendToActualProfile = false)
        {
            try
            {
                _requesting = true;

                for (int i = 0; i < _connectionTimeoutLimit; i++)
                {
                    // Request all data (request type 0x06)
                    List<byte> requestData = new List<byte>
                    {
                        0x00, 0x04,  // CMD: 0x0004 (Request Data)
                        0x06,        // Request type: 0x06 = all data
                        0x20, 0x04   // FLAGS (0x20, CMD_L)
                    };

                    List<byte> response = await RequestHIDBytes(requestData, false, 10, 2000, 5);

                    // RequestHIDBytes already validates CMD and strips header/flags
                    // Response contains only payload: pot_percent(1) + kg_pressed(4) + raw(2) + fine_offset(1) + adc[11](44) = 52 bytes
                    if (response != null && response.Count >= 52)
                    {
                        int offset = 0;

                        // pot_percent (1 byte)
                        int potPercent = response[offset++];

                        // kg_pressed (4 bytes - float)
                        float kgPressed = BitConverter.ToSingle(response.ToArray(), offset);
                        offset += 4;

                        // raw (2 bytes - int16)
                        int raw = BitConverter.ToInt16(response.ToArray(), offset);
                        offset += 2;

                        // fine_offset (1 byte, with offset +150)
                        int fineOffset = response[offset++];

                        // adc array (44 bytes - 11 floats)
                        double[] adcValues = new double[11];
                        for (int j = 0; j < 11; j++)
                        {
                            adcValues[j] = BitConverter.ToSingle(response.ToArray(), offset);
                            offset += 4;
                        }

                        // Update UI - always update real-time values
                        Pages.Devices.JJLC01.UpdatePotPercent(potPercent);
                        Pages.Devices.JJLC01.UpdateKgPressed((double)kgPressed);
                        Pages.Devices.JJLC01.UpdateRawData(raw);

                        if (sendToActualProfile)
                        {
                            // Update UI with firmware data (chart and fine offset) only on first connection
                            // Mark as original firmware data so it can be restored when switching back to active profile
                            Pages.Devices.JJLC01.UpdateSeries(adcValues, true);
                            Pages.Devices.JJLC01.UpdateFineOffset(fineOffset, true);

                            JsonArray adcArray = new JsonArray();
                            foreach (double val in adcValues)
                            {
                                adcArray.Add(val);
                            }

                            JsonObject jsonToUpdate = new JsonObject
                            {
                                {
                                    "data", new JsonObject
                                    {
                                        {
                                            "jjlc01_data", new JsonObject
                                            {
                                                {"adc", adcArray },
                                                {"fine_offset", fineOffset }
                                            }
                                        }
                                    }
                                }
                            };

                            _profile.Update(jsonToUpdate);
                        }
                        else
                        {
                            // Update fine offset in UI during normal operation
                            Pages.Devices.JJLC01.UpdateFineOffset(fineOffset);
                        }

                        _actualConnectionTimeout = 0;
                        break;
                    }
                    else
                    {
                        _actualConnectionTimeout++;

                        if (_actualConnectionTimeout >= _connectionTimeoutLimit)
                        {
                            Disconnect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Insert("JJLC01", "Erro ao requisitar dados", ex);
            }
            finally
            {
                _requesting = false;
            }
        }

        // Method to calibrate loadcell from UI
        public async Task CalibrateLoadCell()
        {
            try
            {
                List<byte> calibrateData = new List<byte>
                {
                    0x00, 0x03,  // CMD: 0x0003 (Calibrate LoadCell)
                    0x20, 0x03   // FLAGS (0x20, CMD_L)
                };

                await SendHIDBytes(calibrateData, false, 0, 2000, 5);
            }
            catch (Exception ex)
            {
                Log.Insert("JJLC01", "Erro ao calibrar loadcell", ex);
            }
        }
    }
}
