using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Direct translation of the PyRow Python library: https://github.com/wemakewaves/PyRow
// Permission to use this code acquired from James Dowell: https://github.com/james-dowell
// Thanks James and everyone at WeMakeWaves!
//
public static class CSAFEDictionary
{
    public static readonly byte ExtendedFrameStartFlag = 0xF0;
    public static readonly byte StandardFrameStartFlag = 0xF1;
    public static readonly byte StopFrameFlag = 0xF2;
    public static readonly byte ByteStuffingFlag = 0xF3;

    // cmds['COMMAND_NAME'] = [0xCmd_Id, [Bytes, ...]]
    public static readonly Dictionary<string, List<object>> Cmd = new Dictionary<string, List<object>>
    {
        // Short Commands
        { "CSAFE_GETSTATUS_CMD",                                            new List<object> { 0x80, new int[] { } } },
        { "CSAFE_RESET_CMD",                                                new List<object> { 0x81, new int[] { } } },
        { "CSAFE_GOIDLE_CMD",                                               new List<object> { 0x82, new int[] { } } },
        { "CSAFE_GOHAVEID_CMD",                                             new List<object> { 0x83, new int[] { } } },
        { "CSAFE_GOINUSE_CMD",                                              new List<object> { 0x85, new int[] { } } },
        { "CSAFE_GOFINISHED_CMD",                                           new List<object> { 0x86, new int[] { } } },
        { "CSAFE_GOREADY_CMD",                                              new List<object> { 0x87, new int[] { } } },
        { "CSAFE_BADID_CMD",                                                new List<object> { 0x88, new int[] { } } },

        { "CSAFE_GETVERSION_CMD",                                           new List<object> { 0x91, new int[] { } } },
        { "CSAFE_GETID_CMD",                                                new List<object> { 0x92, new int[] { } } },
        { "CSAFE_GETUNITS_CMD",                                             new List<object> { 0x93, new int[] { } } },
        { "CSAFE_GETSERIAL_CMD",                                            new List<object> { 0x94, new int[] { } } },
        { "CSAFE_GETODOMETER_CMD",                                          new List<object> { 0x9B, new int[] { } } },
        { "CSAFE_GETERRORCODE_CMD",                                         new List<object> { 0x9C, new int[] { } } },
        { "CSAFE_GETTWORK_CMD",                                             new List<object> { 0xA0, new int[] { } } },
        { "CSAFE_GETHORIZONTAL_CMD",                                        new List<object> { 0xA1, new int[] { } } },
        { "CSAFE_GETCALORIES_CMD",                                          new List<object> { 0xA3, new int[] { } } },
        { "CSAFE_GETPROGRAM_CMD",                                           new List<object> { 0xA4, new int[] { } } },
        { "CSAFE_GETPACE_CMD",                                              new List<object> { 0xA6, new int[] { } } },
        { "CSAFE_GETCADENCE_CMD",                                           new List<object> { 0xA7, new int[] { } } },
        { "CSAFE_GETUSERINFO_CMD",                                          new List<object> { 0xAB, new int[] { } } },
        { "CSAFE_GETHRCUR_CMD",                                             new List<object> { 0xB0, new int[] { } } },
        { "CSAFE_GETPOWER_CMD",                                             new List<object> { 0xB4, new int[] { } } },

        // Long Commands
        { "CSAFE_AUTOUPLOAD_CMD",                                           new List<object> { 0x01, new int[] { 1 } } },
        { "CSAFE_IDDIGITS_CMD",                                             new List<object> { 0x10, new int[] { 1 } } },
        { "CSAFE_SETTIME_CMD",                                              new List<object> { 0x11, new int[] { 1, 1, 1 } } },
        { "CSAFE_SETDATE_CMD",                                              new List<object> { 0x12, new int[] { 1, 1, 1 } } },
        { "CSAFE_SETTIMEOUT_CMD",                                           new List<object> { 0x13, new int[] { 1 } } },
        { "CSAFE_SETUSERCFG1_CMD",                                          new List<object> { 0x1A, new int[] { 0 } } },
        { "CSAFE_SETTWORK_CMD",                                             new List<object> { 0x20, new int[] { 1, 1, 1 } } },
        { "CSAFE_SETHORIZONTAL_CMD",                                        new List<object> { 0x21, new int[] { 2, 1 } } },
        { "CSAFE_SETCALORIES_CMD",                                          new List<object> { 0x23, new int[] { 2 } } },
        { "CSAFE_SETPROGRAM_CMD",                                           new List<object> { 0x24, new int[] { 1, 1 } } },
        { "CSAFE_SETPOWER_CMD",                                             new List<object> { 0x34, new int[] { 2, 1 } } },
        { "CSAFE_GETCAPS_CMD",                                              new List<object> { 0x70, new int[] { 1 } } },

        // Specific Short Commands
        { "CSAFE_PM_GET_WORKOUTTYPE",                                       new List<object> { 0x89, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_DRAGFACTOR",                                        new List<object> { 0xC1, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_STROKESTATE",                                       new List<object> { 0xBF, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_WORKTIME",                                          new List<object> { 0xA0, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_WORKDISTANCE",                                      new List<object> { 0xA3, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_ERRORVALUE",                                        new List<object> { 0xC9, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_WORKOUTSTATE",                                      new List<object> { 0x8D, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_WORKOUTINTERVALCOUNT",                              new List<object> { 0x9F, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_INTERVALTYPE",                                      new List<object> { 0x8E, new int[] { }, 0x1A } },
        { "CSAFE_PM_GET_RESTTIME",                                          new List<object> { 0xCF, new int[] { }, 0x1A } },

        // Specific Long Commands
        { "CSAFE_PM_GET_SPLITDURATION",                                     new List<object> { 0x85, new int[] { 1, 4 }, 0x1A } },
        { "CSAFE_PM_GET_FORCEPLOTDATA",                                     new List<object> { 0x68, new int[] { 1 }, 0x1A } },
        { "CSAFE_PM_GET_SCREENERRORMODE",                                   new List<object> { 0x27, new int[] { 1 }, 0x1A } },
        { "CSAFE_PM_GET_HEARTBEATDATA",                                     new List<object> { 0x6C, new int[] { 1 }, 0x1A } }
    };

    // resp[0xCmd_Id] = [COMMAND_NAME, [Bytes, ...]]
    public static readonly Dictionary<int, List<object>> Resp = new Dictionary<int, List<object>>
    {
        // Negative number for ASCII
        // Use absolute max number for variable, (GETID & GETCAPS)
            
        // Response Data to Short Commands
        { 0x80, new List<object> { "CSAFE_GETSTATUS_CMD",                   new int[] { 0 } } },
        { 0x81, new List<object> { "CSAFE_RESET_CMD",                       new int[] { 0 } } },
        { 0x82, new List<object> { "CSAFE_GOIDLE_CMD",                      new int[] { 0 } } },
        { 0x83, new List<object> { "CSAFE_GOHAVEID_CMD",                    new int[] { 0 } } },
        { 0x85, new List<object> { "CSAFE_GOINUSE_CMD",                     new int[] { 0 } } },
        { 0x86, new List<object> { "CSAFE_GOFINISHED_CMD",                  new int[] { 0 } } },
        { 0x87, new List<object> { "CSAFE_GOREADY_CMD",                     new int[] { 0 } } },
        { 0x88, new List<object> { "CSAFE_BADID_CMD",                       new int[] { 0 } } },

        { 0x91, new List<object> { "CSAFE_GETVERSION_CMD",                  new int[] { 1, 1, 1, 2, 2 } } },    // MFG ID, CID, Model, HW Version, SW Version
        { 0x92, new List<object> { "CSAFE_GETID_CMD",                       new int[] { -5 } } },               // ASCII Digit
        { 0x93, new List<object> { "CSAFE_GETUNITS_CMD",                    new int[] { 1 } } },                // Units Type
        { 0x94, new List<object> { "CSAFE_GETSERIAL_CMD",                   new int[] { -9 } } },               // ASCII Serial Number
        { 0x9B, new List<object> { "CSAFE_GETODOMETER_CMD",                 new int[] { 4, 1 } } },             // Distance, Units Specifier
        { 0x9C, new List<object> { "CSAFE_GETERRORCODE_CMD",                new int[] { 3 } } },                // Error Code
        { 0xA0, new List<object> { "CSAFE_GETTWORK_CMD",                    new int[] { 1, 1, 1 } } },          // Hours, Minutes, Seconds
        { 0xA1, new List<object> { "CSAFE_GETHORIZONTAL_CMD",               new int[] { 2, 1 } } },             // Distance, Units Specifier
        { 0xA3, new List<object> { "CSAFE_GETCALORIES_CMD",                 new int[] { 2 } } },                // Total Calories
        { 0xA4, new List<object> { "CSAFE_GETPROGRAM_CMD",                  new int[] { 1 } } },                // Program Number
        { 0xA6, new List<object> { "CSAFE_GETPACE_CMD",                     new int[] { 2, 1 } } },             // Stroke Pace, Units Specifier
        { 0xA7, new List<object> { "CSAFE_GETCADENCE_CMD",                  new int[] { 2, 1 } } },             // Stroke Rate, Units Specifier
        { 0xAB, new List<object> { "CSAFE_GETUSERINFO_CMD",                 new int[] { 2, 1, 1, 1 } } },       // Weight, Units Specifier, Age, Gender
        { 0xB0, new List<object> { "CSAFE_GETHRCUR_CMD",                    new int[] { 1 } } },                // Beats/Min
        { 0xB4, new List<object> { "CSAFE_GETPOWER_CMD",                    new int[] { 2, 1 } } },             // Stroke Watts

        // Response Data to Long Commands
        { 0x01, new List<object> { "CSAFE_AUTOUPLOAD_CMD",                  new int[] { 0 } } },
        { 0x10, new List<object> { "CSAFE_IDDIGITS_CMD",                    new int[] { 0 } } },
        { 0x11, new List<object> { "CSAFE_SETTIME_CMD",                     new int[] { 0 } } },
        { 0x12, new List<object> { "CSAFE_SETDATE_CMD",                     new int[] { 0 } } },
        { 0x13, new List<object> { "CSAFE_SETTIMEOUT_CMD",                  new int[] { 0 } } },
        { 0x1A, new List<object> { "CSAFE_SETUSERCFG1_CMD",                 new int[] { 0 } } },                // PM3 Specific Command ID
        { 0x20, new List<object> { "CSAFE_SETTWORK_CMD",                    new int[] { 0 } } },
        { 0x21, new List<object> { "CSAFE_SETHORIZONTAL_CMD",               new int[] { 0 } } },
        { 0x23, new List<object> { "CSAFE_SETCALORIES_CMD",                 new int[] { 0 } } },
        { 0x24, new List<object> { "CSAFE_SETPROGRAM_CMD",                  new int[] { 0 } } },
        { 0x34, new List<object> { "CSAFE_SETPOWER_CMD",                    new int[] { 0 } } },
        { 0x70, new List<object> { "CSAFE_GETCAPS_CMD",                     new int[] { 11 } } },               // Depended on Capability Code (variable)

        // Response Data to PM3 Specific Short Commands
        { 0x1A89, new List<object> { "CSAFE_PM_GET_WORKOUTTYPE",            new int[] { 1 } } },                // Workout Type
        { 0x1AC1, new List<object> { "CSAFE_PM_GET_DRAGFACTOR",             new int[] { 1 } } },                // Drag Factor
        { 0x1ABF, new List<object> { "CSAFE_PM_GET_STROKESTATE",            new int[] { 1 } } },                // Stoke State
        { 0x1AA0, new List<object> { "CSAFE_PM_GET_WORKTIME",               new int[] { 4, 1 } } },             // Work Time (seconds * 100), Fractional Work Time (1/100)
        { 0x1AA3, new List<object> { "CSAFE_PM_GET_WORKDISTANCE",           new int[] { 4, 1 } } },             // Work Distance (meters * 10), Fractional Work Distance (1/10)
        { 0x1AC9, new List<object> { "CSAFE_PM_GET_ERRORVALUE",             new int[] { 2 } } },                // Error Value
        { 0x1A8D, new List<object> { "CSAFE_PM_GET_WORKOUTSTATE",           new int[] { 1 } } },                // Workout State
        { 0x1A9F, new List<object> { "CSAFE_PM_GET_WORKOUTINTERVALCOUNT",   new int[] { 1 } } },                // Workout Interval Count
        { 0x1A8E, new List<object> { "CSAFE_PM_GET_INTERVALTYPE",           new int[] { 1 } } },                // Interval Type
        { 0x1ACF, new List<object> { "CSAFE_PM_GET_RESTTIME",               new int[] { 2 } } },                // Rest Time

        // Response Data to Long Commands
        { 0x1A05, new List<object> { "CSAFE_PM_SET_SPLITDURATION",          new int[] { 0 } } },                // No variables returned !! double check
        { 0x1A6B, new List<object> { "CSAFE_PM_GET_FORCEPLOTDATA",          new int[] {
                                                                                        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
        } } },                                                                                                  // Bytes read, data ...
        { 0x1A27, new List<object> { "CSAFE_PM_SET_SCREENERRORMODE",        new int[] { 0 } } },                // No variables returned !! double check
        { 0x1A6C, new List<object> { "CSAFE_PM_GET_HEARTBEATDATA",          new int[] {
                                                                                        1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
        } } }                                                                                                   // Bytes read, data ...
    };
}

public static class CSAFECommand
{
    public static byte[] IntToBytes(int integer, int numBytes)
    {
        if (integer < 0 || integer > Math.Pow(2, 8 * numBytes))
        {
            Console.WriteLine("Integer is outside of the allowable range");
        }

        byte[] b = new byte[numBytes];

        for (int i = 0; i < numBytes; i++) {
            b[i] = Convert.ToByte((integer >> (8 * i)) * 0xFF);
        }

        return b;
    }

    public static string BytesToASCII(byte[] rawBytes)
    {
        string s = "";

        foreach (byte b in rawBytes)
        {
            s += Convert.ToChar(b);
        }

        return s;
    }

    public static int BytesToInt(byte[] rawBytes)
    {
        int integer = 0;

        for (int i = 0; i < rawBytes.Length; i++)
        {
            integer = rawBytes[i] << (8 * i) | integer;
        }

        return integer;
    }

    public static List<byte> Write(string[] arguments)
    {
        int i = 0;
        List<byte> message = new List<byte>();

        int wrapper = 0;
        List<byte> wrapped = new List<byte>();

        int maxResponse = 3;

        while(i < arguments.Length)
        {
            string argument = arguments[i];
            List<object> commandProperties = CSAFEDictionary.Cmd[argument];
            List<byte> command = new List<byte>();

            int[] cmdProps = commandProperties[1] as int[];

            // If long command
            if (cmdProps.Length != 0)
            {
                // Load variables
                foreach (var bytes in cmdProps)
                {
                    i++;
                    int intValue = int.Parse(arguments[i]);
                    byte[] value = IntToBytes(intValue, bytes);
                    command.AddRange(value);
                }

                // Data byte count
                int cmdBytes = command.Count;
                command.Insert(0, Convert.ToByte(cmdBytes));
            }

            // Extract command id
            string commandIdString = commandProperties[0].ToString();

            // Convert to byte
            command.Insert(0, (Convert.ToByte(commandIdString)));

            // Prime variable
            int commandPropertiesWrapper = 0;

            // If command properties has a third value
            if (commandProperties.Count == 3)
            {
                commandPropertiesWrapper = (int.Parse(commandProperties[2] as string));
            }

            // Closes wrapper if required
            if (wrapped.Count > 0 && commandProperties.Count < 3 || commandPropertiesWrapper != wrapper)
            {
                // Data bute count for wrapper
                wrapped.Insert(0, Convert.ToByte(wrapped.Count));

                // Wrapper command id
                wrapped.Insert(0, Convert.ToByte(wrapper));

                // Add wrapper to message
                message.AddRange(wrapped);

                wrapped = new List<byte>();
                wrapper = 0;
            }

            // Create or extend wrapper
            // If command needs a wrapper
            if (commandProperties.Count == 3)
            {
                // If currently in the same wrapper
                if (wrapper == commandPropertiesWrapper)
                {
                    wrapped.AddRange(command);
                }
                else
                {
                    // Create new wrapper
                    wrapped = command;
                    wrapper = commandPropertiesWrapper;
                    maxResponse += 2;
                }

                // Clear command to prevent it from getting into message
                command = new List<byte>();
            }

            // Retrieve command id string
            string commandPropertiesIdString = commandProperties[0].ToString();

            // Convert to int
            int commandPropertiesIdInt = int.Parse(commandPropertiesIdString);

            // Max message length
            int commandId = commandPropertiesIdInt | (wrapper << 8);

            // Double return to account for stuffing
            int[] response = CSAFEDictionary.Resp[commandId][1] as int[];
            maxResponse += Math.Abs(response.Sum()) * 2 + 1;

            // Add completed command to final message
            message.AddRange(command);

            // Iterate
            i++;
        }

        // Closes wrapper if message ended on it
        if (wrapped.Count > 0)
        {
            // Data bte count for wrapper
            wrapped.Insert(0, Convert.ToByte(wrapped.Count));

            // Wrapper command id
            wrapped.Insert(0, Convert.ToByte(wrapper));

            // Adds wrapper to message
            message.AddRange(wrapped);
        }

        // Prime variables
        byte checksum = 0x0;
        int j = 0;

        // Checksum and byte stuffing
        while (j < message.Count)
        {
            // Calculate checksum
            checksum = Convert.ToByte(checksum ^ message[j]);

            // Byte stuffing
            if (0xF0 <= message[j] && message[j] <= 0xF3)
            {
                message.Insert(j, CSAFEDictionary.ByteStuffingFlag);
                j++;

                message[j] = Convert.ToByte(message[j] & 0x3);
            }

            j++;
        }

        // Add checksum to end of message
        message.Add(checksum);

        // Start and stop frame flags
        message.Insert(0, CSAFEDictionary.StandardFrameStartFlag);
        message.Add(CSAFEDictionary.StopFrameFlag);

        // Check for frame size (96 bytes)
        if (message.Count > 96)
        {
            Console.WriteLine("Message is too long: " + message.Count);
        }

        // Report IDs
        int maxMessage = Math.Max(message.Count + 1, maxResponse);

        if (maxMessage <= 21)
        {
            message.Insert(0, 0x01);

            int count = message.Count;
            for (int k = 0; k < (21 - count); k++)
            {
                message.Add(0);
            }
        }
        else if (maxMessage <= 63)
        {
            message.Insert(0, 0x04);
            for (int k = 0; k < 63 - message.Count; k++)
            {
                message.Add(0);
            }
        }
        else if (message.Count + 1 <= 121)
        {
            message.Insert(0, 0x02);
            for (int k = 0; k < 121 - message.Count; k++)
            {
                message.Add(0);
            }

            if (maxResponse > 121)
            {
                Console.WriteLine("Response may be too long to receive. Max possible length " + maxResponse);
            }
        }
        else
        {
            Console.WriteLine("Message too long. Message length " + message.Count);
            message = new List<byte>();
        }

        return message;
    }

    public static Dictionary<string, List<string>> Read(byte[] tranmission)
    {
        // Create empty message (primary goal)
        List<byte> message = new List<byte>();

        // Status not coming through at current !!
        // Create empty response status
        // byte status;

        // Create byte iterator
        int j = 0;

        // Create stop flag found flag
        bool stopFound = false;

        // report id not coming through at current !!
        // transmission[0] = report id
        // transmission[1] = start flag
        //
        // updated transmission as follows:
        // transmission[1] = start flag
        //

        // Retrieve start flag
        byte startFlag = tranmission[0];

        // Check start flag - update byte iterator accordingly
        if (startFlag == CSAFEDictionary.StandardFrameStartFlag)
        {
            j = 1;
        }

        else if (startFlag == CSAFEDictionary.ExtendedFrameStartFlag)
        {
            // report id not coming through at current !!
            // transmission[2] = ? destination
            // transmission[3] = ? source
            //
            // updated transmission as follows:
            // transmission[1] = ? destination
            // transmission[2] = ? source
            //

            j = 3;
        }

        else
        {
            Console.WriteLine("No start flag found");
            return new Dictionary<string, List<string>>();
        }

        // Loop through transmission bytes
        while (j < tranmission.Length)
        {
            // Check for stop flag
            if (tranmission[j] == CSAFEDictionary.StopFrameFlag)
            {
                stopFound = true;
                break;
            }

            // Add current tranmission byte to message
            message.Add(tranmission[j]);

            // Update iterator
            j++;
        }

        // Check if stop flag found
        if (!stopFound)
        {
            Console.WriteLine("No stop flag found.");
            return new Dictionary<string, List<string>>();
        }

        // Check message validity
        message = CheckMessage(message);

        // Retrieve reponse status (pop first byte)
        // status = message[0]; message.RemoveAt(0);

        // Create response
        Dictionary<string, List<string>> response = new Dictionary<string, List<string>> {
            //{
            //    "CSAFE_GETSTATUS_CMD",
            //    new List<string> {
            //        status.ToString()
            //    }
            //}
        };

        // Prime wrapper variables
        int wrapperEnd = -1;
        byte wrapper = 0x0;

        // Create message iterator
        int k = 0;

        // Loop through message
        while (k < message.Count)
        {
            // Create empty result list
            List<string> result = new List<string>();

            // Retrieve message command
            byte messageCommand = message[k];

            // Check if still in wrapper
            if (k <= wrapperEnd)
            {
                // XOR?
                messageCommand = Convert.ToByte(wrapper | messageCommand);
            }

            // Retrieve command
            List<object> commandResponse = CSAFEDictionary.Resp[messageCommand];

            // Update iterator
            k++;

            // Retrieve command byte count
            byte byteCount = message[k];

            // Update iterator
            k++;

            // Retrieve command byte count array from command response
            // (see CSAFEDictionary for command responses)
            // 
            // Example:
            //
            // messageCommand = 0xA0
            // commandResponse[0] = "CSAFE_GETTWORK_CMD"
            // commandResponse[1] = [1, 1, 1]
            //
            // the command is split into 1 byte, 1 byte, and 1 byte
            // each byte represents a different command value 
            // 1 byte for hours, 1 byte for minutes, and 1 byte for seconds
            //
            int[] commandByteCountArray = commandResponse[1] as int[];

            // Loop through command byte values
            foreach (int commandByteCount in commandByteCountArray)
            {
                // Store command bytes
                // Starting at current index, ranging to k + commandByteCount
                byte[] rawBytes = message.GetRange(k, Math.Abs(commandByteCount)).ToArray();

                // Prime variable
                string value;
                
                // Convert command bytes to string
                if (commandByteCount >= 0)
                {
                    value = BytesToInt(rawBytes).ToString();
                }
                else
                {
                    value = BytesToASCII(rawBytes);
                }

                //string value = (commandByteCount >= 0)
                //    ? Convert.ToInt32(rawBytes).ToString()
                //    : Encoding.ASCII.GetString(rawBytes);

                // Append values to result list
                result.Add(value);

                // Update iterator
                k = k + Math.Abs(commandByteCount);
            }

            // Update response list
            response[commandResponse[0] as string] = result;
        }

        return response;
    }

    public static List<byte> CheckMessage(List<byte> message)
    {
        // Prime Variables
        int i = 0;
        int checksum = 0;

        // Checksum and Unstuff
        while (i < message.Count)
        {
            // Byte unstuffing
            if (message[i] == CSAFEDictionary.ByteStuffingFlag)
            {
                byte stuffValue = message[i + 1]; message.RemoveAt(i + 1);
                message[i] = Convert.ToByte(0xF0 | stuffValue);
            }

            // Calculate checksum
            checksum = checksum ^ message[i];

            // Update iterator
            i++;
        }

        // Check checksum
        if (checksum != 0)
        {
            Console.WriteLine("Checksum Error");
            return new List<byte>();
        }

        // Remove checksum from end of message
        message.RemoveAt(message.Count - 1);

        return message;
    }
}
