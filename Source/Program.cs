using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;

namespace MusicToVideo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to MusicToVideo!");
            Console.WriteLine("A CLI tool for converting music to video");
            Console.WriteLine();

            await CreateTestAnimation();
        }

        static async Task CreateTestAnimation()
        {
            try
            {
                var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test_animation.mp4");
                Console.WriteLine($"Creating test animation at: {outputPath}");
                Console.WriteLine("This may take a moment...");

                Console.WriteLine("Checking FFmpeg availability...");

                // Create a simple test video using a direct process call as fallback
                // Since FFMpegCore APIs seem to have issues, let's use Process directly
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-f lavfi -i color=c=blue:size=1280x720:duration=5 -c:v libx264 -r 30 -y \"{outputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Create tasks for reading output and error streams to prevent deadlock
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Add a timeout to prevent hanging indefinitely
                    var timeoutMs = 30000; // 30 seconds timeout
                    var processTask = process.WaitForExitAsync();

                    Console.WriteLine("Processing video...");

                    var completedTask = await Task.WhenAny(processTask, Task.Delay(timeoutMs));

                    if (completedTask == processTask)
                    {
                        // Process completed normally
                        var output = await outputTask;
                        var error = await errorTask;

                        if (process.ExitCode == 0 && File.Exists(outputPath))
                        {
                            Console.WriteLine($"Test animation created successfully: {outputPath}");

                            var fileInfo = new FileInfo(outputPath);
                            Console.WriteLine($"File size: {fileInfo.Length / 1024:F2} KB");

                            // Use FFProbe to analyze the created video
                            try
                            {
                                var videoInfo = await FFProbe.AnalyseAsync(outputPath);
                                Console.WriteLine($"Duration: {videoInfo.Duration:mm\\:ss}");
                                Console.WriteLine($"Resolution: {videoInfo.PrimaryVideoStream?.Width}x{videoInfo.PrimaryVideoStream?.Height}");
                                Console.WriteLine($"Frame rate: {videoInfo.PrimaryVideoStream?.FrameRate:F2} fps");
                            }
                            catch (Exception probeEx)
                            {
                                Console.WriteLine($"Could not analyze video info: {probeEx.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to create test animation.");
                            Console.WriteLine($"FFmpeg exit code: {process.ExitCode}");

                            var errorOutput = await errorTask;
                            if (!string.IsNullOrEmpty(errorOutput))
                            {
                                Console.WriteLine($"Error output: {errorOutput}");
                            }
                        }
                    }
                    else
                    {
                        // Process timed out
                        Console.WriteLine("FFmpeg process timed out. Attempting to kill process...");
                        try
                        {
                            process.Kill();
                            Console.WriteLine("Process killed due to timeout.");
                        }
                        catch (Exception killEx)
                        {
                            Console.WriteLine($"Could not kill process: {killEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating test animation: {ex.Message}");
                Console.WriteLine("Make sure FFmpeg is installed and available in your system PATH.");
                Console.WriteLine("Download FFmpeg from: https://ffmpeg.org/download.html");
                Console.WriteLine("You can also try installing via: winget install FFmpeg");
            }
        }
    }
}
