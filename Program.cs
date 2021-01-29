using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YandexDisk.Client;
using YandexDisk.Client.Clients;
using YandexDisk.Client.Http;

namespace TestApp
{
    class Program
    {
        class UploadableFile
        {
            public readonly string Path;
            public bool Uploaded;
            public string Name { get { return Path.Split(@"\")[^1]; } }
            public UploadableFile(string path)
            {
                this.Path = path;
            }
        }

        private static List<UploadableFile> FileList;
        delegate void UploadStatusHandler();
        static event UploadStatusHandler Update;

        static async Task Main(string[] args)
        {
            //Yandex OAuth token
            string oauthToken = "AgAAAAADLLhRAADLW53pK9LvPUGEm0lezgsp4nA";

            var filesPath = args[0];

            FileList = new List<UploadableFile>();
            foreach (var path in Directory.GetFiles(filesPath))
            {
                FileList.Add(new UploadableFile(path));
            }

            var uploadingPath = args[1];

            Update += StatusTable;
            Update.Invoke();

            await UploadFilesAsync(oauthToken, uploadingPath);
        }

        private static void StatusTable()
        {
            Console.Clear();
            foreach (var file in FileList)
            {
                var uploadStatus = file.Uploaded ? "Загружено" : "Загрузка";
                Console.WriteLine($"{uploadStatus} {file.Name}");
            }

        }

        static async Task UploadFilesAsync(string oauthToken, string uploadingPath)
        {
            IDiskApi diskApi = new DiskHttpApi(oauthToken);
            List<Task> tasks = new List<Task>();

            foreach (var file in FileList)
            {
                var task = Task.Run( () => UploadAsync(diskApi, uploadingPath, file));
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        static async Task UploadAsync(IDiskApi diskApi, string uploadingPath, UploadableFile file)
        {
            await diskApi.Files.UploadFileAsync(path: uploadingPath + "/" + file.Name,
                                                overwrite: true,
                                                file: File.Open(file.Path, FileMode.OpenOrCreate),
                                                cancellationToken: CancellationToken.None);
            file.Uploaded = true;
            Update.Invoke();
        }
    }
}
