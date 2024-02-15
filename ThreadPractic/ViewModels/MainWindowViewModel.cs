using ChatAppService.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Whatsapp.Commands;

namespace ThreadPractic.ViewModels
{
    public class MainWindowViewModel : ServiceINotifyPropertyChanged
    {
        private string fromPath;
        private string toPath;
        private long maximum;
        private long currentValue;

        public ICommand LoadFromCommand { get; set; }
        public ICommand LoadToCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand AbortCommand { get; set; }
        public ICommand ResumeCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public string FromPath { get => fromPath; set { fromPath = value; OnPropertyChanged(); } }
        public string ToPath { get => toPath; set { toPath = value; OnPropertyChanged(); } }
        public long Maximum { get => maximum; set { maximum = value; OnPropertyChanged(); } }
        public long CurrentValue { get => currentValue; set { currentValue = value; OnPropertyChanged(); } }

        public bool IsPaused { get; private set; }
        public bool IsAbort { get; private set; }
        public bool IsCopy { get; private set; }

        public MainWindowViewModel()
        {
            LoadFromCommand = new Command(ExecuteLoadFromCommand);
            LoadToCommand = new Command(ExecuteLoadToCommand);
            CopyCommand = new Command(ExecuteCopyCommand, CanExecuteCopyCommand);
            AbortCommand = new Command(ExecuteAbortCommand, CanExecuteAbortCommand);
            ResumeCommand = new Command(ExecuteResumeCommand, CanExecuteResumeCommand);
            PauseCommand = new Command(ExecutePauseCommand, CanExecutePauseCommand);
        }

        private bool CanExecutePauseCommand(object obj) =>
            !IsPaused && IsCopy;

        private void ExecutePauseCommand(object obj)=>
            IsPaused = true;

        private bool CanExecuteResumeCommand(object obj) =>
            IsPaused && IsCopy;
        private void ExecuteResumeCommand(object obj)=>
            IsPaused = false;
        private bool CanExecuteAbortCommand(object obj) =>
                !IsAbort && IsCopy;
        private void ExecuteAbortCommand(object obj)
        {
            IsAbort = true;
            IsPaused = false;
            IsCopy = false;
            CurrentValue = 0;
            
        }
        private bool CanExecuteCopyCommand(object obj) =>
            !string.IsNullOrEmpty(FromPath) && !string.IsNullOrEmpty(ToPath);

        private void ExecuteCopyCommand(object obj)
        {
            IsAbort = false;
            IsCopy = true;
            Thread th = new(CopyFile);
            th.Start();
           

        }
        private void ExecuteLoadToCommand(object obj) =>
            ToPath = LoadImage()!;

        private void ExecuteLoadFromCommand(object obj) =>
            FromPath = LoadImage()!;


        private string? LoadImage()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
                return fileDialog.FileName;
            return null;
        }



        private void CopyFile()
        {
            using (FileStream stream = new(FromPath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream writeStream = new(ToPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var length = stream.Length;
                    Maximum = stream.Length;
                    var readTemp = 10;
                    var buffer = new byte[readTemp];
                    CurrentValue = 0;

                    do
                    {
                        if (IsPaused)
                            continue;
                        if (IsAbort)
                            return;
                        var data = stream.Read(buffer, 0, readTemp);
                        writeStream.Write(buffer, 0, readTemp);
                        length -= readTemp;
                        CurrentValue += readTemp; ;
                    } while (length > 0);
                }
            };
            IsCopy = false;
            MessageBox.Show("Copy is Succsessful");
        }
    }
}
