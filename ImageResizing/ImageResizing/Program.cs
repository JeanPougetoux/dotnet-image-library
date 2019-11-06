using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace ImageResizing
{
    class Program
    {
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
                new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
                { Endpoint = endpoint };
            return client;
        }

        private static Bitmap MergedBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width),
                                       bmp1.Height + bmp2.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp1, new Point(0, 0));
                g.DrawImage(bmp2, new Point(0, bmp2.Height));
            }
            return result;
        }

        private static void CropAndSave()
        {
            var access = @"C:\Users\formation\Pictures\dracaufeu.jpg";

            Bitmap image = new Bitmap(access);

            var width = image.Width;
            var height = image.Height;

            var pixelsSpan = height / 100 * 14;

            //var cropTopArea = new Rectangle(0, 0, width, pixelsSpan);
            var cropBottomArea = new Rectangle(0, height - pixelsSpan, width, pixelsSpan);

            //Bitmap top = image.Clone(cropTopArea,
            //image.PixelFormat);

            Bitmap bottom = image.Clone(cropBottomArea,
            image.PixelFormat);

            //Bitmap final = MergedBitmaps(top, bottom);
            bottom.Save(@"C:\Users\formation\Pictures\dracaufeuresized.jpg");
        }

        public static async Task ExtractTextLocal(ComputerVisionClient client, string localImage)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("EXTRACT TEXT - LOCAL IMAGE");
            Console.WriteLine();

            // Helps calucalte starting index to retrieve operation ID
            const int numberOfCharsInOperationId = 36;

            Console.WriteLine($"Extracting text from local image {Path.GetFileName(localImage)}...");
            Console.WriteLine();
            using (Stream imageStream = File.OpenRead(localImage))
            {
                // Read the text from the local image
                BatchReadFileInStreamHeaders localFileTextHeaders = await client.BatchReadFileInStreamAsync(imageStream);
                // Get the operation location (operation ID)
                string operationLocation = localFileTextHeaders.OperationLocation;

                // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
                string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                // Extract text, wait for it to complete.
                int i = 0;
                int maxRetries = 10;
                ReadOperationResult results;
                do
                {
                    results = await client.GetReadOperationResultAsync(operationId);
                    Console.WriteLine("Server status: {0}, waiting {1} seconds...", results.Status, i);
                    await Task.Delay(1000);
                    if (maxRetries == 9)
                    {
                        Console.WriteLine("Server timed out.");
                    }
                }
                while ((results.Status == TextOperationStatusCodes.Running ||
                        results.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries);

                // Display the found text.
                Console.WriteLine();
                var textRecognitionLocalFileResults = results.RecognitionResults;
                foreach (TextRecognitionResult recResult in textRecognitionLocalFileResults)
                {
                    foreach (Line line in recResult.Lines)
                    {
                        Console.WriteLine(line.Text);
                    }
                }
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY", EnvironmentVariableTarget.User);
            string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT", EnvironmentVariableTarget.User);
            ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
            ExtractTextLocal(client, @"C:\Users\formation\Pictures\dracaufeuredized.jpg").Wait();
        }
    }
}
