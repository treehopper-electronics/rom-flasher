using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Treehopper;
using Treehopper.Libraries.Memory;

namespace rom_flasher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        SpiFlash mem;

        public async Task ReadData()
        {
            List<byte> bytes = new List<byte>();
            // read the entire 16 MB flash chip
            for (int i = 0; i < 16 * 1024 * 1024; i += 128)
            {
                if (i % 4096 == 0) // to keep things performant, don't print every time
                    Console.Write($"Reading address: 0x{i:X}...");
                var chunk = await mem.ReadArrayAsync(i, 128);
                bytes.AddRange(chunk);
            }

            File.WriteAllBytes("backup.bin", bytes.ToArray());
        }

        public async Task Run()
        {
var board = await ConnectionService.Instance.GetFirstDeviceAsync();
await board.ConnectAsync();
mem = new SpiFlash(board.Spi, board.Pins[3]);
var id = await mem.ReadJedecIdAsync();

            //int numBytes = 16 * 1024 * 1024; // 16 MB = 16777216 bytes

            //var data = new byte[numBytes];

            //for (int i = 0; i < numBytes; i += 128)
            //{
            //    if (i % 4096 == 0)
            //        Console.WriteLine(string.Format("Reading address: 0x{0:X}...", i));
            //    var chunk = await mem.ReadArray(i, 128);
            //    chunk.CopyTo(data, i);
            //}

            //File.WriteAllBytes("test.bin", data);

            Console.WriteLine("Erasing chip...");
            await mem.EraseChip();
            Console.WriteLine("Chip erased!");

            var status = await mem.ReadStatusAsync();

            await mem.WriteStatus1(0x00);
            await mem.WriteStatus2(0x00);

            status = await mem.ReadStatusAsync();

            var result = await mem.ReadArrayAsync(0, 128);

byte[] data = File.ReadAllBytes("image-to-write.bin");
byte[] chunk = new byte[128];
var start = DateTime.Now;
// read the entire 16 MB flash chip
for (int i = 0; i < data.Length; i += 128)
{
    if (i % 4096 == 0)
        Console.Write(string.Format("Writing address: 0x{0:X}...", i));

    Array.Copy(data, i, chunk, 0, 128);

    byte[] verifyData;
    int retries = -1;
    do
    {
        retries++;
        if (retries > 0)
        {
            Debug.WriteLine("Retrying...");
        }
        await mem.Write(chunk, i);
        verifyData = await mem.ReadArrayAsync(i, 128);
        for (int j = 0; j < chunk.Length; j++)
        {
            if (chunk[j] != verifyData[j])
            {
                Debug.WriteLine("Verify mismatch at index: " + j);
                break;
            }
        }
    }
    while (!chunk.SequenceEqual(verifyData));
    if (retries > 3)
    {
        Console.WriteLine("COULD NOT WRITE ADDRESS!!");
    }
    else
    {
        if (i % 4096 == 0)
            Console.WriteLine($"Verified! ({i * 100 / data.Length}% Complete)");
    }
            }

            Console.WriteLine("Finished in {0} seconds", (DateTime.Now - start).TotalSeconds);
        }
    }
}
