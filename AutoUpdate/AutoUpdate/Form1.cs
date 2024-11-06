using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdate
{
    public partial class Form1 : Form
    {
        private const string CurrentVersion = "1.3"; 
        public Form1()
        {
            InitializeComponent();
            progressBar2.Visible = false; 
            CheckVersionAsync();
        }

        private async void CheckVersionAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    string versionUrl = "https://raw.githubusercontent.com/nomorelife1/AutoUpdate/refs/heads/main/Ver";
                    string onlineVersion = await client.GetStringAsync(versionUrl);
                    onlineVersion = onlineVersion.Trim(); 


                    if (onlineVersion == CurrentVersion)
                    {

                        Form2 form2 = new Form2();
                        form2.Show();
                        this.Hide();
                    }
                    else
                    {

                        var result = MessageBox.Show("A new update is available. Do you want to download it?",
                                                     "Update Available", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            progressBar2.Visible = true;
                            await DownloadUpdateAsync(onlineVersion);
                        }
                        else
                        {
                            Application.Exit(); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking version: {ex.Message}");
            }
        }


        private async Task DownloadUpdateAsync(string onlineVersion)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                  
                    string updateLinkUrl = "https://raw.githubusercontent.com/nomorelife1/AutoUpdate/refs/heads/main/Update";
                    string downloadUrl = await client.GetStringAsync(updateLinkUrl);
                    downloadUrl = downloadUrl.Trim(); 

              
                    string newFilePath = Path.Combine(Application.StartupPath, $"NoMoreLife.exe");

                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var totalRead = 0L;
                            var buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;

                                if (totalBytes != -1)
                                {
                                    
                                    int progress = (int)((totalRead * 100L) / totalBytes);
                                    progressBar2.Value = progress;
                                }
                            }
                        }
                    }

                    MessageBox.Show("Update downloaded successfully.");
                    ReplaceAndRestart(newFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading update: {ex.Message}");
            }
        }


        private void ReplaceAndRestart(string newFilePath)
        {
            string currentFilePath = Application.ExecutablePath;


            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                Arguments = $"/C choice /C Y /N /D Y /T 3 & Del \"{currentFilePath}\" & Start \"\" \"{newFilePath}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe"
            };
            Process.Start(startInfo);


            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}