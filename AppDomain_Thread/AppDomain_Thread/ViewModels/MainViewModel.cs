using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AppDomain_Thread.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private string _fromFileName = "";

        public string FromFileName
        {
            get { return _fromFileName; }
            set { Set(ref _fromFileName, value); }
        }


        private string _toFileName = "";

        public string ToFileName
        {
            get { return _toFileName; }
            set { Set(ref _toFileName, value); }
        }


        private int _progressBarValue;

        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set { Set(ref _progressBarValue, value); }
        }



        private int _progressBarMax;

        public int ProgressBarMax
        {
            get { return _progressBarMax; }
            set { Set(ref _progressBarMax, value); }
        }


        public bool isSuspend = false;

        public Thread? MainThread { get; set; }


        public RelayCommand OpenFileDialogCommandFrom
        {
            get => new(() =>
            {
                var fileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
                };

                var isOpen = fileDialog.ShowDialog();

                if (isOpen == true)
                {
                    FromFileName = fileDialog.FileName;
                }


            });
        }


        public RelayCommand OpenFileDialogCommandTo
        {
            get => new(() =>
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
                };

                var isOpen = fileDialog.ShowDialog();

                if (isOpen == true)
                {
                    ToFileName = fileDialog.FileName;
                }


            });
        }



        public RelayCommand ResumeProcessCommand
        {
            get => new(() =>
            {

                try
                {
                    MainThread?.Resume();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (isSuspend)
                    {
                        isSuspend = false;
                    }
                }

            });
        }



        public RelayCommand SuspendProcessCommand
        {
            get => new(() =>
            {
                try
                {
                    MainThread?.Suspend();
                    isSuspend = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
        }



        public RelayCommand AbortProcessCommand
        {
            get => new(() =>
            {
                try
                {
                    if (isSuspend)
                    {
                        MainThread?.Resume();
                    }

                    MainThread?.Abort();

                    ProgressBarValue = 0;
                    FromFileName = "";
                    ToFileName = "";

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (isSuspend)
                    {
                        isSuspend = false;
                    }
                }

            });
        }



        public RelayCommand CopyProcessCommand
        {
            get => new(() =>
            {
                bool isCopied = true;

                MainThread = new Thread
                (
                    () =>
                    {
                        try
                        {
                            using (FileStream stream = File.OpenRead(FromFileName))

                            using (FileStream writeStream = File.OpenWrite(ToFileName))
                            {
                                var reader = new BinaryReader(stream);
                                var writer = new BinaryWriter(writeStream);

                                byte[] buffer = new byte[1024];
                                int bytesRead;


                                var file = new FileInfo(FromFileName);
                                ProgressBarMax = Convert.ToInt32(file.Length);

                                while ((bytesRead = stream.Read(buffer, 0, 1024)) > 0)
                                {
                                    writeStream.Write(buffer, 0, bytesRead);

                                    ProgressBarValue += 1024;
                                    
                                    Thread.Sleep(1);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            isCopied = false;
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            if (isCopied)
                            {
                                Thread.Sleep(1000);
                            }

                            ProgressBarValue = 0;
                            FromFileName = "";
                            ToFileName = "";

                            if (isCopied)
                            {
                                MessageBox.Show("File successfully copied.");
                            }

                        }
                    }
                );
                if (isCopied)
                {
                    MainThread.Start();
                }
            });
        }

    }
}
