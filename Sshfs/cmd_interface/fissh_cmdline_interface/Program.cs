﻿/*
Copyright (c) 2014 bjoe-phi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/


// This Software is a interface to controll FiSSH over Command-Line


#define DEBUG



using System;
using System.Collections.Generic;
using fissh_cmdline_interface;


namespace fissh_cmdline_interface
{
    class Program
    {

        static int Main(string[] args)
        {

            return_error_codes return_value = return_error_codes.no_error;


            try
            {
                fissh_command_expression.parse(args);
            }
            catch (Exception e)
            {
                // if there is an error while parsing the arguments
                // programm will print an error message and syntax exmaples
                // exit code is 1
                fissh_cmdline_interface.fissh_print.wrong_use_error_message(e.Message);
                logger.log.writeLog(SimpleMind.Loglevel.Debug, "cmdline", e.Message );
                
#if DEBUG
                Console.ReadLine();
#endif

                return (int) return_error_codes.wrong_use_of_arguments;
            }




            try
            {
                switch (fissh_command_expression.keyword)
                {
                    case (byte)fissh_cmdline_interface.fissh_command_keywords.mount:
                        #region mount
                        // If user want to mount a registered server
                        if (!fissh_command_expression.parameter_host.is_set_flag)
                        {

                            // If user added a folderlist in parameter
                            if (fissh_command_expression.parameter_folderlist.is_set_flag)
                            {
                                fissh_cmdline_interface.actions.mount_registered_folders();
                            }
                            // If no folderlist is used
                            else
                            {
                                fissh_cmdline_interface.actions.mount_complete_server();
                            }
                        }
                        // If user want to mount a unregisteres Server
                        else
                        {
                            // If Port is not set, use Port 22
                            if (!fissh_command_expression.option_port.is_set_flag)
                            {
                                fissh_command_expression.option_port.set(22);
                            }

                            // If source path is not set, use /
                            if (!fissh_command_expression.option_path.is_set_flag)
                            {
                                fissh_command_expression.option_path.set("/");
                            }

                            //IF user is not set, use root
                            if (!fissh_command_expression.option_login_name.is_set_flag)
                            {
                                fissh_command_expression.option_login_name.set("root");
                            }

                            // If driveletter is not set, use Z:\
                            if (!fissh_command_expression.option_letter.is_set_flag
                                && !fissh_command_expression.option_virtual_drive.is_set_flag)
                            {
                                fissh_command_expression.option_letter.set("Z:");
                            }

                            // If no Authentification-Key is set
                            if (!fissh_command_expression.option_key.is_set_flag)
                            {
                                Console.Write("Enter Password >");
                                fissh_command_expression.option_key.set(Console.ReadLine());
                                fissh_command_expression.option_key.type = Sshfs.ConnectionType.Password;
                            }

                            actions.mount_unregistered_folder();

                        }
                        break;
                        #endregion

                    case (byte)fissh_cmdline_interface.fissh_command_keywords.umount:
                        #region umount
                        // If user wants to umount a simple drive
                        if (fissh_command_expression.option_letter.is_set_flag)
                        {
                            fissh_cmdline_interface.actions.umount_driveletter();
                        }

                        // If user wants to umount a virtual drive
                        else if (fissh_command_expression.option_virtual_drive.is_set_flag)
                        {
                            fissh_cmdline_interface.actions.umount_virtualdrive();
                        }

                        // If user wants to umount folders on a registered server
                        else if (fissh_command_expression.parameter_folderlist.is_set_flag)
                        {
                            fissh_cmdline_interface.actions.umount_registered_folders();
                        }

                        // If user wants to umount a complet registered server
                        else
                        {
                            fissh_cmdline_interface.actions.umount_complet_server();
                        }

                        break;
                        #endregion

                    case (byte)fissh_cmdline_interface.fissh_command_keywords.status:
                        #region status
                        Console.WriteLine("You ask for a status.");
                        break;
                        #endregion

                    case (byte)fissh_cmdline_interface.fissh_command_keywords.help:
                        #region help
                        Console.WriteLine("HELP!!!");
                        break;
                        #endregion

                    default:
                        throw new Exception("Unknown keyword");
                }
            }
            // error with IPC connection
            catch (System.ServiceModel.EndpointNotFoundException)
            {
                string error_message = "Cannot connect to IPC backend server, is fissh service running?";
                logger.log.writeLog(SimpleMind.Loglevel.Warning, "cmdline", error_message);
                fissh_print.simple_error_message(error_message);
                return_value = return_error_codes.ipc_error;
            }
            // wrong use of a argument
            catch (ArgumentException e)
            {
                string error_message = "Error in code: " + e.Message;
                logger.log.writeLog(SimpleMind.Loglevel.Error, "cmdline", error_message);
                fissh_print.simple_error_message(error_message);
                return_value = return_error_codes.error_in_code;
            }
            // Warnings
            catch (System.ComponentModel.WarningException e)
            {
                string error_message = e.Message;
                logger.log.writeLog(SimpleMind.Loglevel.Warning, "cmdline", error_message);
                fissh_print.simple_error_message(error_message);
                return_value = return_error_codes.common_warning;
            }
            catch (Exception e)
            {
                logger.log.writeLog(SimpleMind.Loglevel.Error, "cmdline", e.Message);
                fissh_print.simple_error_message(e.Message);
                return_value = return_error_codes.any_error;
            }



            // if programm could not mount or unmount every given drive
            if (!actions.no_mounting_error)
            {
                return_value = return_error_codes.could_not_mount_every_drive;
            }
 
#if DEBUG
            Console.WriteLine("DEBUGGING:");
            Console.WriteLine("Ended with return value " + (int)return_value + " - " + return_value + ".");
            Console.WriteLine("Press enter to close consol.");
            Console.ReadLine();
#endif

            return (int)return_value;
        }
        
        enum return_error_codes 
        {
            no_error = 0,
            wrong_use_of_arguments = 1,
            common_warning = 2,
            ipc_error = 3,
            could_not_mount_every_drive = 4,
            any_error = 244,
            error_in_code = 255    
        }

    }
}

