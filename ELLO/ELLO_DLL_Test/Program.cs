using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thorlabs.Elliptec.ELLO_DLL;

namespace ELLO_DLL_Test
{
	/// <summary>	Main test program. </summary>
	public class Program
	{
		/// <summary>	Main entry-point for this application. </summary>
		/// <param name="args">	Array of command-line argument strings. [port] [minAddress] [maxAddress] </param>
		static void Main(string[] args)
		{
			// get the communication port
			string port = (args.Length > 0) ? args[0] : "COM1";
			// get the range of addresses used max range is '0' to 'F'
			char _minSearchLimit = (args.Length > 1 && ELLBaseDevice.IsValidAddress(char.ToUpper(args[1][0]))) ?  char.ToUpper(args[1][0]) : '0';
			char _maxSearchLimit = (args.Length > 2 && ELLBaseDevice.IsValidAddress(char.ToUpper(args[2][0]))) ? char.ToUpper(args[2][0]) : 'F';

			// setup handler to display Transmitted data
			ELLDevicePort.DataSent += delegate(string str)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Tx: " + str);
				Console.ResetColor();
			};
			// setup handler to display Received data
			ELLDevicePort.DataReceived += delegate(string str)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("Rx: " + str);
				Console.ResetColor();
			};

			// create ELLDevices class to maintain the collection of Elliptec devices
			ELLDevices ellDevices = new ELLDevices();
			// setup handler to process device update messages
			ellDevices.MessageUpdates.OutputUpdate += delegate(List<string> str, bool error)
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						foreach (string s in str)
						{
							Console.WriteLine("Output->" + s);
						}
						Console.ResetColor();
					};

			// attempt to connect to the port
			if (ELLDevicePort.Connect(port))
			{
				Console.WriteLine("Discover devices");
				Console.WriteLine("================");
				// scan the port for connected devices using the given range of addresses
				List<string> devices = ellDevices.ScanAddresses(_minSearchLimit, _maxSearchLimit);

				foreach (string device in devices)
				{
					// configure each device found
					if (ellDevices.Configure(device))
					{
						// test each device found
						Console.WriteLine("");
						Console.WriteLine("Identify device " + device[0]);
						Console.WriteLine("=================");
						ELLDevice addressedDevice = ellDevices.AddressedDevice(device[0]) as ELLDevice;
						
						if (addressedDevice != null)
						{
							// display the device information
							DeviceID deviceInfo = addressedDevice.DeviceInfo;
							foreach (string str in deviceInfo.Description())
							{
								Console.WriteLine(str);
							}

                            // test each device according to type
                            // NOTE only a shutter and a Linear stage are shown in this example
                            Console.WriteLine("");
							Console.WriteLine("Test device " + device[0]);
							Console.WriteLine("=============");
						    switch (deviceInfo.DeviceType)
						    {
						        case DeviceID.DeviceTypes.Shutter:
						            // test the shutter device
						            addressedDevice.Home(ELLBaseDevice.DeviceDirection.Linear);
						            Thread.Sleep(250);
						            addressedDevice.JogForward();
						            Thread.Sleep(250);
						            addressedDevice.JogBackward();
						            Thread.Sleep(250);
						            break;
						        case DeviceID.DeviceTypes.Shutter4:
						            // test the shutter device
						            addressedDevice.Home(ELLBaseDevice.DeviceDirection.Linear);
						            Thread.Sleep(250);
						            addressedDevice.JogForward();
						            Thread.Sleep(250);
						            addressedDevice.JogForward();
						            Thread.Sleep(250);
						            addressedDevice.JogForward();
						            Thread.Sleep(250);
						            addressedDevice.JogBackward();
						            Thread.Sleep(250);
						            addressedDevice.JogBackward();
						            Thread.Sleep(250);
						            addressedDevice.JogBackward();
						            Thread.Sleep(250);
						            break;
						        case DeviceID.DeviceTypes.LinearStage:
						        case DeviceID.DeviceTypes.LinearStage2:
						        case DeviceID.DeviceTypes.LinearStage17:
						        case DeviceID.DeviceTypes.LinearStage20:
						            // Test the Linear stage

						            // for each motor ('1' and '2' get the motor information
						            for (char c = '1'; c <= '2'; c++)
						            {
						                if (addressedDevice.GetMotorInfo(c))
						                {
						                    MotorInfo motorInfo1 = addressedDevice[c];
						                    foreach (string s in motorInfo1.Description())
						                    {
						                        Console.WriteLine("Output->" + s);
						                    }
						                }
						            }

						            // Test the stage movement
						            addressedDevice.Home(ELLBaseDevice.DeviceDirection.Linear);
						            Thread.Sleep(250);
						            addressedDevice.SetJogstepSize(1.0m);
						            for (int i = 0; i < 10; i++)
						            {
						                addressedDevice.JogForward();
						                Thread.Sleep(100);
						            }
						            break;
						        default:
						            break;
						    }
						}
					}
				}

				ELLDevicePort.Disconnect();
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Port {0} unavailable", port);
				Console.ResetColor();
			}
			Console.WriteLine("Press any key to exit");
			Console.ReadKey(true);

		}
	}
}
