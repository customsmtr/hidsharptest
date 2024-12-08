using HidSharp;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace HidSharpTest
{
    public class CustomGamepad : IDisposable
    {
        private HidDevice device;
        private HidStream stream;
        private byte[] reportBuffer;
        private bool[] buttons = new bool[32];
        private bool isRunning = true;

        public void Initialize()
        {
            try
            {
                reportBuffer = new byte[5]; // 1 byte rapor ID + 4 byte buton durumları
                reportBuffer[0] = 0x01; // Rapor ID

                // Test modu - gerçek cihaz olmadan da çalışır
                #if DEBUG
                    return;
                #endif

                var deviceInfo = new HidDeviceLoader().GetDevices()
                    .FirstOrDefault(d => d.VendorID == 0x0483 && d.ProductID == 0x5750);

                if (deviceInfo == null)
                    throw new Exception("HID cihazı bulunamadı!");

                device = deviceInfo;
                stream = device.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Başlatma hatası: {ex.Message}");
            }
        }

        public void SetButton(int index, bool state)
        {
            if (index >= 0 && index < 32)
            {
                buttons[index] = state;
                UpdateReport();
            }
        }

        private void UpdateReport()
        {
            for (int i = 0; i < 4; i++)
            {
                byte buttonByte = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    if (buttons[i * 8 + bit])
                        buttonByte |= (byte)(1 << bit);
                }
                reportBuffer[i + 1] = buttonByte;
            }
        }

        public void StartReporting()
        {
            while (isRunning)
            {
                try
                {
                    #if !DEBUG
                        if (stream != null)
                            stream.Write(reportBuffer);
                    #endif
                    Thread.Sleep(10);
                }
                catch (Exception)
                {
                    // Hata durumunda sessizce devam et
                    Thread.Sleep(100);
                }
            }
        }

        public void Dispose()
        {
            isRunning = false;
            stream?.Dispose();
            device = null;
        }
    }
}
