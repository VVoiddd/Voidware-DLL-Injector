using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;

namespace VoidwareDLLInjector
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Let's get the party started by populating our app list.
            PopulateOpenApplicationsAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Popping open the settings window. Gotta keep it stylish!
            var settingsWindow = new SettingsWindow { Owner = this };
            if (settingsWindow.ShowDialog() == true)
            {
                // Splash some color based on user's choice. Nice and easy.
                this.Background = new SolidColorBrush(settingsWindow.SelectedBackgroundColor);
            }
        }

        private async void PopulateOpenApplicationsAsync()
        {
            // Async magic to keep our UI snappy while fetching running apps.
            OpenApplicationsListBox.Items.Clear();
            var processes = await Task.Run(() => Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .Select(p => $"{p.MainWindowTitle} - PID: {p.Id}")
                .ToList());

            Dispatcher.Invoke(() =>
            {
                // Look at all those apps we found. Into the list, you go!
                foreach (var process in processes)
                {
                    OpenApplicationsListBox.Items.Add(process);
                }
            });
        }

        private void AddDllButton_Click(object sender, RoutedEventArgs e)
        {
            // Time for a DLL hunt. Open the dialog and let's find some treasure.
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll",
                Title = "Select a DLL file"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                // Found one? Great, let's remember that for later.
                QuickInjectListBox.Items.Add(openFileDialog.FileName);
            }
        }

        private void InjectDllButton_Click(object sender, RoutedEventArgs e)
        {
            // Before we do anything crazy, let's make sure everything's in order.
            if (OpenApplicationsListBox.SelectedItem == null || QuickInjectListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select both an application and a DLL.");
                return;
            }

            // Extracting the essence... I mean, process ID.
            string selectedItem = OpenApplicationsListBox.SelectedItem.ToString();
            int pid = int.Parse(selectedItem.Split('-').Last().Split(':').Last().Trim());
            string dllPath = QuickInjectListBox.SelectedItem.ToString();

            // Ah, found you. Now, stay still. This won't hurt a bit.
            Process targetProcess = Process.GetProcessById(pid);
            InjectDll(dllPath, targetProcess);
        }

        private void InjectDll(string dllPath, Process targetProcess)
        {
            // Alright, deep breath. Time to do some injection.
            IntPtr procHandle = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(dllPath), (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
            CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

            // Clean up after ourselves. No evidence left behind.
            CloseHandle(procHandle);
            MessageBox.Show($"Injected {dllPath} into {targetProcess.ProcessName}");
        }

        // The necessary dark arts to interact with the system at a lower level.
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
