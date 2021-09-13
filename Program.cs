using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.IO.Ports;
using System.Collections;


namespace x_IMU_Mouse
{
    class Program
    {
        public static SerialPort serialPort = new SerialPort("COM4", 115200);
        public static float[] q = new float[5]; //q0,q1,q2,q3,v //Data from IMU includes quternion (q0,q1,q2,q3) and voltage v
        public static float Voltage; //voltage value
        public static bool startMark = false; //flag of start running
        public static bool loseconnection = false; //flag of losing bluetooth connection
        public static int count_index = 0;
        public static double xangle = 0;
        public static double yangle = 0;
        public static double zangle = 0;
        public static double ComputeXAngle(float []q)
        {
            double sinr_cosp = 2 * (q[1] * q[0] + q[2] * q[3]);
            double cosr_cosp = 1 - 2 * (q[1] * q[1] + q[3] * q[3]);
            return Math.Atan2(sinr_cosp, cosr_cosp);
        }

        public static double ComputeZAngle(float[] q)
        {
            float sinp = 2 * (q[1] *q[2]  + q[3] * q[0]);
            if (Math.Abs(sinp) >= 1)
                return Math.PI / 2 * Math.Sign(sinp); // use 90 degrees if out of range
            else
                return Math.Asin(sinp);
        }

        public static double ComputeYAngle(float[] q)
        {
            float siny_cosp = 2 * (q[2] * q[0] - q[1] * q[3]);
            float cosy_cosp = 1 - 2 * (q[2] * q[2] + q[3] * q[3]);
            return Math.Atan2(siny_cosp, cosy_cosp);
        }

        static void UARTStreamer()
        {
            
            while (true)
            {
                count_index++;
                try
                {
                    int LineSep = serialPort.ReadChar();
                    //Each data package always starts with letter 'Q'
                    if (LineSep == 'Q')
                    {
                        Console.Write("Q");
                        int letter = serialPort.ReadChar(); //","//The second bit is ','
                        bool minus = false;
                        char buf;
                        int k = 0;
                        while (k < 5)//read in all five values q0, q1,q2,q3,v
                        {
                            minus = false;
                            q[k] = 0;
                            buf = (char)serialPort.ReadChar();//read q0
                           
                            if (buf == '-')//check if it is a negative number
                            {
                                minus = true; //negative number begins with '-'
                                buf = (char)serialPort.ReadChar();// read the char after '-'
                               
                            }
                            int d = buf - '0';//convert char to int
                            q[k] += d; //d is the ones place number
                            buf = (char)serialPort.ReadChar();//'.' decimal point
                           
                            for (int j = 1; j <= 3; j++) // three digits after the decimal point
                            {
                                //read in 3 chars and covert the string to a float value
                                buf = (char)serialPort.ReadChar();
                               
                                d = buf - '0';
                                q[k] += (float)(d / (Math.Pow(10, j)));
                            }
                            if (minus == true) //if negative value
                                q[k] *= -1;
                            k++;
                            if (k < 5)
                            {
                                //',' is the dividen char between each data
                                buf = (char)serialPort.ReadChar(); //','
                               
                            }
                            else
                                //this is the last char for line change
                                buf = (char)serialPort.ReadChar(); //'enter'
                           
                        }
                    }
                    //IMU to Unity coordinate conversion
              
                    Voltage = q[4];
                 
                        xangle = ComputeXAngle(q);
                        yangle = -ComputeYAngle(q);
                        zangle = ComputeZAngle(q);

                        var sx = string.Format("{0:0.00}", xangle);
                        Console.WriteLine(sx);
                        var sy = string.Format("{0:0.00}", yangle);
                        Console.WriteLine(sy);
                        var sz = string.Format("{0:0.00}", zangle);
                        Console.WriteLine(sz);

                        SendInputClass.MouseEvent((int)(SendInputClass.MOUSEEVENTF.ABSOLUTE | SendInputClass.MOUSEEVENTF.MOVE),
                                              (int)(32768.5f + ((-zangle * 5 / 3.1415) * 32768.5f)),
                                              (int)(32768.5f + ((-yangle * 5 / 3.1415) * 32768.5f)),
                                              0);
                   
                }
                catch (TimeoutException)
                {

                    loseconnection = true;
                    Console.WriteLine("TIMEOUT");

                }
                catch (FormatException)
                {
                    loseconnection = true;
                    Console.WriteLine("INCORRECT FORMAT");
                }
            }
        }
        static void OpenConnection()
        {

            if (serialPort != null)
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine("Closing port because it was already open!");
                }
                else
                {
                    serialPort.Open();
                    serialPort.ReadTimeout = 40; // 25Hz	
                    Console.WriteLine("Port opened");
                }
            }
            else
            {
                if (serialPort.IsOpen)
                {
                    Console.WriteLine("Port is already open!");
                }
                else
                {
                    Console.WriteLine("Port is null!");
                }
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");    
            // Connect to x-IMU
            OpenConnection();   
            UARTStreamer();
        }
    }
}