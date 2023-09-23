using ArduinoUploader.Hardware;
using ArduinoUploader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJManager.Class
{
    internal class FirmwareUpdater
    {

        // const obj = new FirmewareUpdater("C:/melzinha.jpg", "com6", "Mixer de Áudio JJM-01");
        // const obj == objeto com atributos setados
        public FirmwareUpdater(string filePath, string comPort, string deviceName) 
        {
            var options = new ArduinoSketchUploaderOptions();
            options.FileName = filePath;
            options.PortName = comPort;

            switch (deviceName)
            {
                case "ButtonBox JJB-01":
                    options.ArduinoModel = ArduinoModel.Micro;
                    break;
                case "Mixer de Áudio JJM-01":
                    options.ArduinoModel = ArduinoModel.Micro;
                    break;
            }

            var uploader = new ArduinoSketchUploader(options);
            uploader.UploadSketch();
        }
    }
}
