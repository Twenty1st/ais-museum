using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class chooseDirectories
    {
        // Статические поля для хранения путей к директориям
        public static string dirImages = Directory.GetCurrentDirectory() + "\\images";
        public static string dirBackup = Directory.GetCurrentDirectory() + "\\backup";
        public static string dirCSV = Directory.GetCurrentDirectory() + "\\export csv data";
        public static string dirDocuments = Directory.GetCurrentDirectory() + "\\Документы и отчеты";

        // Метод для создания необходимых директорий
        public static void createDirectories()
        {
            // Инициализация переменной path значением dirImages
            string path = dirImages;

            // Проверяем, существует ли директория для изображений, если нет - создаем
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Обновляем path значением dirBackup
            path = dirBackup;

            // Проверяем, существует ли директория для резервных копий, если нет - создаем
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Обновляем path значением dirCSV
            path = dirCSV;

            // Проверяем, существует ли директория для CSV файлов, если нет - создаем
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Обновляем path значением dirDocuments
            path = dirDocuments;

            // Проверяем, существует ли директория для документов файлов, если нет - создаем
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
