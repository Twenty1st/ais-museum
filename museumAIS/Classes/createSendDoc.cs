using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class createSendDoc
    {
        public static void createDocument(Dictionary<string, string> replacementDict)
        {
            // Создаем новое приложение Word
            Application wordApp = new Application();
            Document wordDoc = null;

            try
            {
                // Открываем шаблон
                string templatePath = Directory.GetCurrentDirectory() + "\\docTemplate.docx";
                wordDoc = wordApp.Documents.Open(templatePath);

                // Заполняем шаблон данными из словаря replacementDict
                foreach (var replacement in replacementDict)
                {
                    ReplacePlaceholder(wordDoc, "<" + replacement.Key + ">", replacement.Value);
                }

                string savePath = chooseDirectories.dirDocuments + "\\Документ о передачи на " +
                    replacementDict["where"] + " (" + replacementDict["curEcspRegNum"] + ").docx";
                // Сохраняем новый документ
                wordDoc.SaveAs(savePath);
                // Открываем новый документ
                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (wordDoc != null)
                {
                    wordDoc.Close(false);
                }
                wordApp.Quit();
            }
            finally
            {
                // Освобождаем ресурсы
                if (wordDoc != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
        }
        private static void ReplacePlaceholder(Document doc, string placeholder, string newText)
        {
            foreach (Range tmpRange in doc.StoryRanges)
            {
                tmpRange.Find.ClearFormatting();
                tmpRange.Find.Execute(FindText: placeholder, ReplaceWith: newText, Replace: WdReplace.wdReplaceAll);
            }
        }
    }
}
